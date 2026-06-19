using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace haru.market.Services
{
    // A single courier supported by TrackingMore (e.g. "jtexpress-ph" / "J&T Express Philippines")
    public class TrackingMoreCourier
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // The bits of a TrackingMore "tracking" object.
    public class TrackingMoreResult
    {
        public string? Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string CourierCode { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }

        // One of: pending, notfound, transit, pickup, undelivered, delivered, exception, expired
        public string DeliveryStatus { get; set; } = "pending";
        public string? Substatus { get; set; }
    }

    public class TrackingMoreService
    {
        private const string BaseUrl = "https://api.trackingmore.com/v4";

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // Courier list rarely changes, so we cache it in memory for a while instead of
        // hitting the API every time the admin opens the "Edit Order" modal.
        private List<TrackingMoreCourier>? _courierCache;
        private DateTime _courierCacheExpiry = DateTime.MinValue;

        public TrackingMoreService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        private string ApiKey => _configuration["TrackingMore:ApiKey"] ?? string.Empty;

        private HttpRequestMessage BuildRequest(HttpMethod method, string pathAndQuery, object? jsonBody = null)
        {
            var request = new HttpRequestMessage(method, $"{BaseUrl}/{pathAndQuery}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("Tracking-Api-Key", ApiKey);

            if (jsonBody != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");
            }

            return request;
        }

        private static int GetMetaCode(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("meta", out var meta) && meta.TryGetProperty("code", out var metaCode) && metaCode.ValueKind == JsonValueKind.Number)
                    return metaCode.GetInt32();

                if (root.TryGetProperty("code", out var rootCode) && rootCode.ValueKind == JsonValueKind.Number)
                    return rootCode.GetInt32();
            }

            return 0;
        }

        private static string GetMetaMessage(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("meta", out var meta) && meta.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    return msg.GetString() ?? "Unknown TrackingMore error.";

                if (root.TryGetProperty("message", out var rootMsg) && rootMsg.ValueKind == JsonValueKind.String)
                    return rootMsg.GetString() ?? "Unknown TrackingMore error.";
            }

            return "Unknown TrackingMore error.";
        }

        private static string GetStringField(JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        private static TrackingMoreResult ParseTrackingData(JsonElement data)
        {
            string deliveryStatus = GetStringField(data, "delivery_status");

            return new TrackingMoreResult
            {
                Id = GetStringField(data, "id"),
                TrackingNumber = GetStringField(data, "tracking_number"),
                CourierCode = GetStringField(data, "courier_code"),
                OrderNumber = GetStringField(data, "order_number"),
                DeliveryStatus = string.IsNullOrWhiteSpace(deliveryStatus) ? "pending" : deliveryStatus,
                Substatus = GetStringField(data, "substatus")
            };
        }

        // Creates (or, in TrackingMore's V4 wording, "creates & gets") a tracking.
        // order_number is set to our Firestore order ID so webhooks can map straight back to it.
        public async Task<TrackingMoreResult> CreateTrackingAsync(string trackingNumber, string courierCode, string? orderNumber = null, string? customerName = null)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("TrackingMore API key is not configured. Add a 'TrackingMore:ApiKey' entry to appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(trackingNumber) || string.IsNullOrWhiteSpace(courierCode))
            {
                throw new Exception("A tracking number and courier are both required to create a TrackingMore tracking.");
            }

            var payload = new Dictionary<string, object?>
            {
                { "tracking_number", trackingNumber.Trim() },
                { "courier_code", courierCode.Trim() }
            };

            if (!string.IsNullOrWhiteSpace(orderNumber)) payload["order_number"] = orderNumber;
            if (!string.IsNullOrWhiteSpace(customerName)) payload["customer_name"] = customerName;

            using var request = BuildRequest(HttpMethod.Post, "trackings/create", payload);
            using var response = await _httpClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;
            int code = GetMetaCode(root);

            // 4101 = "Tracking No. already exists" -- this happens if the order is re-saved as
            // Shipped, or the same tracking number was used before. Treat it as a success and
            // just fetch the existing tracking instead of failing the request.
            if (code == 4101)
            {
                var existing = await GetTrackingAsync(trackingNumber, courierCode);
                if (existing != null) return existing;
            }

            if (code != 200 || !root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                throw new Exception($"TrackingMore could not create the tracking: {GetMetaMessage(root)}");
            }

            return ParseTrackingData(data);
        }

        // Pulls the latest known status for a tracking number directly (used by the
        // admin "Refresh tracking" button, independently of webhooks).
        public async Task<TrackingMoreResult?> GetTrackingAsync(string trackingNumber, string courierCode)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("TrackingMore API key is not configured. Add a 'TrackingMore:ApiKey' entry to appsettings.json.");
            }

            string query = $"trackings/get?tracking_numbers={Uri.EscapeDataString(trackingNumber.Trim())}&courier_code={Uri.EscapeDataString(courierCode.Trim())}";

            using var request = BuildRequest(HttpMethod.Get, query);
            using var response = await _httpClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            if (GetMetaCode(root) != 200 || !root.TryGetProperty("data", out var data))
            {
                return null;
            }

            if (data.ValueKind == JsonValueKind.Array)
            {
                if (data.GetArrayLength() == 0) return null;
                return ParseTrackingData(data[0]);
            }

            if (data.ValueKind == JsonValueKind.Object)
            {
                return ParseTrackingData(data);
            }

            return null;
        }

        // Returns the full list of couriers TrackingMore supports, so the admin can pick one
        // from a dropdown instead of guessing courier codes.
        public async Task<List<TrackingMoreCourier>> GetAllCouriersAsync()
        {
            if (_courierCache != null && DateTime.UtcNow < _courierCacheExpiry)
            {
                return _courierCache;
            }

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("TrackingMore API key is not configured. Add a 'TrackingMore:ApiKey' entry to appsettings.json.");
            }

            using var request = BuildRequest(HttpMethod.Get, "couriers/all");
            using var response = await _httpClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            var couriers = new List<TrackingMoreCourier>();

            if (GetMetaCode(root) == 200 && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    string code = GetStringField(item, "courier_code");
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    string name = GetStringField(item, "courier_name");
                    couriers.Add(new TrackingMoreCourier { Code = code, Name = string.IsNullOrWhiteSpace(name) ? code : name });
                }
            }

            couriers = couriers.OrderBy(c => c.Name).ToList();
            _courierCache = couriers;
            _courierCacheExpiry = DateTime.UtcNow.AddHours(6);
            return couriers;
        }

        // Suggests likely couriers based on the tracking number's format. Handy because most
        // admins don't memorize courier codes -- they just paste the tracking number.
        public async Task<List<TrackingMoreCourier>> DetectCourierAsync(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                return new List<TrackingMoreCourier>();
            }

            var payload = new Dictionary<string, object?> { { "tracking_number", trackingNumber.Trim() } };

            using var request = BuildRequest(HttpMethod.Post, "couriers/detect", payload);
            using var response = await _httpClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            var matches = new List<TrackingMoreCourier>();

            if (GetMetaCode(root) == 200 && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    string code = GetStringField(item, "courier_code");
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    string name = GetStringField(item, "courier_name");
                    matches.Add(new TrackingMoreCourier { Code = code, Name = string.IsNullOrWhiteSpace(name) ? code : name });
                }
            }

            return matches;
        }
    }
}
