function updateSummary() {
    const items = document.querySelectorAll(".Bag_Product_Card");

    let itemCount = 0;
    let subtotal = 0;

    items.forEach(item => {
        const qtyElement = item.querySelector(".qty");
        if (qtyElement) {
            const qty = parseInt(qtyElement.innerText) || 0;
            const price = parseFloat(item.dataset.price) || 0;

            itemCount += qty;
            subtotal += qty * price;
        }
    });

    const shipping = subtotal > 0 ? 180 : 0;
    const discount = subtotal > 0 ? 50 : 0;
    const total = subtotal + shipping - discount;

    const totalCountLabel = document.querySelector(".sum-qty");
    if (totalCountLabel) totalCountLabel.innerText = itemCount;

    const subtotalLabel = document.querySelector(".sum-subtotal");
    if (subtotalLabel) subtotalLabel.innerText = "₱" + subtotal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    const shippingLabel = document.querySelector(".sum-shipping");
    if (shippingLabel) shippingLabel.innerText = "₱" + shipping.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    const discountLabel = document.querySelector(".sum-discount");
    if (discountLabel) discountLabel.innerText = "-₱" + discount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    const totalLabel = document.querySelector(".sum-total");
    if (totalLabel) totalLabel.innerText = "₱" + total.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

document.addEventListener("click", function (e) {
    const card = e.target.closest(".Bag_Product_Card");
    if (!card) return;

    const productId = card.dataset.id;
    const qtyEl = card.querySelector(".qty");

    if (e.target.closest(".qty-minus")) {
        let qty = parseInt(qtyEl.innerText) || 1;
        if (qty <= 1) return;
        qty -= 1;
        qtyEl.innerText = qty;
        updateSummary();

        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: qty })
        }).catch(err => console.error("Could not sync updated counter to database:", err));
    }

    if (e.target.closest(".qty-plus")) {
        let qty = parseInt(qtyEl.innerText) || 1;
        qty += 1;
        qtyEl.innerText = qty;
        updateSummary();

        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: qty })
        }).catch(err => console.error("Could not sync updated counter to database:", err));
    }

    if (e.target.closest(".delete-btn")) {
        card.style.transition = "all 0.2s ease-out";
        card.style.opacity = "0";
        card.style.transform = "translateX(20px)";
        
        setTimeout(() => {
            card.remove();
            updateSummary();
            
            const remainingCards = document.querySelectorAll(".Bag_Product_Card");
            if (remainingCards.length === 0) {
                const container = document.querySelector(".Bag_Product_Container");
                if (container) {
                    container.innerHTML = `<div style="text-align: center; padding: 100px 20px; width: 100%;"><p style="font-family: 'Nunito', sans-serif; font-size: 1.2rem; color: #888; font-weight: 600; margin: 0;">Your shopping bag is empty.</p></div>`;
                }
            }
        }, 200);

        fetch('/Cart/DeleteItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId })
        }).catch(err => console.error("Could not drop document item from cloud database:", err));
    }
});