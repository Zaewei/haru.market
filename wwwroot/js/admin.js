document.addEventListener('DOMContentLoaded', function () {

    // ── Hamburger sidebar toggle ──────────────────────────────
    const hamburgerBtn   = document.getElementById('hamburgerBtn');
    const adminSidebar   = document.getElementById('adminSidebar');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    if (hamburgerBtn && adminSidebar && sidebarOverlay) {

        function openSidebar() {
            adminSidebar.classList.add('open');
            sidebarOverlay.classList.add('visible');
            hamburgerBtn.classList.add('open');
            hamburgerBtn.setAttribute('aria-expanded', 'true');
        }

        function closeSidebar() {
            adminSidebar.classList.remove('open');
            sidebarOverlay.classList.remove('visible');
            hamburgerBtn.classList.remove('open');
            hamburgerBtn.setAttribute('aria-expanded', 'false');
        }

        hamburgerBtn.addEventListener('click', function () {
            adminSidebar.classList.contains('open') ? closeSidebar() : openSidebar();
        });

        sidebarOverlay.addEventListener('click', closeSidebar);

        adminSidebar.querySelectorAll('.sidebar-link').forEach(function (link) {
            link.addEventListener('click', closeSidebar);
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeSidebar();
        });
    }

    // ── Shared helpers ────────────────────────────────────────
    function ordBadgeFor(status) {
        var map = {
            pending:   '<span class="ord-badge ord-badge--pending">Pending</span>',
            paid:      '<span class="ord-badge ord-badge--paid">Paid</span>',
            shipped:   '<span class="ord-badge ord-badge--shipped">Shipped</span>',
            delivered: '<span class="ord-badge ord-badge--delivered">Completed</span>',
            cancelled: '<span class="ord-badge ord-badge--cancelled">Cancelled</span>'
        };
        return map[status] || '<span class="ord-badge">' + status + '</span>';
    }

   // ── Charts ────────────────────────────────────────────────
    const viewsCanvas = document.getElementById('viewsChart');
    const usersCanvas = document.getElementById('usersChart');

    if (viewsCanvas || usersCanvas) {
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js';
        script.onload = function () {

            // ── Lookbook Views —
            if (viewsCanvas) {
                const rawLabels = JSON.parse(viewsCanvas.getAttribute('data-labels') || '[]');
                const rawValues = JSON.parse(viewsCanvas.getAttribute('data-values') || '[]');

                const finalLabels = rawLabels.length > 0 ? rawLabels : ['No Data'];
                const finalValues = rawValues.length > 0 ? rawValues : [0];

                const maxVal = Math.max(...finalValues, 100);
                const suggestedMax = Math.ceil(maxVal / 1000) * 1000;

                new Chart(viewsCanvas, {
                    type: 'line',
                    data: {
                        labels: finalLabels,
                        datasets: [{
                            data: finalValues,
                            borderColor: '#c96a7f',
                            backgroundColor: 'rgba(201,106,127,0.08)',
                            borderWidth: 2,
                            pointRadius: 4,
                            pointBackgroundColor: '#c96a7f',
                            tension: 0.4,
                            fill: true
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { display: false } },
                        scales: {
                            y: {
                                beginAtZero: true,
                                min: 0,
                                suggestedMax: suggestedMax,
                                ticks: {
                                    color: '#b8929f',
                                    font: { family: 'Poppins, sans-serif' },
                                    callback: function(value) {
                                        if (value >= 1000) return (value / 1000) + 'K';
                                        return value;
                                    }
                                },
                                grid: { color: '#f7e4e8' }
                            },
                            x: {
                                grid: { display: false },
                                ticks: {
                                    color: '#b8929f',
                                    font: { family: 'Poppins, sans-serif' }
                                }
                            }
                        }
                    }
                });
            }

            // ── Users Overview — donut chart ──
            if (usersCanvas) {
                const liveUsers = parseInt(usersCanvas.getAttribute('data-total-users') || '0', 10);

                new Chart(usersCanvas, {
                    type: 'doughnut',
                    data: {
                        labels: ['New Users', 'Returning Users', 'Inactive Users'],
                        datasets: [{
                            data: [liveUsers, 0, 0],
                            backgroundColor: ['#c96a7f', '#e8b4be', '#f5dde2'],
                            borderWidth: 0
                        }]
                    },
                    options: {
                        cutout: '70%',
                        plugins: {
                            legend: { display: false },
                            tooltip: { enabled: true }
                        }
                    }
                });
            }
        };
        document.head.appendChild(script);
    }
    // ── Lookbook Page ─────────────────────────────────────────

    // ── Modal helpers ───────────────────────────────────────
    function lbOpenModal(id) {
        var el = document.getElementById(id);
        if (el) { el.style.display = 'flex'; }
    }
    function lbCloseModal(id) {
        var el = document.getElementById(id);
        if (el) { el.style.display = 'none'; }
    }

    // Close any modal when clicking the dark overlay
    document.querySelectorAll('.lb-modal-overlay').forEach(function(overlay) {
        overlay.addEventListener('click', function(e) {
            if (e.target === overlay) overlay.style.display = 'none';
        });
    });

    // ── Row 1: Cover image ──────────────────────────────────
    var changeCoverBtn = document.getElementById('changeCoverBtn');
    var coverFileInput = document.getElementById('coverFileInput');
    var coverArea      = document.getElementById('coverArea');

    if (changeCoverBtn && coverFileInput && coverArea) {
        changeCoverBtn.addEventListener('click', function() { coverFileInput.click(); });

        coverFileInput.addEventListener('change', function() {
            var file = coverFileInput.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function(ev) {
                var existing = document.getElementById('coverImg');
                if (existing) {
                    existing.src = ev.target.result;
                } else {
                    var ph = document.getElementById('coverPlaceholder');
                    if (ph) ph.style.display = 'none';
                    var img = document.createElement('img');
                    img.src = ev.target.result;
                    img.className = 'lb-cover-img';
                    img.id = 'coverImg';
                    coverArea.appendChild(img);
                }
            };
            reader.readAsDataURL(file);
        });
    }

    var removeCoverBtn = document.getElementById('removeCoverBtn');
    if (removeCoverBtn && coverArea) {
        removeCoverBtn.addEventListener('click', function() {
            var img = document.getElementById('coverImg');
            if (img) img.remove();
            var ph = document.getElementById('coverPlaceholder');
            if (ph) ph.style.display = 'flex';
            if (coverFileInput) coverFileInput.value = '';
        });
    }

    // ── Row 1: Preview + Update buttons ────────────────────
    var previewBtn = document.getElementById('previewBtn');
    if (previewBtn) {
        previewBtn.addEventListener('click', function(e) {
            e.preventDefault();
            window.open('/Home/Lookbook', '_blank');
        });
    }

    var updateLookbookBtn = document.getElementById('updateLookbookBtn');
    if (updateLookbookBtn) {
        updateLookbookBtn.addEventListener('click', function() {
            var form = document.getElementById('lookbookForm');
            if (form) form.reset();
            var titleEl = document.getElementById('featuredTitle');
            var descEl  = document.getElementById('featuredDesc');
            var titleInput = document.getElementById('lbTitle');
            var descInput  = document.getElementById('lbDesc');
            if (titleEl && titleInput) titleInput.value = titleEl.textContent.trim();
            if (descEl  && descInput)  descInput.value  = descEl.textContent.trim();
            var modalTitle = document.getElementById('modalTitle');
            if (modalTitle) modalTitle.textContent = 'Update Lookbook';
            lbOpenModal('lbModal');
        });
    }

    // ── Row 2: Thumbnail strip — switch main viewer ─────────
    var thumbStrip = document.getElementById('thumbStrip');
    if (thumbStrip) {
        thumbStrip.addEventListener('click', function(e) {
            var thumb = e.target.closest('.lb-thumb');
            if (!thumb) return;
            thumbStrip.querySelectorAll('.lb-thumb').forEach(function(t) { t.classList.remove('active'); });
            thumb.classList.add('active');
            var thumbImg  = thumb.querySelector('img');
            var mainImg   = document.getElementById('mainViewerImg');
            if (mainImg && thumbImg) mainImg.src = thumbImg.src;
        });
    }

    // ── Row 2: Edit Text modal ──────────────────────────────
    var editTextModal      = document.getElementById('editTextModal');
    var closeEditTextModal = document.getElementById('closeEditTextModal');
    var cancelEditText     = document.getElementById('cancelEditText');
    var saveEditText       = document.getElementById('saveEditText');

    if (closeEditTextModal) closeEditTextModal.addEventListener('click', function() { lbCloseModal('editTextModal'); });
    if (cancelEditText)     cancelEditText.addEventListener('click',     function() { lbCloseModal('editTextModal'); });

    if (saveEditText) {
        saveEditText.addEventListener('click', function() {
            var newTitle = document.getElementById('editTextTitle').value.trim();
            var newDesc  = document.getElementById('editTextDesc').value.trim();
            var featTitle = document.getElementById('featuredTitle');
            var featDesc  = document.getElementById('featuredDesc');
            if (featTitle && newTitle) featTitle.textContent = newTitle;
            if (featDesc  && newDesc)  featDesc.textContent  = newDesc;
            lbCloseModal('editTextModal');
        });
    }

    // ── Row 2 & 3: Manage Media modal ──────────────────────
    var manageMediaModal   = document.getElementById('manageMediaModal');
    var closeManageMedia   = document.getElementById('closeManageMedia');
    var cancelManageMedia  = document.getElementById('cancelManageMedia');
    var saveManageMedia    = document.getElementById('saveManageMedia');
    var mediaFileInput     = document.getElementById('mediaFileInput');
    var mediaGrid          = document.getElementById('mediaGrid');
    var lbActiveMediaId    = null;
    var lbMediaSlots       = [null, null, null, null]; // up to 4 image data URLs
    var lbMediaEditingSlot = -1;

    function lbRenderMediaGrid() {
        if (!mediaGrid) return;
        mediaGrid.innerHTML = '';
        for (var i = 0; i < 4; i++) {
            (function(idx) {
                if (lbMediaSlots[idx]) {
                    var slot = document.createElement('div');
                    slot.className = 'lb-media-slot';
                    var img = document.createElement('img');
                    img.src = lbMediaSlots[idx];
                    slot.appendChild(img);
                    var rmBtn = document.createElement('button');
                    rmBtn.className = 'lb-media-remove';
                    rmBtn.innerHTML = '&times;';
                    rmBtn.addEventListener('click', function(e) {
                        e.stopPropagation();
                        lbMediaSlots[idx] = null;
                        lbRenderMediaGrid();
                    });
                    slot.appendChild(rmBtn);
                    slot.addEventListener('click', function() {
                        lbMediaEditingSlot = idx;
                        if (mediaFileInput) mediaFileInput.click();
                    });
                    mediaGrid.appendChild(slot);
                } else {
                    var empty = document.createElement('div');
                    empty.className = 'lb-media-slot-empty';
                    empty.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke-width="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg><span>Add photo</span>';
                    empty.addEventListener('click', function() {
                        lbMediaEditingSlot = idx;
                        if (mediaFileInput) mediaFileInput.click();
                    });
                    mediaGrid.appendChild(empty);
                }
            })(i);
        }
    }

    if (mediaFileInput) {
        mediaFileInput.addEventListener('change', function() {
            var file = mediaFileInput.files[0];
            if (!file || lbMediaEditingSlot < 0) return;
            var reader = new FileReader();
            reader.onload = function(ev) {
                lbMediaSlots[lbMediaEditingSlot] = ev.target.result;
                lbMediaEditingSlot = -1;
                lbRenderMediaGrid();
                mediaFileInput.value = '';
            };
            reader.readAsDataURL(file);
        });
    }

    if (closeManageMedia)  closeManageMedia.addEventListener('click',  function() { lbCloseModal('manageMediaModal'); });
    if (cancelManageMedia) cancelManageMedia.addEventListener('click', function() { lbCloseModal('manageMediaModal'); });

    if (saveManageMedia) {
        saveManageMedia.addEventListener('click', function() {
            // Apply first non-null slot image to the featured main viewer + thumbs
            var firstImg = lbMediaSlots.find(function(s) { return s !== null; });
            if (firstImg) {
                var mainViewerImg = document.getElementById('mainViewerImg');
                if (mainViewerImg) mainViewerImg.src = firstImg;
                var thumbs = document.querySelectorAll('#thumbStrip .lb-thumb');
                thumbs.forEach(function(t, ti) {
                    var src = lbMediaSlots[ti] || firstImg;
                    var img = t.querySelector('img');
                    if (img) { img.src = src; } else {
                        var ni = document.createElement('img');
                        ni.src = src; t.appendChild(ni);
                    }
                });
            }
            lbCloseModal('manageMediaModal');
        });
    }

    // ── Row 3: Delegated actions (edit, delete, manage-media, publish) ──
    document.addEventListener('click', function(e) {
        var btn = e.target.closest('[data-action]');
        if (!btn) return;
        var action = btn.getAttribute('data-action');
        var id     = btn.getAttribute('data-id');

        if (action === 'edit-text') {
            var featTitle = document.getElementById('featuredTitle');
            var featDesc  = document.getElementById('featuredDesc');
            var etTitle   = document.getElementById('editTextTitle');
            var etDesc    = document.getElementById('editTextDesc');
            if (etTitle && featTitle) etTitle.value = featTitle.textContent.trim();
            if (etDesc  && featDesc)  etDesc.value  = featDesc.textContent.trim();
            lbOpenModal('editTextModal');
        }

        if (action === 'edit') {
            var form = document.getElementById('lookbookForm');
            if (form) form.reset();
            var lbIdEl = document.getElementById('lbId');
            if (lbIdEl) lbIdEl.value = id || '';
            var modalTitle = document.getElementById('modalTitle');
            if (modalTitle) modalTitle.textContent = 'Edit Lookbook';
            var item = document.getElementById('lb-item-' + id);
            if (item) {
                var nameEl     = item.querySelector('.lb-list-name');
                var titleInput = document.getElementById('lbTitle');
                if (nameEl && titleInput) titleInput.value = nameEl.textContent.trim();
            }
            lbOpenModal('lbModal');
        }

        if (action === 'delete') {
            var deleteIdEl = document.getElementById('deleteId');
            if (deleteIdEl) deleteIdEl.value = id || '';
            lbOpenModal('deleteModal');
        }

        if (action === 'manage-media') {
            lbActiveMediaId = id || null;
            lbMediaSlots    = [null, null, null, null];
            lbMediaEditingSlot = -1;
            // Pre-populate from existing card image if available
            if (id) {
                var cardEl = document.getElementById('lb-item-' + id);
                if (cardEl) {
                    var existingImg = cardEl.querySelector('.lb-draft-main-img img');
                    if (existingImg && existingImg.src) lbMediaSlots[0] = existingImg.src;
                }
            }
            lbRenderMediaGrid();
            lbOpenModal('manageMediaModal');
        }

        if (action === 'publish') {
            if (!id) return;
            btn.disabled    = true;
            btn.textContent = 'Publishing…';
            var token = ((document.querySelector('input[name="__RequestVerificationToken"]') || {}).value) || '';
            fetch('/Lookbook/PublishLookbook', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: 'id=' + encodeURIComponent(id) + '&__RequestVerificationToken=' + encodeURIComponent(token)
            }).then(function(res) {
                if (res.ok || res.redirected) {
                    var item = document.getElementById('lb-item-' + id);
                    if (item) {
                        var badge = item.querySelector('.lb-badge-draft');
                        if (badge) {
                            badge.textContent       = 'Published';
                            badge.style.background  = '#e6f4ea';
                            badge.style.color       = '#2e7d32';
                            badge.style.borderColor = '#b7dfbc';
                        }
                    }
                    btn.textContent = '✓ Published';
                } else {
                    btn.disabled = false; btn.textContent = 'Publish';
                }
            }).catch(function() { btn.disabled = false; btn.textContent = 'Publish'; });
        }
    });

    // ── Close modals ────────────────────────────────────────
    var closeDeleteModalBtn = document.getElementById('closeDeleteModal');
    if (closeDeleteModalBtn) closeDeleteModalBtn.addEventListener('click', function() { lbCloseModal('deleteModal'); });
    var cancelDeleteBtn = document.getElementById('cancelDelete');
    if (cancelDeleteBtn) cancelDeleteBtn.addEventListener('click', function() { lbCloseModal('deleteModal'); });

    var closeModalBtn  = document.getElementById('closeModal');
    if (closeModalBtn)  closeModalBtn.addEventListener('click',  function() { lbCloseModal('lbModal'); });
    var cancelModalBtn = document.getElementById('cancelModal');
    if (cancelModalBtn) cancelModalBtn.addEventListener('click', function() { lbCloseModal('lbModal'); });

    // ── Row 3: Creation card — image slot uploads ───────────
    var lbCreateCard    = document.getElementById('lbCreateCard');
    var createRowTrigger = document.getElementById('createRowTrigger');
    var createLookbookBtn = document.getElementById('createLookbookBtn');
    var cancelCreateBtn = document.getElementById('cancelCreateBtn');
    var saveCreateBtn   = document.getElementById('saveCreateBtn');
    var newSlotImages   = [null, null, null, null];

    // Show create card, hide trigger button
    if (createLookbookBtn && lbCreateCard && createRowTrigger) {
        // Initially hide the card, show button
        lbCreateCard.style.display = 'none';

        createLookbookBtn.addEventListener('click', function() {
            lbCreateCard.style.display = 'grid';
            createRowTrigger.style.display = 'none';
            // Reset fields
            newSlotImages = [null, null, null, null];
            var titleInput = document.getElementById('newLbTitle');
            var descInput  = document.getElementById('newLbDesc');
            if (titleInput) titleInput.value = '';
            if (descInput)  descInput.value  = '';
            // Reset thumb slots
            lbCreateCard.querySelectorAll('.lb-thumb-upload').forEach(function(t) {
                t.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#c9a0ac" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>';
                t.style.background = '';
            });
            // Reset main slot
            var mainSlot = document.getElementById('createMainSlot');
            if (mainSlot) {
                mainSlot.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="#c9a0ac" stroke-width="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg><span>Click to add photo</span>';
            }
        });
    }

    if (cancelCreateBtn) {
        cancelCreateBtn.addEventListener('click', function() {
            if (lbCreateCard) lbCreateCard.style.display = 'none';
            if (createRowTrigger) createRowTrigger.style.display = '';
        });
    }

    // Wire slot file inputs for the creation card
    function lbWireCreateSlot(slotEl, fileInputId, slotIndex) {
        var fileInput = document.getElementById(fileInputId);
        if (!slotEl || !fileInput) return;
        slotEl.addEventListener('click', function() { fileInput.click(); });
        fileInput.addEventListener('change', function() {
            var file = fileInput.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function(ev) {
                newSlotImages[slotIndex] = ev.target.result;
                slotEl.innerHTML = '';
                var img = document.createElement('img');
                img.src = ev.target.result;
                img.style.cssText = 'width:100%;height:100%;object-fit:cover;display:block;';
                slotEl.appendChild(img);
                fileInput.value = '';
            };
            reader.readAsDataURL(file);
        });
    }

    if (lbCreateCard) {
        var thumbUploads = lbCreateCard.querySelectorAll('.lb-thumb-upload');
        thumbUploads.forEach(function(slot, i) {
            lbWireCreateSlot(slot, 'slotFile' + i, i);
        });
        var mainSlotEl = document.getElementById('createMainSlot');
        lbWireCreateSlot(mainSlotEl, 'slotFile3', 3);
    }

    // Save / Publish from creation card — opens the backend form modal
    if (saveCreateBtn) {
        saveCreateBtn.addEventListener('click', function() {
            var titleVal = (document.getElementById('newLbTitle') || {}).value || '';
            var descVal  = (document.getElementById('newLbDesc')  || {}).value || '';
            var mediaVal = newSlotImages[0] || newSlotImages[1] || newSlotImages[2] || newSlotImages[3] || '';

            // Populate the hidden form and submit
            var form = document.getElementById('lookbookForm');
            var lbIdEl   = document.getElementById('lbId');
            var titleInp = document.getElementById('lbTitle');
            var descInp  = document.getElementById('lbDesc');
            var mediaInp = document.getElementById('lbMedia');
            var modalTitleEl = document.getElementById('modalTitle');

            if (form) form.reset();
            if (lbIdEl)   lbIdEl.value   = '';
            if (titleInp) titleInp.value = titleVal;
            if (descInp)  descInp.value  = descVal;
            if (mediaInp) mediaInp.value = mediaVal;
            if (modalTitleEl) modalTitleEl.textContent = 'Save New Lookbook';
            lbOpenModal('lbModal');
        });
    }


    // ── Product Management Page ───────────────────────────────────────────
    if (document.getElementById('productTableBody')) {

        var pmNextId     = 16;
        var pmEditingId  = null;
        var pmDeletingId = null;

        function pmOpenModal(id)  { var el = document.getElementById(id); if (el) el.style.display = 'flex'; }
        function pmCloseModal(id) { var el = document.getElementById(id); if (el) el.style.display = 'none'; }

        // ── Add Product button ──
        var addProductBtn = document.getElementById('addProductBtn');
        if (addProductBtn) {
            addProductBtn.addEventListener('click', function () {
                var m = document.getElementById('addProductModal');
                if (m) m.style.display = 'flex';
            });
        }

        // ── Close modals on overlay click or Escape ──
        document.querySelectorAll('.pm-modal-overlay').forEach(function (overlay) {
            overlay.addEventListener('click', function (e) {
                if (e.target === overlay) overlay.style.display = 'none';
            });
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                document.querySelectorAll('.pm-modal-overlay').forEach(function (o) { o.style.display = 'none'; });
            }
        });

        // ── Add Product modal – image upload ──
        var addUploadArea  = document.getElementById('addImageUploadArea');
        var addFileInput   = document.getElementById('addProductImageFile');
        var addPreviewImg  = document.getElementById('addPreviewImage');
        var addPlaceholder = document.getElementById('addImagePlaceholder');
        var addImageHidden = document.getElementById('addImageUrlHidden');

        function addShowPreview(src) {
            if (!addPreviewImg) return;
            addPreviewImg.src = src; addPreviewImg.style.display = 'block';
            if (addPlaceholder) addPlaceholder.style.display = 'none';
            if (addImageHidden) addImageHidden.value = src;
        }
        function addClearPreview() {
            if (!addPreviewImg) return;
            addPreviewImg.src = ''; addPreviewImg.style.display = 'none';
            if (addPlaceholder) addPlaceholder.style.display = 'flex';
            if (addFileInput)   addFileInput.value = '';
            if (addImageHidden) addImageHidden.value = '';
        }
        if (addUploadArea) {
            addUploadArea.addEventListener('click', function () { if (addFileInput) addFileInput.click(); });
            addUploadArea.addEventListener('dragover', function (e) { e.preventDefault(); addUploadArea.style.borderColor = '#c96a7f'; addUploadArea.style.background = '#fde8ed'; });
            addUploadArea.addEventListener('dragleave', function () { addUploadArea.style.borderColor = ''; addUploadArea.style.background = ''; });
            addUploadArea.addEventListener('drop', function (e) {
                e.preventDefault(); addUploadArea.style.borderColor = ''; addUploadArea.style.background = '';
                var file = e.dataTransfer.files[0]; if (!file) return;
                var reader = new FileReader(); reader.onload = function (ev) { addShowPreview(ev.target.result); }; reader.readAsDataURL(file);
            });
        }
        if (addFileInput) {
            addFileInput.addEventListener('change', function () {
                var file = addFileInput.files[0]; if (!file) return;
                var reader = new FileReader(); reader.onload = function (e) { addShowPreview(e.target.result); }; reader.readAsDataURL(file);
            });
        }

        // ── Delete modal buttons ──
        var _closeDel  = document.getElementById('closeDeleteProductModal');
        var _cancelDel = document.getElementById('cancelDeleteProduct');
        var _confirmDel = document.getElementById('confirmDeleteProduct');
        
        if (_closeDel)   _closeDel.addEventListener('click',   function () { pmCloseModal('deleteProductModal'); });
        if (_cancelDel)  _cancelDel.addEventListener('click',  function () { pmCloseModal('deleteProductModal'); });
        
        // The updated confirmation button that talks to the database
        if (_confirmDel) _confirmDel.addEventListener('click', function () {
            if (!pmDeletingId) return;

            var btn = this;
            btn.textContent = "Deleting...";
            btn.disabled = true;

            fetch('/Admin/DeleteProduct?id=' + encodeURIComponent(pmDeletingId), {
                method: 'POST'
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    var row = document.querySelector('.pm-row[data-id="' + pmDeletingId + '"]');
                    if (row) row.remove();
                    pmRender(); 
                    pmCloseModal('deleteProductModal'); 
                    pmDeletingId = null;
                } else {
                    alert("Failed to delete product: " + data.message);
                }
            })
            .catch(err => console.error(err))
            .finally(() => {
                btn.textContent = "Delete";
                btn.disabled = false;
            });
        });

        // ── Edit / Delete delegated click ──
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]');
            if (!btn) return;
            var action = btn.getAttribute('data-action');
            var id     = btn.getAttribute('data-id');

            if (action === 'pm-edit') {
                var row = document.querySelector('.pm-row[data-id="' + id + '"]');
                if (!row) return;
                pmEditingId = id;
                var cells = row.querySelectorAll('td');

                var imgEl = cells[0].querySelector('img');
                pmCarouselImages = [];
                if (imgEl && imgEl.src && !imgEl.src.endsWith('/')) {
                    pmCarouselImages.push(imgEl.src);
                }
                pmCarouselIndex = 0;
                pmRenderCarousel();

                var stockData = JSON.parse(row.getAttribute('data-stock') || '{}');

                var nameInp  = document.getElementById('pmEditName');
                var priceInp = document.getElementById('pmEditPrice');
                if (nameInp)  nameInp.value  = cells[1].querySelector('.pm-name').textContent.trim();
                if (priceInp) priceInp.value = cells[4].querySelector('.pm-price').textContent.replace('\u20b1','').replace(/,/g,'').trim();

                var colorVal = cells[2].querySelector('.pm-meta').textContent.trim().toLowerCase();
                document.querySelectorAll('.pm-edit-color').forEach(function (c) {
                    c.classList.toggle('pm-edit-color--active', !!(c.dataset.color && c.dataset.color.toLowerCase() === colorVal));
                });

                ['S','M','L','XL','XXL'].forEach(function (s) { 
                    var el = document.getElementById('pmStock-' + s); 
                    if (el) el.textContent = stockData[s] || 0; 
                });

                var panel = document.getElementById('pmEditPanel');
                var table = document.getElementById('pmTableView');
                if (panel) panel.style.display = 'block';
                if (table) table.style.display = 'none';
            }

            if (action === 'pm-delete') {
                pmDeletingId = id;
                var row  = document.querySelector('.pm-row[data-id="' + id + '"]');
                var name = row ? row.querySelector('.pm-name').textContent.trim() : 'this product';
                var nameEl = document.getElementById('deleteProductName');
                if (nameEl) nameEl.textContent = name;
                pmOpenModal('deleteProductModal');
            }
        });

        // ── Inline edit: Back ──
        var pmBackBtn = document.getElementById('pmEditBackBtn');
        if (pmBackBtn) {
            pmBackBtn.addEventListener('click', function () {
                var panel = document.getElementById('pmEditPanel');
                var table = document.getElementById('pmTableView');
                if (panel) panel.style.display = 'none';
                if (table) table.style.display = '';
                pmEditingId = null;
            });
        }

        // ── Inline edit: Save Changes ──
        var pmEditSaveBtn = document.getElementById('pmEditSaveBtn');
        if (pmEditSaveBtn) {
            pmEditSaveBtn.addEventListener('click', function () {
                if (!pmEditingId) return;

                var stockDict = {
                    "S": parseInt(document.getElementById('pmStock-S').textContent) || 0,
                    "M": parseInt(document.getElementById('pmStock-M').textContent) || 0,
                    "L": parseInt(document.getElementById('pmStock-L').textContent) || 0,
                    "XL": parseInt(document.getElementById('pmStock-XL').textContent) || 0,
                    "XXL": parseInt(document.getElementById('pmStock-XXL').textContent) || 0
                };

                var newName = document.getElementById('pmEditName').value.trim();
                var newPrice = document.getElementById('pmEditPrice').value.trim();
                var activeColorEl = document.querySelector('.pm-edit-color--active');
                var newColor = activeColorEl ? activeColorEl.dataset.color : '';

                fetch('/Admin/UpdateProduct', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        id: pmEditingId,
                        name: newName,
                        price: parseFloat(newPrice) || 0,
                        color: newColor,
                        stockQuantity: stockDict
                    })
                })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        var total = Object.values(stockDict).reduce((a, b) => a + b, 0);
                        var row = document.querySelector('.pm-row[data-id="' + pmEditingId + '"]');
                        if (row) row.querySelectorAll('td')[5].querySelector('.pm-meta').textContent = total;
                        
                        document.getElementById('pmEditPanel').style.display = 'none';
                        document.getElementById('pmTableView').style.display = '';
                        pmEditingId = null;
                    } else {
                        alert("Save failed: " + data.message);
                    }
                });
            });
        }

        // ── Inline edit: stock +/- ──
       document.addEventListener('click', function (e) {
            var btn = e.target.closest('.pm-stock-btn');
            if (!btn) return;
            var container = btn.closest('.pm-edit-stock-ctrl');
            var valEl = container ? container.querySelector('.pm-stock-val') : null;
            
            if (!valEl) return;

            var currentVal = parseInt(valEl.textContent) || 0;
            var dir = parseInt(btn.dataset.dir);
            
            var newVal = Math.max(0, currentVal + dir);
            valEl.textContent = newVal;
        });

        // ── Inline edit: add a new color swatch ──
        var pmAddColorBtn = document.querySelector('.pm-edit-color-add');
        var pmColorPickerInput = document.getElementById('pmColorPickerInput');
        var pmColorConfirmPopover = document.getElementById('pmColorConfirmPopover');
        var pmColorPreviewDot = document.getElementById('pmColorPreviewDot');
        var pmColorPreviewHex = document.getElementById('pmColorPreviewHex');

        if (pmAddColorBtn && pmColorPickerInput && pmColorConfirmPopover) {
            pmAddColorBtn.addEventListener('click', function () {
                pmColorPickerInput.click();
            });

            pmColorPickerInput.addEventListener('input', function () {
                var hex = this.value;
                pmColorPreviewDot.style.background = hex;
                pmColorPreviewHex.textContent = hex;
                pmColorConfirmPopover.style.display = 'flex';
            });

            document.getElementById('pmColorConfirmBtn').addEventListener('click', function () {
                var hex = pmColorPickerInput.value;
                var swatch = document.createElement('div');
                swatch.className = 'pm-edit-color pm-edit-color--custom';
                swatch.style.background = hex;
                swatch.style.borderColor = hex;
                swatch.dataset.color = hex;
                swatch.title = hex;
                swatch.innerHTML = '<span class="pm-color-remove-x" data-action="pm-color-remove">&times;</span>';
                pmColorsContainer.insertBefore(swatch, pmAddColorBtn);
                pmColorsContainer.querySelectorAll('.pm-edit-color').forEach(function (x) {
                    x.classList.remove('pm-edit-color--active');
                });
                swatch.classList.add('pm-edit-color--active');
                pmColorConfirmPopover.style.display = 'none';
            });

            document.getElementById('pmColorCancelBtn').addEventListener('click', function () {
                pmColorConfirmPopover.style.display = 'none';
            });
        }

        // ── Inline edit: remove a color (with confirmation) ──
        var pmColorRemoveConfirmPopover = document.getElementById('pmColorRemoveConfirmPopover');
        var pmColorRemoveConfirmBtn     = document.getElementById('pmColorRemoveConfirmBtn');
        var pmColorRemoveCancelBtn      = document.getElementById('pmColorRemoveCancelBtn');
        var pmSwatchPendingRemoval      = null;

        if (pmColorsContainer && pmColorRemoveConfirmPopover) {
            pmColorsContainer.addEventListener('click', function (e) {
                var xBtn = e.target.closest('.pm-color-remove-x');
                if (!xBtn) return;
                e.stopPropagation();
                pmSwatchPendingRemoval = xBtn.closest('.pm-edit-color');
                pmColorRemoveConfirmPopover.style.display = 'flex';
            });

            pmColorRemoveConfirmBtn.addEventListener('click', function () {
                if (pmSwatchPendingRemoval) {
                    pmSwatchPendingRemoval.remove();
                    pmSwatchPendingRemoval = null;
                }
                pmColorRemoveConfirmPopover.style.display = 'none';
            });

            pmColorRemoveCancelBtn.addEventListener('click', function () {
                pmSwatchPendingRemoval = null;
                pmColorRemoveConfirmPopover.style.display = 'none';
            });
        }

        // ── Carousel state ──
        var pmCarouselImages = []; // array of image src strings
        var pmCarouselIndex  = 0;

        function pmRenderCarousel() {
            var mainImg  = document.getElementById('pmEditMainImg');
            var mainPH   = document.getElementById('pmEditMainPlaceholder');
            var prevBtn  = document.getElementById('pmCarouselPrev');
            var nextBtn  = document.getElementById('pmCarouselNext');
            var dotsEl   = document.getElementById('pmCarouselDots');
            var thumbsEl = document.getElementById('pmEditThumbs');
            var addBtn   = document.getElementById('pmThumbAddBtn');

            var imgs = pmCarouselImages;
            var idx  = pmCarouselIndex;

            // Main viewer
            if (imgs.length > 0) {
                if (mainImg) { mainImg.src = imgs[idx]; mainImg.style.display = 'block'; }
                if (mainPH)  mainPH.style.display = 'none';
            } else {
                if (mainImg) { mainImg.src = ''; mainImg.style.display = 'none'; }
                if (mainPH)  mainPH.style.display = 'flex';
            }

            // Arrows
            if (prevBtn) prevBtn.disabled = (imgs.length <= 1);
            if (nextBtn) nextBtn.disabled = (imgs.length <= 1);

            // Dots
            if (dotsEl) {
                dotsEl.innerHTML = '';
                imgs.forEach(function(_, i) {
                    var d = document.createElement('span');
                    d.className = 'pm-carousel-dot' + (i === idx ? ' pm-carousel-dot--active' : '');
                    (function(dotIdx) {
                        d.addEventListener('click', function() { pmCarouselIndex = dotIdx; pmRenderCarousel(); });
                    })(i);
                    dotsEl.appendChild(d);
                });
            }

            // Remove Photo button visibility
            var removePhotoBtn = document.getElementById('pmRemovePhotoBtn');
            if (removePhotoBtn) {
                removePhotoBtn.style.display = 'inline-flex';
                removePhotoBtn.disabled = imgs.length === 0;
            }

            // Thumbnail strip
            if (thumbsEl) {
                Array.from(thumbsEl.querySelectorAll('.pm-edit-thumb:not(.pm-edit-thumb--add)')).forEach(function(el) { el.remove(); });
                imgs.forEach(function(src, i) {
                    var thumb = document.createElement('div');
                    thumb.className = 'pm-edit-thumb' + (i === idx ? ' pm-edit-thumb--active' : '');
                    thumb.draggable = true;
                    thumb.dataset.index = i;

                    var img = document.createElement('img');
                    img.src = src;
                    thumb.appendChild(img);

                    // Click to set active
                    (function(clickIdx) {
                        thumb.addEventListener('click', function() {
                            pmCarouselIndex = clickIdx;
                            pmRenderCarousel();
                        });
                    })(i);

                    // Drag-and-drop
                    thumb.addEventListener('dragstart', function(e) {
                        e.dataTransfer.setData('text/plain', this.dataset.index);
                        this.classList.add('pm-edit-thumb--dragging');
                    });
                    thumb.addEventListener('dragend', function() {
                        this.classList.remove('pm-edit-thumb--dragging');
                        if (thumbsEl) thumbsEl.querySelectorAll('.pm-edit-thumb').forEach(function(t) { t.classList.remove('pm-edit-thumb--dragover'); });
                    });
                    thumb.addEventListener('dragover', function(e) {
                        e.preventDefault();
                        if (thumbsEl) thumbsEl.querySelectorAll('.pm-edit-thumb').forEach(function(t) { t.classList.remove('pm-edit-thumb--dragover'); });
                        this.classList.add('pm-edit-thumb--dragover');
                    });
                    thumb.addEventListener('drop', function(e) {
                        e.preventDefault();
                        var fromIdx = parseInt(e.dataTransfer.getData('text/plain'));
                        var toIdx   = parseInt(this.dataset.index);
                        if (fromIdx === toIdx || isNaN(fromIdx) || isNaN(toIdx)) return;
                        var moved = pmCarouselImages.splice(fromIdx, 1)[0];
                        pmCarouselImages.splice(toIdx, 0, moved);
                        pmCarouselIndex = toIdx;
                        pmRenderCarousel();
                    });

                    if (addBtn) thumbsEl.insertBefore(thumb, addBtn);
                    else thumbsEl.appendChild(thumb);
                });
            }
        }

        // Arrow navigation
        var pmPrevBtn = document.getElementById('pmCarouselPrev');
        var pmNextBtn = document.getElementById('pmCarouselNext');
        if (pmPrevBtn) pmPrevBtn.addEventListener('click', function() {
            if (pmCarouselImages.length <= 1) return;
            pmCarouselIndex = (pmCarouselIndex - 1 + pmCarouselImages.length) % pmCarouselImages.length;
            pmRenderCarousel();
        });
        if (pmNextBtn) pmNextBtn.addEventListener('click', function() {
            if (pmCarouselImages.length <= 1) return;
            pmCarouselIndex = (pmCarouselIndex + 1) % pmCarouselImages.length;
            pmRenderCarousel();
        });

        // Remove current photo
        var pmRemovePhotoBtn = document.getElementById('pmRemovePhotoBtn');
        if (pmRemovePhotoBtn) {
            pmRemovePhotoBtn.addEventListener('click', function() {
                if (!pmCarouselImages.length) return;
                pmCarouselImages.splice(pmCarouselIndex, 1);
                if (pmCarouselIndex >= pmCarouselImages.length) pmCarouselIndex = Math.max(0, pmCarouselImages.length - 1);
                pmRenderCarousel();
            });
        }

        // ── Inline edit: image upload (multi-file) ──
        var pmEditImageFile = document.getElementById('pmEditImageFile');
        if (pmEditImageFile) {
            pmEditImageFile.addEventListener('change', function () {
                var files = Array.from(this.files);
                if (!files.length) return;
                var loaded = 0;
                files.forEach(function(file) {
                    var reader = new FileReader();
                    reader.onload = function(ev) {
                        pmCarouselImages.push(ev.target.result);
                        loaded++;
                        if (loaded === files.length) {
                            pmCarouselIndex = pmCarouselImages.length - files.length;
                            pmRenderCarousel();
                        }
                    };
                    reader.readAsDataURL(file);
                });
                pmEditImageFile.value = '';
            });
        }


                // ── Pagination ──
        var pmCurrentPage = 1;
        var pmPerPage = 5;

        function pmGetAllRows() { return Array.from(document.querySelectorAll('#productTableBody .pm-row')); }

        function pmRender() {
            var rows  = pmGetAllRows();
            var total = Math.max(1, Math.ceil(rows.length / pmPerPage));
            if (pmCurrentPage > total) pmCurrentPage = total;
            rows.forEach(function (r, i) { r.style.display = (Math.floor(i / pmPerPage) + 1 === pmCurrentPage) ? '' : 'none'; });

            var bar = document.getElementById('pmPaginationBar');
            if (!bar) return;
            bar.innerHTML = '';

            var prev = document.createElement('button');
            prev.className = 'pm-page-btn'; prev.textContent = 'Previous'; prev.disabled = (pmCurrentPage === 1);
            prev.addEventListener('click', function () { pmGoTo(pmCurrentPage - 1); });
            bar.appendChild(prev);

            for (var p = 1; p <= total; p++) {
                (function (page) {
                    var btn = document.createElement('button');
                    btn.className = 'pm-page-num' + (page === pmCurrentPage ? ' active' : '');
                    btn.textContent = page;
                    btn.addEventListener('click', function () { pmGoTo(page); });
                    bar.appendChild(btn);
                })(p);
            }

            var next = document.createElement('button');
            next.className = 'pm-page-btn'; next.textContent = 'Next'; next.disabled = (pmCurrentPage === total);
            next.addEventListener('click', function () { pmGoTo(pmCurrentPage + 1); });
            bar.appendChild(next);
        }

        function pmGoTo(page) {
            pmCurrentPage = Math.max(1, Math.min(page, Math.max(1, Math.ceil(pmGetAllRows().length / pmPerPage))));
            pmRender();
        }

        pmRender();

        function pmBuildRowHTML(id, imgSrc, name, color, size, price, stock) {
            var thumb = imgSrc ? '<img src="' + imgSrc + '" class="pm-thumb" alt="' + name + '" />' : '<div class="pm-thumb pm-thumb-placeholder">No Image</div>';
            return '<td><div class="pm-thumb-wrap">' + thumb + '</div></td>'
                + '<td><span class="pm-name">' + name + '</span></td>'
                + '<td><span class="pm-meta">' + color + '</span></td>'
                + '<td><span class="pm-meta">' + size + '</span></td>'
                + '<td><span class="pm-price">\u20b1' + price + '</span></td>'
                + '<td><span class="pm-meta">' + stock + '</span></td>'
                + '<td class="pm-actions pm-actions--stack">'
                + '<button class="btn-edit" data-action="pm-edit" data-id="' + id + '">Edit</button>'
                + '<button class="btn-delete" data-action="pm-delete" data-id="' + id + '">Delete</button>'
                + '</td>';
        }

        function pmUpdateThumbCell(cell, imgSrc) {
            var wrap = cell ? cell.querySelector('.pm-thumb-wrap') : null; if (!wrap) return;
            if (imgSrc) { var ex = wrap.querySelector('img'); if (ex) ex.src = imgSrc; else wrap.innerHTML = '<img src="' + imgSrc + '" class="pm-thumb" alt="" />'; }
            else wrap.innerHTML = '<div class="pm-thumb pm-thumb-placeholder">No Image</div>';
        }

    }
    // ── End Product Management ────────────────────────────────────

    // ── Shared order data (used by Orders page AND Users page) ──
    var allOrders = window.allOrders || [];

    // ── Orders Page ───────────────────────────────────────────
    
    if (document.getElementById('ordersBody')) {

        var ordPageSize        = 10;
        var ordCurrentPage     = 1;
        var ordActiveEditIdx   = -1;
        var ordActiveDeleteIdx = -1;

        function ordFiltered() {
            var q      = document.getElementById('ordSearch').value.toLowerCase();
            var status = document.getElementById('statusFilter').value;
            var dateEl = document.getElementById('dateFilter');
            var dateVal = dateEl ? dateEl.value : '';
            return allOrders.filter(function(o) {
                var matchQ = !q || o.id.toLowerCase().includes(q) || o.name.toLowerCase().includes(q) || o.email.toLowerCase().includes(q);
                var matchS = !status || o.status === status;
                var matchD = !dateVal || ordMonthKey(o.date) === dateVal;
                return matchQ && matchS && matchD;
            });
        }

        function ordMonthKey(dateStr) {
            if (!dateStr) return '';
            var parts = dateStr.split(' ');
            return parts[0] + ' ' + parts[2]; // e.g. "May 2026"
        }

        function ordPopulateDateFilter() {
            var el = document.getElementById('dateFilter');
            if (!el) return;
            var seen = {};
            var months = [];
            allOrders.forEach(function(o) {
                var key = ordMonthKey(o.date);
                if (key && !seen[key]) { seen[key] = true; months.push(key); }
            });
            // Sort descending
            months.sort(function(a, b) {
                var da = new Date(a), db = new Date(b);
                return db - da;
            });
            el.innerHTML = '<option value="">All Dates</option>';
            months.forEach(function(m) {
                var opt = document.createElement('option');
                opt.value = m;
                opt.textContent = m;
                el.appendChild(opt);
            });
        }

        function ordUpdateStats() {
            var total    = allOrders.length;
            var revenue  = allOrders.reduce(function(sum, o) {
                var n = parseFloat(String(o.total).replace(/[^0-9.]/g, '')) || 0;
                return sum + n;
            }, 0);
            var pending  = allOrders.filter(function(o) { return o.status === 'pending'; }).length;
            var shipped  = allOrders.filter(function(o) { return o.status === 'shipped'; }).length;

            var elTotal   = document.getElementById('statTotalOrders');
            var elRev     = document.getElementById('statTotalRevenue');
            var elPending = document.getElementById('statPendingOrders');
            var elShipped = document.getElementById('statShippedOrders');

            if (elTotal)   elTotal.textContent   = total;
            if (elRev)     elRev.textContent      = '\u20b1' + revenue.toLocaleString();
            if (elPending) elPending.textContent  = pending;
            if (elShipped) elShipped.textContent  = shipped;
        }

        function ordRender() {
        if (window.allOrders && window.allOrders.length > 0 && allOrders.length === 0) {
            allOrders = window.allOrders;
        }

        var orders = ordFiltered();
        var totalPages = Math.max(1, Math.ceil(orders.length / ordPageSize));
        if (ordCurrentPage > totalPages) ordCurrentPage = totalPages;
        var start = (ordCurrentPage - 1) * ordPageSize;
        var page  = orders.slice(start, start + ordPageSize);

        var tbody = document.getElementById('ordersBody');
        var empty = document.getElementById('ordEmpty');

        if (page.length === 0) {
            tbody.innerHTML = '';
            empty.style.display = 'block';
        } else {
            empty.style.display = 'none';
            tbody.innerHTML = page.map(function(o) {
                var idx = allOrders.indexOf(o);
                return '<tr>' +
                    '<td class="ord-id">' + o.id + '</td>' +
                    '<td><div class="ord-customer-name">' + o.name + '</div><div class="ord-customer-email">' + o.email + '</div></td>' +
                    '<td><div class="ord-date">' + o.date + '</div><div class="ord-time">' + o.time + '</div></td>' +
                    '<td>' + ordBadgeFor(o.status) + '</td>' +
                    '<td class="ord-payment">' + o.payment + '</td>' +
                    '<td class="ord-total">' + o.total + '</td>' +
                    '<td class="ord-actions-cell">' +
                        '<div class="ord-ellipsis-wrap">' +
                            '<button class="btn-ord-ellipsis" data-idx="' + idx + '" aria-label="More actions"><span></span><span></span><span></span></button>' +
                            '<div class="ord-dropdown" id="drop-' + idx + '">' +
                                '<button class="ord-drop-item" data-action="ord-view"   data-idx="' + idx + '">View</button>' +
                                '<button class="ord-drop-item" data-action="ord-edit"   data-idx="' + idx + '">Edit Status</button>' +
                                '<button class="ord-drop-item ord-drop-item--delete" data-action="ord-delete" data-idx="' + idx + '">Delete</button>' +
                            '</div>' +
                        '</div>' +
                    '</td>' +
                '</tr>';
            }).join('');
        }
        ordRenderPagination(totalPages);
        ordBindRows();
    }
        function ordRenderPagination(totalPages) {
            var el = document.getElementById('ordPagination');
            if (!el) return;
            if (totalPages <= 1) { el.innerHTML = ''; return; }
            el.innerHTML = '';

            var prev = document.createElement('button');
            prev.className = 'ord-page-btn';
            prev.textContent = 'Previous';
            prev.disabled = ordCurrentPage === 1;
            prev.addEventListener('click', function() { if (ordCurrentPage > 1) { ordCurrentPage--; ordRender(); } });
            el.appendChild(prev);

            for (var i = 1; i <= totalPages; i++) {
                (function(page) {
                    var btn = document.createElement('button');
                    btn.className = 'ord-page-btn ord-page-num' + (page === ordCurrentPage ? ' ord-page-num--active' : '');
                    btn.textContent = page;
                    btn.addEventListener('click', function() { ordCurrentPage = page; ordRender(); });
                    el.appendChild(btn);
                })(i);
            }

            var next = document.createElement('button');
            next.className = 'ord-page-btn';
            next.textContent = 'Next';
            next.disabled = ordCurrentPage === totalPages;
            next.addEventListener('click', function() { if (ordCurrentPage < totalPages) { ordCurrentPage++; ordRender(); } });
            el.appendChild(next);
        }

        function ordCloseAllDropdowns() {
            document.querySelectorAll('.ord-dropdown--open').forEach(function(d) { d.classList.remove('ord-dropdown--open'); });
        }

        function ordBindRows() {
            document.querySelectorAll('.btn-ord-view').forEach(function(btn) {
                btn.addEventListener('click', function() { ordOpenViewModal(parseInt(this.dataset.idx)); });
            });
            document.querySelectorAll('.btn-ord-ellipsis').forEach(function(btn) {
                btn.addEventListener('click', function(e) {
                    e.stopPropagation();
                    var drop = document.getElementById('drop-' + this.dataset.idx);
                    var open = drop.classList.contains('ord-dropdown--open');
                    ordCloseAllDropdowns();
                    if (!open) drop.classList.add('ord-dropdown--open');
                });
            });
            document.querySelectorAll('.ord-drop-item').forEach(function(btn) {
                btn.addEventListener('click', function(e) {
                    e.stopPropagation();
                    var idx    = parseInt(this.dataset.idx);
                    var action = this.dataset.action;
                    ordCloseAllDropdowns();
                    if (action === 'ord-view')   ordOpenViewModal(idx);
                    if (action === 'ord-edit')   ordOpenEditModal(idx);
                    if (action === 'ord-delete') ordOpenDeleteModal(idx);
                });
            });
        }

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-usr-view') || e.target.closest('.usr-detail-overlay')) return;
            ordCloseAllDropdowns();
        });

        function ordOpenModal(id) {
            var el = document.getElementById(id);
            if (el) el.classList.add('ord-modal-overlay--open');
            document.body.style.overflow = 'hidden';
        }
        function ordCloseModal(id) {
            var el = document.getElementById(id);
            if (el) el.classList.remove('ord-modal-overlay--open');
            document.body.style.overflow = '';
        }

        ['viewOrderModal', 'editOrderModal', 'deleteOrderModal'].forEach(function(id) {
            var el = document.getElementById(id);
            if (el) el.addEventListener('click', function(e) { if (e.target === this) ordCloseModal(id); });
        });

        document.getElementById('closeViewModal').addEventListener('click',       function() { ordCloseModal('viewOrderModal'); });
        document.getElementById('closeViewModalFooter').addEventListener('click', function() { ordCloseModal('viewOrderModal'); });
        document.getElementById('closeEditModal').addEventListener('click',       function() { ordCloseModal('editOrderModal'); });
        document.getElementById('cancelEditModal').addEventListener('click',      function() { ordCloseModal('editOrderModal'); });
        document.getElementById('closeDeleteModal').addEventListener('click',     function() { ordCloseModal('deleteOrderModal'); });
        document.getElementById('cancelDeleteModal').addEventListener('click',    function() { ordCloseModal('deleteOrderModal'); });

        function ordOpenViewModal(idx) {
            var o = allOrders[idx];
            document.getElementById('viewModalTitle').textContent = 'Order ' + o.id;
            var trackingRow = '';
            if (o.trackingNumber) {
                trackingRow =
                    '<div class="ord-view-row"><span class="ord-view-label">Courier</span><span class="ord-view-val">' + (o.courierCode || '—') + '</span></div>' +
                    '<div class="ord-view-row"><span class="ord-view-label">Tracking #</span><span class="ord-view-val">' + o.trackingNumber + '</span></div>' +
                    '<div class="ord-view-row"><span class="ord-view-label">Tracking Status</span><span class="ord-view-val" id="ordTrackingStatusVal">' + (o.trackingStatus || 'pending') + '</span></div>' +
                    '<div class="ord-view-row"><span class="ord-view-label"></span><span class="ord-view-val"><button type="button" class="pm-btn-cancel" id="btnRefreshTracking" data-idx="' + idx + '">Refresh Tracking</button></span></div>';
            }
            document.getElementById('viewModalBody').innerHTML =
                '<div class="ord-view-row"><span class="ord-view-label">Order ID</span><span class="ord-view-val ord-id">' + o.id + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Customer</span><span class="ord-view-val">' + o.name + '<br><small>' + o.email + '</small></span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Date</span><span class="ord-view-val">' + o.date + ' · ' + o.time + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Status</span><span class="ord-view-val">' + ordBadgeFor(o.status) + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Payment</span><span class="ord-view-val">' + o.payment + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Total</span><span class="ord-view-val ord-total">' + o.total + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Address</span><span class="ord-view-val">' + o.address + '</span></div>' +
                trackingRow;

            var refreshBtn = document.getElementById('btnRefreshTracking');
            if (refreshBtn) {
                refreshBtn.addEventListener('click', function() {
                    var orderId = this.dataset.idx !== undefined ? allOrders[parseInt(this.dataset.idx)].id : o.id;
                    refreshBtn.disabled = true;
                    refreshBtn.textContent = 'Refreshing…';
                    fetch('/Admin/RefreshOrderTracking', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: new URLSearchParams({ 'orderId': orderId })
                    })
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        if (data.success) {
                            o.trackingStatus = data.trackingStatus;
                            var statusEl = document.getElementById('ordTrackingStatusVal');
                            if (statusEl) statusEl.textContent = data.trackingStatus;
                            ordShowToast('Tracking refreshed: ' + data.trackingStatus);
                        } else {
                            ordShowToast(data.message || 'Could not refresh tracking.');
                        }
                    })
                    .catch(function() { ordShowToast('Could not reach the server to refresh tracking.'); })
                    .finally(function() {
                        refreshBtn.disabled = false;
                        refreshBtn.textContent = 'Refresh Tracking';
                    });
                });
            }

            ordOpenModal('viewOrderModal');
        }

        var ordCouriersCache = null;
        function ordPopulateCourierSelect(selectedCode) {
            var sel = document.getElementById('editCourierSelect');
            if (!sel) return;

            function fill(couriers) {
                sel.innerHTML = '<option value="">Select a courier…</option>' + couriers.map(function(c) {
                    return '<option value="' + c.code + '"' + (c.code === selectedCode ? ' selected' : '') + '>' + c.name + '</option>';
                }).join('');
            }

            if (ordCouriersCache) { fill(ordCouriersCache); return; }

            sel.innerHTML = '<option value="">Loading couriers…</option>';
            fetch('/Admin/GetCouriers')
                .then(function(r) { return r.json(); })
                .then(function(data) {
                    if (data.success) {
                        ordCouriersCache = data.couriers;
                        fill(ordCouriersCache);
                    } else {
                        sel.innerHTML = '<option value="">Could not load couriers</option>';
                    }
                })
                .catch(function() { sel.innerHTML = '<option value="">Could not load couriers</option>'; });
        }

        function ordToggleTrackingFields() {
            var group = document.getElementById('trackingFieldsGroup');
            var status = document.getElementById('editStatusSelect').value;
            if (group) group.style.display = (status === 'shipped') ? 'block' : 'none';
        }
        var editStatusSelectEl = document.getElementById('editStatusSelect');
        if (editStatusSelectEl) editStatusSelectEl.addEventListener('change', ordToggleTrackingFields);

        function ordOpenEditModal(idx) {
            ordActiveEditIdx = idx;
            var o = allOrders[idx];
            document.getElementById('editModalOrderId').textContent = o.id;
            document.getElementById('editModalCustomer').textContent = o.name;
            document.getElementById('editStatusSelect').value = o.status;
            document.getElementById('editTrackingNumber').value = o.trackingNumber || '';
            ordPopulateCourierSelect(o.courierCode || '');
            ordToggleTrackingFields();
            ordOpenModal('editOrderModal');
        }
        // NOTE: actual saving (POST to /Admin/UpdateOrderStatus, including courier/tracking
        // number so a TrackingMore shipment gets created) is wired up in Orders.cshtml.

        function ordOpenDeleteModal(idx) {
            ordActiveDeleteIdx = idx;
            document.getElementById('deleteModalOrderId').textContent = allOrders[idx].id;
            ordOpenModal('deleteOrderModal');
        }
        // NOTE: actual deletion (POST to /Admin/DeleteOrder) is wired up in Orders.cshtml.

        document.getElementById('ordSearch').addEventListener('input',    function() { ordCurrentPage = 1; ordRender(); });
        document.getElementById('statusFilter').addEventListener('change', function() { ordCurrentPage = 1; ordRender(); });
        var dateFilterEl = document.getElementById('dateFilter');
        if (dateFilterEl) dateFilterEl.addEventListener('change', function() { ordCurrentPage = 1; ordRender(); });

        function ordShowToast(msg) {
            var t = document.createElement('div');
            t.className = 'ord-toast';
            t.textContent = msg;
            document.body.appendChild(t);
            requestAnimationFrame(function() { t.classList.add('ord-toast--show'); });
            setTimeout(function() {
                t.classList.remove('ord-toast--show');
                setTimeout(function() { t.remove(); }, 350);
            }, 2600);
        }

        ordPopulateDateFilter();
        ordRender();
        ordUpdateStats();
    }
    // ── End Orders Page ───────────────────────────────────────

    // ── Users Page ────────────────────────────────────────────
    if (document.getElementById('usersBody')) {
        if (window.allUsers && window.allUsers.length > 0) {
        allUsers = window.allUsers; 
    }
        var usrPageSize    = 10;
        var usrCurrentPage = 1;

        // ── Update user stat cards from allUsers data ──
        function usrUpdateStats() {
            var total    = allUsers.length;
            var active   = allUsers.filter(function(u) { return u.status === 'active'; }).length;
            var inactive = allUsers.filter(function(u) { return u.status === 'inactive'; }).length;

            // "New this month" = joined in the same month/year as the most recent join date
            var months = allUsers.map(function(u) { return u.joined; }).sort().reverse();
            var latestMonth = months.length ? months[0].split(' ').slice(0,2).join(' ') : '';
            var newThisMonth = allUsers.filter(function(u) {
                return u.joined.indexOf(latestMonth) === 0;
            }).length;

            var elTotal    = document.getElementById('statTotalUsers');
            var elActive   = document.getElementById('statActiveUsers');
            var elNew      = document.getElementById('statNewUsers');
            var elInactive = document.getElementById('statInactiveUsers');

            if (elTotal)    elTotal.textContent    = total;
            if (elActive)   elActive.textContent   = active;
            if (elNew)      elNew.textContent       = newThisMonth;
            if (elInactive) elInactive.textContent  = inactive;
        }

        function usrFiltered() {
            var q      = document.getElementById('usrSearch').value.toLowerCase();
            var role   = document.getElementById('roleFilter').value;
            return allUsers.filter(function(u) {
                var matchQ = !q || u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q) || u.id.toLowerCase().includes(q);
                var matchR = !role || u.role === role;
                return matchQ && matchR;
            });
        }

        function usrInitials(name) {
            return name.split(' ').slice(0,2).map(function(w){ return w[0]; }).join('').toUpperCase();
        }

        function usrBadgeRole(role) {
            return role === 'admin'
                ? '<span class="usr-badge usr-badge--admin">Admin</span>'
                : '<span class="usr-badge usr-badge--customer">Customer</span>';
        }
        function usrBadgeStatus(status) {
            return status === 'active'
                ? '<span class="usr-badge usr-badge--active">Active</span>'
                : '<span class="usr-badge usr-badge--inactive">Inactive</span>';
        }

        function usrRender() {
            var users = usrFiltered();
            var totalPages = Math.max(1, Math.ceil(users.length / usrPageSize));
            if (usrCurrentPage > totalPages) usrCurrentPage = totalPages;
            var start = (usrCurrentPage - 1) * usrPageSize;
            var page  = users.slice(start, start + usrPageSize);

            var tbody = document.getElementById('usersBody');
            var empty = document.getElementById('usrEmpty');

            if (page.length === 0) {
                tbody.innerHTML = '';
                empty.style.display = 'block';
            } else {
                empty.style.display = 'none';
                tbody.innerHTML = page.map(function(u) {
                    var idx = allUsers.indexOf(u);
                    return '<tr>' +
                        '<td><div class="usr-cell"><div class="usr-avatar">' + usrInitials(u.name) + '</div><span class="usr-cell-name">' + u.name + '</span></div></td>' +
                        '<td>' + u.email + '</td>' +
                        '<td><div class="usr-joined">' + u.joined + '</div></td>' +
                        '<td>' + usrBadgeRole(u.role) + '</td>' +
                        '<td>' + usrBadgeStatus(u.status) + '</td>' +
                        '<td><div class="usr-last-active">' + u.lastActive + '</div></td>' +
                        '<td><button class="btn-usr-view" data-idx="' + idx + '">View</button></td>' +
                    '</tr>';
                }).join('');
            }
            usrRenderPagination(totalPages);
            usrBindRows();
        }

        function usrRenderPagination(totalPages) {
            var el = document.getElementById('usrPagination');
            if (totalPages <= 1) { el.innerHTML = ''; return; }
            var html = '<button class="usr-page-btn" id="usrPrevPage"' + (usrCurrentPage === 1 ? ' disabled' : '') + '>Previous</button>';
            for (var i = 1; i <= totalPages; i++) {
                html += '<button class="usr-page-btn usr-page-num' + (i === usrCurrentPage ? ' usr-page-num--active' : '') + '" data-page="' + i + '">' + i + '</button>';
            }
            html += '<button class="usr-page-btn" id="usrNextPage"' + (usrCurrentPage === totalPages ? ' disabled' : '') + '>Next</button>';
            el.innerHTML = html;
            el.querySelectorAll('.usr-page-num').forEach(function(btn) {
                btn.addEventListener('click', function() { usrCurrentPage = parseInt(this.dataset.page); usrRender(); });
            });
            var prev = el.querySelector('#usrPrevPage');
            var next = el.querySelector('#usrNextPage');
            if (prev) prev.addEventListener('click', function() { if (usrCurrentPage > 1) { usrCurrentPage--; usrRender(); } });
            if (next) next.addEventListener('click', function() { if (usrCurrentPage < totalPages) { usrCurrentPage++; usrRender(); } });
        }

        function usrBindRows() {
            // intentionally empty — handled by delegated listener below
        }

        // Delegated click for View buttons — runs once, always works
        document.addEventListener('click', function(e) {
            var viewBtn = e.target.closest('.btn-usr-view');
            if (viewBtn) {
                e.stopPropagation();
                usrOpenDetail(parseInt(viewBtn.getAttribute('data-idx')));
            }
        });

        // ── User Detail Panel ─────────────────────────────────
        function usrOpenDetail(idx) {
            var u = allUsers[idx];
            // populate fields
            document.getElementById('detailName').textContent       = u.name;
            document.getElementById('detailEmail').textContent      = u.email;
            document.getElementById('detailPhone').textContent      = u.phone;
            document.getElementById('detailAddress').textContent    = u.address;
            document.getElementById('detailJoined').textContent     = 'Joined on ' + u.joined;
            document.getElementById('detailUserId').textContent     = u.id;
            document.getElementById('detailRole').textContent       = u.role.charAt(0).toUpperCase() + u.role.slice(1);
            document.getElementById('detailLastActive').textContent = u.lastActive;
            document.getElementById('detailPwChange').textContent   = u.pwChange;
            document.getElementById('detailOrders').textContent     = u.orders;
            document.getElementById('detailLookbooks').textContent  = u.lookbooks;
            document.getElementById('detailProducts').textContent   = u.products;
            document.getElementById('detailSpent').textContent      = u.spent;
            document.getElementById('detailWishlist').textContent   = u.wishlist;

            // status badge
            var statusBadge = document.getElementById('detailStatus');
            statusBadge.textContent = u.status.charAt(0).toUpperCase() + u.status.slice(1);
            statusBadge.className = 'usr-overview-status' + (u.status === 'inactive' ? ' usr-overview-status--inactive' : '');

            // status inline
            document.getElementById('detailStatusInline').innerHTML = u.status === 'active'
                ? '<span class="usr-badge usr-badge--active">Active</span>'
                : '<span class="usr-badge usr-badge--inactive">Inactive</span>';

            // past orders — built from allOrders filtered by this user's name
            var userOrders = allOrders.filter(function(o) { return o.name === u.name; });
            var list = document.getElementById('detailOrdersList');
            if (userOrders.length === 0) {
                list.innerHTML = '<p style="font-family:\'Poppins\',sans-serif;font-size:0.85rem;color:#b8929f;padding:1rem 0;">No orders found for this user.</p>';
            } else {
                list.innerHTML = userOrders.map(function(o) {
                    var ordIdx = allOrders.indexOf(o);
                    return '<div class="usr-order-row">' +
                        '<div class="usr-order-imgs">' +
                            '<div class="usr-order-img"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke-width="1.5"><path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/></svg></div>' +
                        '</div>' +
                        '<div class="usr-order-info">' +
                            '<div class="usr-order-id-status">' +
                                '<span class="usr-order-id">Order ' + o.id + '</span>' +
                                '<span class="usr-order-delivered">' + o.status.charAt(0).toUpperCase() + o.status.slice(1) + '</span>' +
                            '</div>' +
                            '<div class="usr-order-meta">' + o.date + ' &bull; ' + o.payment + '</div>' +
                        '</div>' +
                        '<div class="usr-order-right">' +
                            '<div class="usr-order-total-label">Total</div>' +
                            '<div class="usr-order-total-val">' + o.total + '</div>' +
                            '<div class="usr-order-payment">Payment Method &nbsp; ' + o.payment + '</div>' +
                            '<button class="usr-btn-view-order" data-ord-idx="' + ordIdx + '">View Order</button>' +
                        '</div>' +
                    '</div>';
                }).join('');

                // Wire up each View Order button to open the order view modal
                list.querySelectorAll('.usr-btn-view-order').forEach(function(btn) {
                    btn.addEventListener('click', function() {
                        var idx = parseInt(this.getAttribute('data-ord-idx'));
                        var o   = allOrders[idx];
                        if (!o) return;

                        var modal = document.getElementById('usrOrderViewModal');
                        if (!modal) return;

                        document.getElementById('usrOrderViewTitle').textContent = 'Order ' + o.id;
                        document.getElementById('usrOrderViewBody').innerHTML =
                            '<div class="ord-view-row"><span class="ord-view-label">Order ID</span><span class="ord-view-val ord-id">' + o.id + '</span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Customer</span><span class="ord-view-val">' + o.name + '<br><small>' + o.email + '</small></span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Date</span><span class="ord-view-val">' + o.date + ' · ' + o.time + '</span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Status</span><span class="ord-view-val">' + ordBadgeFor(o.status) + '</span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Payment</span><span class="ord-view-val">' + o.payment + '</span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Total</span><span class="ord-view-val ord-total">' + o.total + '</span></div>' +
                            '<div class="ord-view-row"><span class="ord-view-label">Address</span><span class="ord-view-val">' + o.address + '</span></div>';

                        modal.classList.add('ord-modal-overlay--open');
                        document.body.style.overflow = 'hidden';
                    });
                });
            }

            document.getElementById('userDetailOverlay').classList.add('usr-detail-overlay--open');
            document.body.style.overflow = 'hidden';
        }

        function usrCloseDetail() {
            document.getElementById('userDetailOverlay').classList.remove('usr-detail-overlay--open');
            document.body.style.overflow = '';
        }

        document.getElementById('closeDetailBtn').addEventListener('click', usrCloseDetail);
        document.getElementById('goBackBtn').addEventListener('click', usrCloseDetail);

        // Close the order view modal
        function usrCloseOrderModal() {
            var modal = document.getElementById('usrOrderViewModal');
            if (modal) modal.classList.remove('ord-modal-overlay--open');
            document.body.style.overflow = '';
        }
        var closeOVM  = document.getElementById('closeUsrOrderViewModal');
        var closeOVMF = document.getElementById('closeUsrOrderViewFooter');
        var ovm       = document.getElementById('usrOrderViewModal');
        if (closeOVM)  closeOVM.addEventListener('click',  usrCloseOrderModal);
        if (closeOVMF) closeOVMF.addEventListener('click', usrCloseOrderModal);
        if (ovm)       ovm.addEventListener('click', function(e) { if (e.target === ovm) usrCloseOrderModal(); });

        document.getElementById('usrSearch').addEventListener('input',    function() { usrCurrentPage = 1; usrRender(); });
        document.getElementById('roleFilter').addEventListener('change',   function() { usrCurrentPage = 1; usrRender(); });

        usrUpdateStats();
        usrRender();
    }
    // ── End Users Page ────────────────────────────────────────

    // ── Dashboard Page ────────────────────────────────────────
    if (document.getElementById('dashProducts') || document.getElementById('dashLookbooks')) {
        function dashUpdate() {
            // Products: count rows in productTableBody if on same page, else use stored count
            var productRows = document.querySelectorAll('#productTableBody .pm-row');
            var productCount = productRows.length > 0
                ? productRows.length
                : parseInt(sessionStorage.getItem('haruProductCount') || '0');

            // Lookbooks: count list items if present
            var lookbookItems = document.querySelectorAll('.lb-list-item');
            var lookbookCount = lookbookItems.length > 0
                ? lookbookItems.length
                : parseInt(sessionStorage.getItem('haruLookbookCount') || '0');

            // Users: count user rows if present
            var userRows = document.querySelectorAll('.users-table-row');
            var userCount = userRows.length > 0
                ? userRows.length
                : parseInt(sessionStorage.getItem('haruUserCount') || '0');

            var elP = document.getElementById('dashProducts');
            var elL = document.getElementById('dashLookbooks');
            var elU = document.getElementById('dashUsers');
            var elV = document.getElementById('dashViews');

            if (elP) elP.textContent = productCount;
            if (elL) elL.textContent = lookbookCount;
            if (elU) elU.textContent = userCount;
            if (elV) elV.textContent = parseInt(sessionStorage.getItem('haruViews') || '0');
        }
        dashUpdate();
    }

    if (document.getElementById('ordersBody')) {
        document.addEventListener('click', function(e) {
            // Find the closest button that has a data-action attribute
            var btn = e.target.closest('.ord-drop-item');
            if (!btn) return;

            e.stopPropagation();
            var action = btn.getAttribute('data-action');
            var idx    = parseInt(btn.getAttribute('data-idx'));

            if (action === 'ord-view') {
                ordOpenViewModal(idx);
            } else if (action === 'ord-edit') {
                ordOpenEditModal(idx);
            } else if (action === 'ord-delete') {
                ordOpenDeleteModal(idx);
            }
            
            // Close dropdown after action
            document.querySelectorAll('.ord-dropdown').forEach(d => d.classList.remove('ord-dropdown--open'));
        });
    }
    // ── End Dashboard Page ────────────────────────────────────

});
