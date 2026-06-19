const colorbuttons = document.querySelectorAll(".Color_Button");

colorbuttons.forEach(button => {
    button.addEventListener("click", () => {
        colorbuttons.forEach(btn => {
            btn.classList.remove("active");
        });
        button.classList.add("active");
    });
});

const sizebuttons = document.querySelectorAll(".Size_Button");

sizebuttons.forEach(button => {
    button.addEventListener("click", () => {

        sizebuttons.forEach(btn => {
            btn.classList.remove("active");
        });

        button.classList.add("active");
    });
});

document.addEventListener("DOMContentLoaded", () => {

    const minusBtn = document.getElementById("minusBtn");
    const plusBtn = document.getElementById("plusBtn");
    const quantityInput = document.getElementById("quantity");

    plusBtn.addEventListener("click", () => {
        quantityInput.value = parseInt(quantityInput.value) + 1;
    });

    minusBtn.addEventListener("click", () => {
        if (parseInt(quantityInput.value) > 1) {
            quantityInput.value = parseInt(quantityInput.value) - 1;
        }
    });

});
