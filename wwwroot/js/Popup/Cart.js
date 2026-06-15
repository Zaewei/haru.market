function updateSummary() {

    const items = document.querySelectorAll(".Bag_Product_Container");

    let itemCount = 0;
    let subtotal = 0;

    items.forEach(item => {

        const qty = parseInt(item.querySelector(".qty").innerText);
        const price = parseFloat(item.dataset.price);

        itemCount += qty;
        subtotal += qty * price;
    });

    const shipping = subtotal > 0 ? 180 : 0;
    const discount = 50; // you can later make dynamic
    const total = subtotal + shipping - discount;

    // UPDATE UI
    document.querySelector(".Right_Table tr:nth-child(1) th").innerText = itemCount;
    document.querySelector(".Right_Table tr:nth-child(2) th").innerText = subtotal.toFixed(2);
    document.querySelector(".Right_Table tr:nth-child(3) th").innerText = shipping.toFixed(2);
    document.querySelector(".Right_Table tr:nth-child(4) th").innerText = discount.toFixed(2);
    document.querySelector(".Right_Table tr:nth-child(5) th").innerText = total.toFixed(2);
}

document.addEventListener("click", function (e) {

    const card = e.target.closest(".Bag_Product_Container");
    if (!card) return;

    const qtyEl = card.querySelector(".qty");

    // ➖
    if (e.target.closest(".qty-minus")) {
        let qty = parseInt(qtyEl.innerText);
        qty = Math.max(1, qty - 1);
        qtyEl.innerText = qty;

        updateSummary();
    }

    // ➕
    if (e.target.closest(".qty-plus")) {
        let qty = parseInt(qtyEl.innerText);
        qty += 1;
        qtyEl.innerText = qty;

        updateSummary();
    }

    // 🗑 DELETE
    if (e.target.closest(".delete-btn")) {
        card.remove();
        updateSummary();
    }
});