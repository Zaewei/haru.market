
document.addEventListener("DOMContentLoaded", () => {

    setupPopup("searchBtn", "searchPopup", "closeSearch");
    setupPopup("heartBtn", "heartPopup", "closeHeart");
    setupPopup("bagBtn", "bagPopup", "closeBag");
    setupPopup("userBtn", "userPopup", "closeUser");

});

function setupPopup(buttonId, popupId, closeId) {

    const button = document.getElementById(buttonId);
    const popup = document.getElementById(popupId);
    const close = document.getElementById(closeId);

    if (!button || !popup || !close) return;

    button.addEventListener("click", (e) => {
        e.preventDefault();
        popup.style.display = "flex";
    });

    close.addEventListener("click", () => {
        popup.style.display = "none";
    });
}