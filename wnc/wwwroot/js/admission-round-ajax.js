let currentPage = 1;
let currentPageSize = 10;
let modalMode = 'create';
let editingRoundId = null;

document.addEventListener('DOMContentLoaded', function () {
    loadFilters();
    loadRounds(currentPage);
    setupEventListeners();
});

function setupEventListeners() {
    document.getElementById('btnCreate').addEventListener('click', openCreateModal);
    document.getElementById('btnSave').addEventListener('click', saveRound);
    document.getElementById('btnConfirmDelete').addEventListener('click', confirmDelete);
}

function handleSearchKeypress(event) {
    if (event.key === 'Enter') {
        searchRounds();
    }
}

function changePageSize() {
    currentPageSize = parseInt(document.getElementById('pageSize').value);
    currentPage = 1;
    loadRounds(currentPage);
}

function searchRounds() {
    currentPage = 1;
    loadRounds(currentPage);
}

function goToPage(page) {
    currentPage = page;
    loadRounds(page);
}

// =============================================================================
// METHOD 1: XmlHttpRequest - Dùng cho nút Refresh
// =============================================================================
function refreshWithXHR() {
    showLoading();

    const xhr = new XMLHttpRequest();
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const year = document.getElementById('filterYear').value;

    const url = `/AdmissionRound/GetAdmissionRounds?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&admissionYear=${year}&page=${currentPage}&pageSize=${currentPageSize}`;

    xhr.open('GET', url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                const data = JSON.parse(xhr.responseText);
                renderRounds(data);
                renderPagination(data);
                updateShowingInfo(data);
                showToast('Data refreshed using XmlHttpRequest!', 'success');
            } else {
                showError('Error loading rounds: ' + xhr.statusText);
            }
        }
    };

    xhr.onerror = function () {
        showError('Network error occurred');
    };

    xhr.send();
}

// =============================================================================
// METHOD 2: Fetch API - Dùng cho Search và Pagination
// =============================================================================
async function loadRounds(page) {
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const year = document.getElementById('filterYear').value;

    const url = `/AdmissionRound/GetAdmissionRounds?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&admissionYear=${year}&page=${page}&pageSize=${currentPageSize}`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        renderRounds(data);
        renderPagination(data);
        updateShowingInfo(data);
    } catch (error) {
        console.error('Error:', error);
        showError('Failed to load rounds: ' + error.message);
    }
}

