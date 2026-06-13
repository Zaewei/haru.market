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
    //
    // HOW TO CONNECT FROM RAZOR / BACKEND:
    //
    // Line chart — add data attributes to the <canvas> element:
    //   <canvas id="viewsChart"
    //       data-labels="@string.Join(",", Model.ViewLabels)"
    //       data-values="@string.Join(",", Model.ViewCounts)"
    //       data-max="@Model.ViewMax">
    //   </canvas>
    //
    // Donut chart — add data attributes to the <canvas> element:
    //   <canvas id="usersChart"
    //       data-new="@Model.NewUsers"
    //       data-returning="@Model.ReturningUsers"
    //       data-inactive="@Model.InactiveUsers">
    //   </canvas>
    //
    // ──────────────────────────────────────────────────────────

    const viewsCanvas = document.getElementById('viewsChart');
    const usersCanvas = document.getElementById('usersChart');

    if (viewsCanvas || usersCanvas) {
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js';
        script.onload = function () {

            // ── Lookbook Views — line chart ──
            if (viewsCanvas) {
                // Read from data attributes; fall back to placeholders if not set
                var rawLabels = viewsCanvas.getAttribute('data-labels');
                var rawValues = viewsCanvas.getAttribute('data-values');
                var rawMax    = viewsCanvas.getAttribute('data-max');

                var viewLabels = rawLabels ? rawLabels.split(',').map(function(s) { return s.trim(); })
                                           : ['Week 1', 'Week 2', 'Week 3', 'Week 4', 'Week 5'];
                var viewValues = rawValues ? rawValues.split(',').map(function(s) { return parseFloat(s.trim()); })
                                           : [0, 0, 0, 0, 0];
                var viewMax    = rawMax ? parseInt(rawMax, 10)
                                        : Math.max.apply(null, viewValues) || 10000;

                new Chart(viewsCanvas, {
                    type: 'line',
                    data: {
                        labels: viewLabels,
                        datasets: [{
                            data: viewValues,
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
                        plugins: {
                            legend: { display: false },
                            tooltip: { enabled: true }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                min: 0,
                                max: viewMax,
                                ticks: {
                                    stepSize: Math.ceil(viewMax / 5),
                                    color: '#b8929f',
                                    font: { family: 'Times New Roman' },
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
                                    font: { family: 'Times New Roman' }
                                }
                            }
                        }
                    }
                });
            }

            // ── Users Overview — donut chart ──
            if (usersCanvas) {
                // Read from data attributes; fall back to 0s if not set
                var newUsers       = parseFloat(usersCanvas.getAttribute('data-new'))       || 0;
                var returningUsers = parseFloat(usersCanvas.getAttribute('data-returning')) || 0;
                var inactiveUsers  = parseFloat(usersCanvas.getAttribute('data-inactive'))  || 0;

                new Chart(usersCanvas, {
                    type: 'doughnut',
                    data: {
                        labels: ['New Users', 'Returning Users', 'Inactive Users'],
                        datasets: [{
                            data: [newUsers, returningUsers, inactiveUsers],
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


    // ── Product Management Page ───────────────────────────────
    if (document.getElementById('productTableBody')) {

        var pmNextId = 16;
        var pmEditingId = null;
        var pmDeletingId = null;

        function pmOpenModal(id) {
            var el = document.getElementById(id);
            if (el) el.style.display = 'flex';
        }
        function pmCloseModal(id) {
            var el = document.getElementById(id);
            if (el) el.style.display = 'none';
        }

        // ── Image upload ──
        var pmUploadArea   = document.getElementById('imageUploadArea');
        var pmFileInput    = document.getElementById('productImageFile');
        var pmPreviewImg   = document.getElementById('previewImage');
        var pmPlaceholder  = document.getElementById('imagePlaceholder');
        var pmImageActions = document.getElementById('imageActions');

        function pmShowPreview(src) {
            pmPreviewImg.src                  = src;
            pmPreviewImg.style.display        = 'block';
            pmPlaceholder.style.display       = 'none';
            pmImageActions.style.display      = 'flex';
        }
        function pmClearPreview() {
            pmPreviewImg.src                  = '';
            pmPreviewImg.style.display        = 'none';
            pmPlaceholder.style.display       = 'flex';
            pmImageActions.style.display      = 'none';
            pmFileInput.value                 = '';
        }

        pmUploadArea.addEventListener('click', function (e) {
            if (e.target.closest('#imageActions')) return;
            pmFileInput.click();
        });

        pmFileInput.addEventListener('change', function () {
            var file = pmFileInput.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function (e) { pmShowPreview(e.target.result); };
            reader.readAsDataURL(file);
        });

        document.getElementById('changeImageBtn').addEventListener('click', function (e) {
            e.stopPropagation();
            pmFileInput.click();
        });
        document.getElementById('removeImageBtn').addEventListener('click', function (e) {
            e.stopPropagation();
            pmClearPreview();
        });

        // ── Reset form ──
        function pmResetForm() {
            document.getElementById('productName').value  = '';
            document.getElementById('productColor').value = '';
            document.getElementById('productSize').value  = '';
            document.getElementById('productPrice').value = '';
            document.getElementById('productStock').value = '';
            pmClearPreview();
            pmEditingId = null;
        }

        // ── Open Add modal ──
        document.getElementById('addProductBtn').addEventListener('click', function () {
            pmResetForm();
            document.getElementById('productModalTitle').textContent = 'Add Product';
            document.getElementById('modalDeleteBtn').style.display = 'none';
            document.getElementById('saveProductBtn').textContent = 'Save Product';
            pmOpenModal('productModal');
        });

        // ── Close modals ──
        document.getElementById('closeProductModal').addEventListener('click',       function () { pmCloseModal('productModal'); });
        document.getElementById('cancelProductModal').addEventListener('click',      function () { pmCloseModal('productModal'); });
        document.getElementById('modalDeleteBtn').addEventListener('click', function () {
            if (!pmEditingId) return;
            var row = document.querySelector('.pm-row[data-id="' + pmEditingId + '"]');
            if (row) row.remove();
            pmRender();
            pmCloseModal('productModal');
            pmEditingId = null;
        });
        document.getElementById('closeDeleteProductModal').addEventListener('click', function () { pmCloseModal('deleteProductModal'); });
        document.getElementById('cancelDeleteProduct').addEventListener('click',     function () { pmCloseModal('deleteProductModal'); });

        // Close on overlay click
        document.querySelectorAll('.pm-modal-overlay').forEach(function (overlay) {
            overlay.addEventListener('click', function (e) {
                if (e.target === overlay) overlay.style.display = 'none';
            });
        });

        // ── Edit / Delete (delegated) ──
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]');
            if (!btn) return;
            var action = btn.getAttribute('data-action');
            var id     = btn.getAttribute('data-id');

            if (action === 'pm-edit') {
                var row = document.querySelector('.pm-row[data-id="' + id + '"]');
                if (!row) return;
                pmResetForm();
                pmEditingId = id;
                var cells = row.querySelectorAll('td');
                var imgEl = cells[0].querySelector('img');
                if (imgEl) pmShowPreview(imgEl.src);
                document.getElementById('productName').value  = cells[1].querySelector('.pm-name').textContent.trim();
                document.getElementById('productColor').value = cells[2].querySelector('.pm-meta').textContent.trim();
                document.getElementById('productSize').value  = cells[3].querySelector('.pm-meta').textContent.trim();
                document.getElementById('productPrice').value = cells[4].querySelector('.pm-price').textContent.replace('₱', '').trim();
                document.getElementById('productStock').value = cells[5].querySelector('.pm-meta').textContent.trim();
                document.getElementById('productModalTitle').textContent = 'Edit Product';
                document.getElementById('modalDeleteBtn').style.display = 'inline-flex';
                document.getElementById('saveProductBtn').textContent = 'Save Changes';
                pmOpenModal('productModal');
            }

            if (action === 'pm-delete') {
                pmDeletingId = id;
                var row  = document.querySelector('.pm-row[data-id="' + id + '"]');
                var name = row ? row.querySelector('.pm-name').textContent.trim() : 'this product';
                document.getElementById('deleteProductName').textContent = name;
                pmOpenModal('deleteProductModal');
            }
        });

        // ── Save (Add or Edit) ──
        document.getElementById('saveProductBtn').addEventListener('click', function () {
            var name   = document.getElementById('productName').value.trim();
            var color  = document.getElementById('productColor').value.trim();
            var size   = document.getElementById('productSize').value;
            var price  = document.getElementById('productPrice').value.trim();
            var stock  = document.getElementById('productStock').value.trim();
            var imgSrc = pmPreviewImg.style.display !== 'none' ? pmPreviewImg.src : '';

            if (!name || !color || !size || !price || !stock) {
                alert('Please fill in all fields.'); return;
            }

            if (pmEditingId) {
                var row   = document.querySelector('.pm-row[data-id="' + pmEditingId + '"]');
                if (row) {
                    var cells = row.querySelectorAll('td');
                    pmUpdateThumbCell(cells[0], imgSrc);
                    cells[1].querySelector('.pm-name').textContent  = name;
                    cells[2].querySelector('.pm-meta').textContent  = color;
                    cells[3].querySelector('.pm-meta').textContent  = size;
                    cells[4].querySelector('.pm-price').textContent = '₱' + price;
                    cells[5].querySelector('.pm-meta').textContent  = stock;
                }
            } else {
                var newId  = pmNextId++;
                var tbody  = document.getElementById('productTableBody');
                var tr     = document.createElement('tr');
                tr.className = 'pm-row';
                tr.setAttribute('data-id', newId);
                tr.innerHTML = pmBuildRowHTML(newId, imgSrc, name, color, size, price, stock);
                tbody.appendChild(tr);
                pmRender();
            }
            pmCloseModal('productModal');
        });

        // ── Confirm Delete ──
        document.getElementById('confirmDeleteProduct').addEventListener('click', function () {
            if (!pmDeletingId) return;
            var row = document.querySelector('.pm-row[data-id="' + pmDeletingId + '"]');
            if (row) row.remove();
            pmRender();
            pmCloseModal('deleteProductModal');
            pmDeletingId = null;
        });

        // ── Pagination ──
        var pmCurrentPage = 1;
        var pmPerPage     = 5;

        function pmGetAllRows() {
            return Array.from(document.querySelectorAll('#productTableBody .pm-row'));
        }

        function pmTotalPages() {
            return Math.max(1, Math.ceil(pmGetAllRows().length / pmPerPage));
        }

        function pmRender() {
            var rows  = pmGetAllRows();
            var total = Math.max(1, Math.ceil(rows.length / pmPerPage));
            if (pmCurrentPage > total) pmCurrentPage = total;

            // Show/hide rows
            rows.forEach(function (row, i) {
                var page = Math.floor(i / pmPerPage) + 1;
                row.style.display = (page === pmCurrentPage) ? '' : 'none';
            });

            // Rebuild pagination bar
            var bar = document.getElementById('pmPaginationBar');
            if (!bar) return;
            bar.innerHTML = '';

            // Previous
            var prev = document.createElement('button');
            prev.className   = 'pm-page-btn';
            prev.textContent = 'Previous';
            prev.disabled    = pmCurrentPage === 1;
            prev.addEventListener('click', function () { pmGoTo(pmCurrentPage - 1); });
            bar.appendChild(prev);

            // Page numbers
            for (var p = 1; p <= total; p++) {
                (function(page) {
                    var btn = document.createElement('button');
                    btn.className   = 'pm-page-num' + (page === pmCurrentPage ? ' active' : '');
                    btn.textContent = page;
                    btn.addEventListener('click', function () { pmGoTo(page); });
                    bar.appendChild(btn);
                })(p);
            }

            // Next
            var next = document.createElement('button');
            next.className   = 'pm-page-btn';
            next.textContent = 'Next';
            next.disabled    = pmCurrentPage === total;
            next.addEventListener('click', function () { pmGoTo(pmCurrentPage + 1); });
            bar.appendChild(next);
        }

        function pmGoTo(page) {
            var total = pmTotalPages();
            pmCurrentPage = Math.max(1, Math.min(page, total));
            pmRender();
        }

        pmRender();

        function pmBuildRowHTML(id, imgSrc, name, color, size, price, stock) {
            var thumbHTML = imgSrc
                ? '<img src="' + imgSrc + '" class="pm-thumb" alt="' + name + '" />'
                : '<div class="pm-thumb pm-thumb-placeholder"><span style="color:#b8929f;font-size:0.6rem;font-family:\'Times New Roman\',serif;">IMG</span></div>';
            return '<td><div class="pm-thumb-wrap">' + thumbHTML + '</div></td>'
                + '<td><span class="pm-name">' + name + '</span></td>'
                + '<td><span class="pm-meta">' + color + '</span></td>'
                + '<td><span class="pm-meta">' + size + '</span></td>'
                + '<td><span class="pm-price">₱' + price + '</span></td>'
                + '<td><span class="pm-meta">' + stock + '</span></td>'
                + '<td class="pm-actions">'
                + '<button class="btn-edit" data-action="pm-edit" data-id="' + id + '">Edit</button>'
                + '</td>';
        }

        function pmUpdateThumbCell(cell, imgSrc) {
            var wrap = cell.querySelector('.pm-thumb-wrap');
            if (!wrap) return;
            if (imgSrc) {
                var existing = wrap.querySelector('img.pm-thumb');
                if (existing) {
                    existing.src = imgSrc;
                } else {
                    wrap.innerHTML = '<img src="' + imgSrc + '" class="pm-thumb" alt="" />';
                }
            } else {
                // Image was removed — show placeholder
                wrap.innerHTML = '<div class="pm-thumb pm-thumb-placeholder"><span style="color:#b8929f;font-size:0.6rem;">IMG</span></div>';
            }
        }

    }
    // ── End Product Management ────────────────────────────────

    // ── Shared order data (used by Orders page AND Users page) ──
    // ── Orders Page ───────────────────────────────────────────
    //
    // HOW TO CONNECT FROM RAZOR / BACKEND:
    //
    // Render each order as a <tr> inside <tbody id="ordersBody">.
    // Each <tr> must carry data attributes so JS can read the order:
    //
    //   <tbody id="ordersBody">
    //     @foreach (var o in Model.Orders) {
    //       <tr class="ord-row"
    //           data-id="@o.Id"
    //           data-name="@o.CustomerName"
    //           data-email="@o.CustomerEmail"
    //           data-date="@o.Date.ToString("MMM d, yyyy")"
    //           data-time="@o.Date.ToString("hh:mm tt")"
    //           data-status="@o.Status.ToLower()"
    //           data-payment="@o.PaymentMethod"
    //           data-total="@o.Total.ToString("N0")"
    //           data-address="@o.ShippingAddress">
    //         <!-- cells are rendered by JS; you may leave the <tr> empty -->
    //       </tr>
    //     }
    //   </tbody>
    //
    // Stat cards are derived from the rows automatically — no extra markup needed.
    // ──────────────────────────────────────────────────────────

    // Only runs when the orders table is present on the page.
    if (document.getElementById('ordersBody')) {

        // ── Read orders from DOM rows ──────────────────────────
        function ordReadFromDOM() {
            return Array.from(document.querySelectorAll('#ordersBody .ord-row')).map(function(tr) {
                return {
                    id:      tr.getAttribute('data-id')      || '',
                    name:    tr.getAttribute('data-name')     || '',
                    email:   tr.getAttribute('data-email')    || '',
                    date:    tr.getAttribute('data-date')     || '',
                    time:    tr.getAttribute('data-time')     || '',
                    status:  tr.getAttribute('data-status')   || '',
                    payment: tr.getAttribute('data-payment')  || '',
                    total:   tr.getAttribute('data-total')    || '',
                    address: tr.getAttribute('data-address')  || ''
                };
            });
        }

        var allOrders          = ordReadFromDOM();
        var ordPageSize        = 10;
        var ordCurrentPage     = 1;
        var ordActiveEditIdx   = -1;
        var ordActiveDeleteIdx = -1;

        function ordFiltered() {
            var q       = document.getElementById('ordSearch').value.toLowerCase();
            var status  = document.getElementById('statusFilter').value;
            var dateEl  = document.getElementById('dateFilter');
            var dateVal = dateEl ? dateEl.value : '';
            return allOrders.filter(function(o) {
                var matchQ = !q || o.id.toLowerCase().includes(q) || o.name.toLowerCase().includes(q) || o.email.toLowerCase().includes(q);
                var matchS = !status || o.status === status;
                var matchD = !dateVal || ordMonthKey(o.date) === dateVal;
                return matchQ && matchS && matchD;
            });
        }

        // Returns "Month YYYY" key from a date string like "May 21, 2026"
        function ordMonthKey(dateStr) {
            if (!dateStr) return '';
            var parts = dateStr.split(' ');
            return parts[0] + ' ' + parts[2];
        }

        // Populate date dropdown from unique months in allOrders
        function ordPopulateDateFilter() {
            var el = document.getElementById('dateFilter');
            if (!el) return;
            var seen = {}, months = [];
            allOrders.forEach(function(o) {
                var key = ordMonthKey(o.date);
                if (key && !seen[key]) { seen[key] = true; months.push(key); }
            });
            months.sort(function(a, b) { return new Date(b) - new Date(a); });
            el.innerHTML = '<option value="">All Dates</option>';
            months.forEach(function(m) {
                var opt = document.createElement('option');
                opt.value = m; opt.textContent = m;
                el.appendChild(opt);
            });
        }

        function ordUpdateStats() {
            var total   = allOrders.length;
            var revenue = allOrders.reduce(function(sum, o) {
                return sum + (parseFloat(String(o.total).replace(/[^0-9.]/g, '')) || 0);
            }, 0);
            var pending = allOrders.filter(function(o) { return o.status === 'pending'; }).length;
            var shipped = allOrders.filter(function(o) { return o.status === 'shipped'; }).length;

            var elTotal   = document.getElementById('statTotalOrders');
            var elRev     = document.getElementById('statTotalRevenue');
            var elPending = document.getElementById('statPendingOrders');
            var elShipped = document.getElementById('statShippedOrders');

            if (elTotal)   elTotal.textContent  = total;
            if (elRev)     elRev.textContent     = '\u20b1' + revenue.toLocaleString();
            if (elPending) elPending.textContent = pending;
            if (elShipped) elShipped.textContent = shipped;
        }

        function ordRender() {
            var orders     = ordFiltered();
            var totalPages = Math.max(1, Math.ceil(orders.length / ordPageSize));
            if (ordCurrentPage > totalPages) ordCurrentPage = totalPages;
            var start = (ordCurrentPage - 1) * ordPageSize;
            var page  = orders.slice(start, start + ordPageSize);

            var tbody = document.getElementById('ordersBody');
            var empty = document.getElementById('ordEmpty');

            if (page.length === 0) {
                tbody.innerHTML = '';
                if (empty) empty.style.display = 'block';
            } else {
                if (empty) empty.style.display = 'none';
                tbody.innerHTML = page.map(function(o) {
                    var idx = allOrders.indexOf(o);
                    return '<tr class="ord-row"' +
                        ' data-id="'      + o.id      + '"' +
                        ' data-name="'    + o.name    + '"' +
                        ' data-email="'   + o.email   + '"' +
                        ' data-date="'    + o.date    + '"' +
                        ' data-time="'    + o.time    + '"' +
                        ' data-status="'  + o.status  + '"' +
                        ' data-payment="' + o.payment + '"' +
                        ' data-total="'   + o.total   + '"' +
                        ' data-address="' + o.address + '">' +
                        '<td class="ord-id">' + o.id + '</td>' +
                        '<td><div class="ord-customer-name">' + o.name + '</div><div class="ord-customer-email">' + o.email + '</div></td>' +
                        '<td><div class="ord-date">' + o.date + '</div><div class="ord-time">' + o.time + '</div></td>' +
                        '<td>' + ordBadgeFor(o.status) + '</td>' +
                        '<td class="ord-payment">' + o.payment + '</td>' +
                        '<td class="ord-total">\u20b1' + o.total + '</td>' +
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
            document.getElementById('viewModalBody').innerHTML =
                '<div class="ord-view-row"><span class="ord-view-label">Order ID</span><span class="ord-view-val ord-id">' + o.id + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Customer</span><span class="ord-view-val">' + o.name + '<br><small>' + o.email + '</small></span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Date</span><span class="ord-view-val">' + o.date + ' \u00b7 ' + o.time + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Status</span><span class="ord-view-val">' + ordBadgeFor(o.status) + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Payment</span><span class="ord-view-val">' + o.payment + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Total</span><span class="ord-view-val ord-total">\u20b1' + o.total + '</span></div>' +
                '<div class="ord-view-row"><span class="ord-view-label">Address</span><span class="ord-view-val">' + o.address + '</span></div>';
            ordOpenModal('viewOrderModal');
        }

        function ordOpenEditModal(idx) {
            ordActiveEditIdx = idx;
            var o = allOrders[idx];
            document.getElementById('editModalOrderId').textContent  = o.id;
            document.getElementById('editModalCustomer').textContent = o.name;
            document.getElementById('editStatusSelect').value        = o.status;
            ordOpenModal('editOrderModal');
        }
        document.getElementById('saveEditModal').addEventListener('click', function() {
            if (ordActiveEditIdx < 0) return;
            allOrders[ordActiveEditIdx].status = document.getElementById('editStatusSelect').value;
            ordCloseModal('editOrderModal');
            ordRender();
            ordUpdateStats();
            ordShowToast('Order status updated.');
        });

        function ordOpenDeleteModal(idx) {
            ordActiveDeleteIdx = idx;
            document.getElementById('deleteModalOrderId').textContent = allOrders[idx].id;
            ordOpenModal('deleteOrderModal');
        }
        document.getElementById('confirmDeleteModal').addEventListener('click', function() {
            if (ordActiveDeleteIdx < 0) return;
            allOrders.splice(ordActiveDeleteIdx, 1);
            ordActiveDeleteIdx = -1;
            ordCloseModal('deleteOrderModal');
            ordRender();
            ordUpdateStats();
            ordShowToast('Order deleted.');
        });

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

        // \u2500\u2500 HOW TO CONNECT FROM RAZOR / BACKEND \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500
        //
        // Render each user as a <tr> inside <tbody id="usersBody">.
        // Each <tr> must carry data attributes so JS can read the user:
        //
        //   <tbody id="usersBody">
        //     @foreach (var u in Model.Users) {
        //       <tr class="usr-row"
        //           data-id="@u.Id"
        //           data-name="@u.FullName"
        //           data-email="@u.Email"
        //           data-phone="@u.Phone"
        //           data-address="@u.Address"
        //           data-joined="@u.JoinedDate.ToString('MMM d, yyyy')"
        //           data-last-active="@u.LastActive"
        //           data-role="@u.Role.ToLower()"
        //           data-status="@u.Status.ToLower()"
        //           data-pw-change="@u.LastPasswordChange.ToString('MMM d, yyyy')"
        //           data-orders="@u.OrderCount"
        //           data-lookbooks="@u.LookbookCount"
        //           data-products="@u.ProductCount"
        //           data-spent="@u.TotalSpent.ToString('N0')"
        //           data-wishlist="@u.WishlistCount">
        //         <!-- cells are rendered by JS; you may leave the <tr> empty -->
        //       </tr>
        //     }
        //   </tbody>
        //
        // Stat cards (total, active, inactive, new this month) are derived
        // from the rows automatically \u2014 no extra markup needed.
        // \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        // \u2500\u2500 Read users from DOM rows \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500
        function usrReadFromDOM() {
            return Array.from(document.querySelectorAll('#usersBody .usr-row')).map(function(tr) {
                return {
                    id:         tr.getAttribute('data-id')          || '',
                    name:       tr.getAttribute('data-name')        || '',
                    email:      tr.getAttribute('data-email')       || '',
                    phone:      tr.getAttribute('data-phone')       || '',
                    address:    tr.getAttribute('data-address')     || '',
                    joined:     tr.getAttribute('data-joined')      || '',
                    lastActive: tr.getAttribute('data-last-active') || '',
                    role:       tr.getAttribute('data-role')        || '',
                    status:     tr.getAttribute('data-status')      || '',
                    pwChange:   tr.getAttribute('data-pw-change')   || '',
                    orders:     parseInt(tr.getAttribute('data-orders'))    || 0,
                    lookbooks:  parseInt(tr.getAttribute('data-lookbooks')) || 0,
                    products:   parseInt(tr.getAttribute('data-products'))  || 0,
                    spent:      tr.getAttribute('data-spent')       || '0',
                    wishlist:   parseInt(tr.getAttribute('data-wishlist'))  || 0
                };
            });
        }

        var allUsers = usrReadFromDOM();

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

            // past orders — read from DOM rows filtered by data-email matching this user
            // TIP: for a more reliable match, add data-user-id="@o.UserId" to each .ord-row
            // and filter by u.id instead of u.email
            var allOrderRows = Array.from(document.querySelectorAll('#ordersBody .ord-row'));
            var userOrders = allOrders.filter(function(o) { return o.email === u.email; });
            // If orders page isn't loaded, fall back to reading fresh from any visible ord-rows
            if (allOrderRows.length && !userOrders.length) {
                userOrders = allOrderRows
                    .filter(function(tr) { return tr.getAttribute('data-email') === u.email; })
                    .map(function(tr) {
                        return {
                            id:      tr.getAttribute('data-id')      || '',
                            name:    tr.getAttribute('data-name')     || '',
                            email:   tr.getAttribute('data-email')    || '',
                            date:    tr.getAttribute('data-date')     || '',
                            time:    tr.getAttribute('data-time')     || '',
                            status:  tr.getAttribute('data-status')   || '',
                            payment: tr.getAttribute('data-payment')  || '',
                            total:   tr.getAttribute('data-total')    || '',
                            address: tr.getAttribute('data-address')  || ''
                        };
                    });
            }
            var list = document.getElementById('detailOrdersList');
            if (userOrders.length === 0) {
                list.innerHTML = '<p style="font-family:\'Poppins\',sans-serif;font-size:0.85rem;color:#b8929f;padding:1rem 0;">No orders found for this user.</p>';
            } else {
                list.innerHTML = userOrders.map(function(o) {
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
                            '<div class="usr-order-total-val">\u20b1' + o.total + '</div>' +
                            '<div class="usr-order-payment">Payment Method &nbsp; ' + o.payment + '</div>' +
                            '<button class="usr-btn-view-order" data-ord-id="' + o.id + '">View Order</button>' +
                        '</div>' +
                    '</div>';
                }).join('');

                // Wire up each View Order button — look up order by ID, not array index
                list.querySelectorAll('.usr-btn-view-order').forEach(function(btn) {
                    btn.addEventListener('click', function() {
                        var ordId = this.getAttribute('data-ord-id');
                        var o     = allOrders.filter(function(x) { return x.id === ordId; })[0];
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
    // ── End Dashboard Page ────────────────────────────────────

});
