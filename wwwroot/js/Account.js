document.addEventListener("DOMContentLoaded", () => {
    const toggles = document.querySelectorAll(".Account_Toggle_Pass");

    toggles.forEach((toggle) => {
        toggle.addEventListener("click", (e) => {
            e.preventDefault();

            const wrapper = toggle.closest(".Account_Input_Wrap");
            if (!wrapper) return;

            const input = wrapper.querySelector("input");
            if (!input) return;

            const isPassword = input.getAttribute("type") === "password";
            input.setAttribute("type", isPassword ? "text" : "password");
            toggle.classList.toggle("Account_Eye_Off", isPassword);
        });
    });
});
