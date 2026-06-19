document.addEventListener("DOMContentLoaded", () => {
    setupPopup("searchBtn", "searchPopup", "closeSearch");
    setupPopup("heartBtn", "heartPopup", "closeHeart");
    setupPopup("bagBtn", "bagPopup", "closeBag");
    setupPopup("userBtn", "userPopup", "closeUser");

    const userPopup = document.getElementById("userPopup");
    const logoutConfirmModal = document.getElementById("logoutConfirmModal");
    const profileDetailsModal = document.getElementById("profileDetailsModal");

    const btnOpenLogout = document.getElementById("btnOpenLogout");
    if (btnOpenLogout && logoutConfirmModal) {
        btnOpenLogout.addEventListener("click", () => {
            if (userPopup) userPopup.style.display = "none";
            logoutConfirmModal.style.display = "flex";
        });
    }

    const btnOpenProfile = document.getElementById("btnOpenProfile");
    const btnEditProfile = document.getElementById("btnEditProfile");
    const btnCancelEdit = document.getElementById("btnCancelEdit");
    const profileViewSection = document.getElementById("profileViewSection");
    const profileEditSection = document.getElementById("profileEditSection");
    const profileModalTitle = document.getElementById("profileModalTitle");

    const resetProfileToView = () => {
        if (profileViewSection) profileViewSection.style.display = "block";
        if (profileEditSection) profileEditSection.style.display = "none";
        if (profileModalTitle) profileModalTitle.innerText = "My Profile";
    };

    if (btnOpenProfile && profileDetailsModal) {
        btnOpenProfile.addEventListener("click", () => {
            if (userPopup) userPopup.style.display = "none";
            profileDetailsModal.style.display = "flex";
            
            fetch('/Account/GetUserProfile')
                .then(response => {
                    if (!response.ok) throw new Error("Server error: " + response.status);
                    return response.json();
                })
                .then(data => {
                    document.getElementById("lblFullName").innerText = data.fullName;
                    document.getElementById("lblAddress").innerText = data.address;
                    document.getElementById("lblContact").innerText = data.contact;
                    
                    document.getElementById("editFullName").value = data.fullName;
                    document.getElementById("editAddress").value = data.address;
                    document.getElementById("editContact").value = data.contact;
                })
                .catch(err => console.error("Fetch error:", err));
        });
    }

    if (btnEditProfile) {
        btnEditProfile.addEventListener("click", () => {
            if (profileViewSection) profileViewSection.style.display = "none";
            if (profileEditSection) profileEditSection.style.display = "block";
            if (profileModalTitle) profileModalTitle.innerText = "Edit Details";
        });
    }

    if (btnCancelEdit) {
        btnCancelEdit.addEventListener("click", () => {
            resetProfileToView();
        });
    }

    const closeButtons = ["btnXLogout", "btnCancelLogout", "btnXProfile", "btnCloseProfile"];
    closeButtons.forEach(id => {
        const btn = document.getElementById(id);
        if (btn) {
            btn.addEventListener("click", () => {
                if (logoutConfirmModal) logoutConfirmModal.style.display = "none";
                if (profileDetailsModal) {
                    profileDetailsModal.style.display = "none";
                    resetProfileToView();
                }
            });
        }
    });

    const editProfileForm = document.getElementById("editProfileForm");
    if (editProfileForm) {
        editProfileForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const formData = new FormData(editProfileForm);
            
            fetch('/Account/UpdateProfile', {
                method: 'POST',
                headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value },
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showSuccessModal("Profile updated successfully!");
                    
                    document.getElementById("btnCancelEdit").click();
                    document.getElementById("lblFullName").innerText = document.getElementById("editFullName").value;
                    document.getElementById("lblAddress").innerText = document.getElementById("editAddress").value;
                    document.getElementById("lblContact").innerText = document.getElementById("editContact").value;
                }
            })
            .catch(err => console.error("Save failed:", err));
        });
    }
});

function setupPopup(buttonId, popupId, closeId) {
    const button = document.getElementById(buttonId);
    const popup = document.getElementById(popupId);
    const close = document.getElementById(closeId);
    if (!button || !popup || !close) return;
    button.addEventListener("click", (e) => { e.preventDefault(); popup.style.display = "flex"; });
    close.addEventListener("click", () => { popup.style.display = "none"; });
}

function showSuccessModal(message) {
    const modal = document.getElementById("successModal");
    const msgElement = document.getElementById("successMessage");
    if (modal && msgElement) {
        msgElement.innerText = message;
        modal.style.display = "flex";
    }
}

function closeSuccessModal() {
    document.getElementById("successModal").style.display = "none";
}

function trackView(id, type) {
    fetch('/Lookbook/IncrementViewCount', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'id': id, 'type': type })
    });
}