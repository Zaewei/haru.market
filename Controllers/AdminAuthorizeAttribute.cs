using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// It should inherit from ActionFilterAttribute, NOT Controller
public class AdminAuthorizeAttribute : ActionFilterAttribute 
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Your existing check logic
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}