// =============================================================================
// METHOD 3: jQuery AJAX - Dùng cho CRUD operations
// =============================================================================
function loadFilters() {
    $.ajax({
        url: '/AdmissionRound/GetAdmissionYears',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            const select = document.getElementById('filterYear');
            data.forEach(function (item) {
                const option = document.createElement('option');
                option.value = item.value;
                option.textContent = item.text;
                select.appendChild(option);
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading years:', error);
        }
    });
}

function openCreateModal() {
    modalMode = 'create';
    editingRoundId = null;
    document.getElementById('modalTitle').textContent = 'Create Admission Round';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Save (jQuery AJAX)';

    $.ajax({
        url: '/AdmissionRound/GetStatuses',
        method: 'GET',
        dataType: 'json',
        success: function (statuses) {
            const html = generateFormHtml(null, statuses);
            document.getElementById('modalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('roundModal'));
            modal.show();
        },
        error: function () {
            showError('Error loading form data');
        }
    });
}

function openEditModal(roundId) {
    modalMode = 'edit';
    editingRoundId = roundId;
    document.getElementById('modalTitle').textContent = 'Edit Admission Round';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Update (jQuery AJAX)';

    $.ajax({
        url: '/AdmissionRound/GetById',
        method: 'GET',
        data: { id: roundId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                $.ajax({
                    url: '/AdmissionRound/GetStatuses',
                    method: 'GET',
                    dataType: 'json',
                    success: function (statuses) {
                        const html = generateFormHtml(response.data, statuses);
                        document.getElementById('modalBody').innerHTML = html;
                        const modal = new bootstrap.Modal(document.getElementById('roundModal'));
                        modal.show();
                    }
                });
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError('Error loading round data');
        }
    });
}

function openDetailModal(roundId) {
    $.ajax({
        url: '/AdmissionRound/GetById',
        method: 'GET',
        data: { id: roundId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                const r = response.data;
                const html = `
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Code:</strong> ${escapeHtml(r.roundCode)}</p>
                            <p><strong>Name:</strong> ${escapeHtml(r.roundName)}</p>
                            <p><strong>Year:</strong> ${r.admissionYear}</p>
                            <p><strong>Status:</strong> ${getStatusBadge(r.status)}</p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Start Date:</strong> ${formatDate(r.startAt)}</p>
                            <p><strong>End Date:</strong> ${formatDate(r.endAt)}</p>
                            <p><strong>Enrollment Confirmation:</strong> ${r.allowEnrollmentConfirmation ? 'Yes' : 'No'}</p>
                        </div>
                    </div>
                    ${r.notes ? `
                    <div class="row mt-2">
                        <div class="col-12">
                            <p><strong>Notes:</strong></p>
                            <p class="text-muted">${escapeHtml(r.notes)}</p>
                        </div>
                    </div>
                    ` : ''}
                `;
                document.getElementById('detailModalBody').innerHTML = html;
                const modal = new bootstrap.Modal(document.getElementById('detailModal'));
                modal.show();
            } else {
                showError(response.message);
            }
        }
    });
}

function generateFormHtml(round, statuses) {
    const isEdit = round !== null;
    const r = round || {};

    let statusOptions = '';
    statuses.forEach(function (s) {
        const selected = (r.status === s.value) ? 'selected' : '';
        statusOptions += `<option value="${s.value}" ${selected}>${s.text}</option>`;
    });

    const currentYear = new Date().getFullYear();
    const currentMonth = String(new Date().getMonth() + 1).padStart(2, '0');
    const currentDay = String(new Date().getDate()).padStart(2, '0');
    const today = `${currentYear}-${currentMonth}-${currentDay}`;

    const nextMonth = new Date(currentYear, new Date().getMonth() + 1, 1);
    const nextMonthStr = nextMonth.toISOString().slice(0, 10);

    return `
        <form id="roundForm">
            <input type="hidden" id="roundId" value="${isEdit ? r.id : ''}">
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Round Code <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="roundCode" value="${r.roundCode || ''}" required maxlength="50">
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Round Name <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="roundName" value="${r.roundName || ''}" required maxlength="255">
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <div class="mb-3">
                        <label class="form-label">Admission Year <span class="text-danger">*</span></label>
                        <input type="number" class="form-control" id="admissionYear" value="${r.admissionYear || currentYear}" required min="2020" max="2100">
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="mb-3">
                        <label class="form-label">Start Date <span class="text-danger">*</span></label>
                        <input type="date" class="form-control" id="startAt" value="${r.startAt ? r.startAt.split('T')[0] : today}" required>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="mb-3">
                        <label class="form-label">End Date <span class="text-danger">*</span></label>
                        <input type="date" class="form-control" id="endAt" value="${r.endAt ? r.endAt.split('T')[0] : nextMonthStr}" required>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Status</label>
                        <select class="form-select" id="status">
                            ${statusOptions}
                        </select>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">&nbsp;</label>
                        <div class="form-check mt-2">
                            <input class="form-check-input" type="checkbox" id="allowEnrollmentConfirmation" ${r.allowEnrollmentConfirmation ? 'checked' : ''}>
                            <label class="form-check-label" for="allowEnrollmentConfirmation">
                                Allow Enrollment Confirmation
                            </label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="mb-3">
                <label class="form-label">Notes</label>
                <textarea class="form-control" id="notes" rows="3">${r.notes || ''}</textarea>
            </div>
        </form>
    `;
}

function saveRound() {
    const form = document.getElementById('roundForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const startAt = new Date(document.getElementById('startAt').value);
    const endAt = new Date(document.getElementById('endAt').value);

    if (endAt <= startAt) {
        showError('End date must be after start date');
        return;
    }

    const data = {
        roundCode: document.getElementById('roundCode').value,
        roundName: document.getElementById('roundName').value,
        admissionYear: parseInt(document.getElementById('admissionYear').value),
        startAt: document.getElementById('startAt').value,
        endAt: document.getElementById('endAt').value,
        status: document.getElementById('status').value,
        notes: document.getElementById('notes').value,
        allowEnrollmentConfirmation: document.getElementById('allowEnrollmentConfirmation').checked
    };

    if (modalMode === 'create') {
        $.ajax({
            url: '/AdmissionRound/Create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('roundModal')).hide();
                    showSuccess('Admission round created successfully!');
                    loadRounds(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error creating round: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    } else {
        data.id = document.getElementById('roundId').value;
        $.ajax({
            url: '/AdmissionRound/Edit',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('roundModal')).hide();
                    showSuccess('Admission round updated successfully!');
                    loadRounds(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error updating round: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    }
}

function openDeleteModal(roundId, roundName) {
    document.getElementById('deleteRoundId').value = roundId;
    document.getElementById('deleteRoundName').textContent = roundName;
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    modal.show();
}

function confirmDelete() {
    const roundId = document.getElementById('deleteRoundId').value;

    $.ajax({
        url: '/AdmissionRound/Delete',
        method: 'POST',
        data: { id: roundId },
        success: function (response) {
            if (response.success) {
                bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
                showSuccess('Admission round deleted successfully!');
                loadRounds(currentPage);
            } else {
                showError(response.message);
            }
        },
        error: function (xhr) {
            showError('Error deleting round: ' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

// =============================================================================
// RENDER FUNCTIONS
// =============================================================================
function renderRounds(data) {
    const tbody = document.getElementById('roundTableBody');

    if (!data.items || data.items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="bi bi-inbox" style="font-size: 2rem;"></i>
                    <p class="mt-2">No admission rounds found</p>
                </td>
            </tr>
        `;
        return;
    }

    let html = '';
    data.items.forEach(function (round) {
        const statusBadge = getStatusBadge(round.status);

        html += `
            <tr>
                <td><strong>${escapeHtml(round.roundCode)}</strong></td>
                <td>${escapeHtml(round.roundName)}</td>
                <td><span class="badge bg-primary">${round.admissionYear}</span></td>
                <td>${escapeHtml(round.dateRangeText)}</td>
                <td>${statusBadge}</td>
                <td class="text-center">
                    <span class="badge bg-info">${round.programCount}</span>
                </td>
                <td class="text-center">
                    <span class="badge bg-secondary">${round.applicationCount}</span>
                </td>
                <td>
                    <button class="btn btn-sm btn-info me-1" onclick="openDetailModal('${round.id}')" title="View Details">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-primary me-1" onclick="openEditModal('${round.id}')" title="Edit">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-dark" onclick="openDeleteModal('${round.id}', '${escapeHtml(round.roundName)}')" title="Delete">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.innerHTML = html;
}

function renderPagination(data) {
    const pagination = document.getElementById('pagination');
    let html = '';

    const disabledPrev = !data.hasPreviousPage ? 'disabled' : '';
    html += `
        <li class="page-item ${disabledPrev}">
            <a class="page-link" href="#" onclick="goToPage(${data.pageNumber - 1}); return false;">Previous</a>
        </li>
    `;

    const maxVisible = 5;
    let startPage = Math.max(1, data.pageNumber - Math.floor(maxVisible / 2));
    let endPage = Math.min(data.totalPages, startPage + maxVisible - 1);

    if (endPage - startPage < maxVisible - 1) {
        startPage = Math.max(1, endPage - maxVisible + 1);
    }

    if (startPage > 1) {
        html += `<li class="page-item"><a class="page-link" href="#" onclick="goToPage(1); return false;">1</a></li>`;
        if (startPage > 2) {
            html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        }
    }

    for (let i = startPage; i <= endPage; i++) {
        const active = i === data.pageNumber ? 'active' : '';
        html += `
            <li class="page-item ${active}">
                <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
            </li>
        `;
    }

    if (endPage < data.totalPages) {
        if (endPage < data.totalPages - 1) {
            html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        }
        html += `<li class="page-item"><a class="page-link" href="#" onclick="goToPage(${data.totalPages}); return false;">${data.totalPages}</a></li>`;
    }

    const disabledNext = !data.hasNextPage ? 'disabled' : '';
    html += `
        <li class="page-item ${disabledNext}">
            <a class="page-link" href="#" onclick="goToPage(${data.pageNumber + 1}); return false;">Next</a>
        </li>
    `;

    pagination.innerHTML = html;
}

function updateShowingInfo(data) {
    const from = (data.pageNumber - 1) * data.pageSize + 1;
    const to = Math.min(data.pageNumber * data.pageSize, data.totalCount);

    document.getElementById('showingFrom').textContent = data.totalCount > 0 ? from : 0;
    document.getElementById('showingTo').textContent = to;
    document.getElementById('totalCount').textContent = data.totalCount;
}

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================
function getStatusBadge(status) {
    const badges = {
        'DRAFT': '<span class="badge bg-secondary">Nháp</span>',
        'PUBLISHED': '<span class="badge bg-success">Đã đăng tải</span>',
        'CLOSED': '<span class="badge bg-danger">Đã đóng</span>',
        'ARCHIVED': '<span class="badge bg-dark">Đã lưu trữ</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN');
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showLoading() {
    const tbody = document.getElementById('roundTableBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center text-muted py-4">
                <div class="spinner-border text-danger" role="status"></div>
                <p class="mt-2">Loading...</p>
            </td>
        </tr>
    `;
}

function showSuccess(message) {
    showToast(message, 'success');
}

function showError(message) {
    showToast(message, 'danger');
}

function showToast(message, type) {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
        <i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), type === 'success' ? 3000 : 5000);
}
