let currentPage = 1;
let currentPageSize = 10;
let modalMode = 'create';
let editingProgramId = null;

document.addEventListener('DOMContentLoaded', function () {
    loadFilters();
    loadPrograms(currentPage);
    setupEventListeners();
});

function setupEventListeners() {
    document.getElementById('btnCreate').addEventListener('click', openCreateModal);
    document.getElementById('btnSave').addEventListener('click', saveProgram);
    document.getElementById('btnConfirmDelete').addEventListener('click', confirmDelete);
}

function handleSearchKeypress(event) {
    if (event.key === 'Enter') {
        searchPrograms();
    }
}

function changePageSize() {
    currentPageSize = parseInt(document.getElementById('pageSize').value);
    currentPage = 1;
    loadPrograms(currentPage);
}

function searchPrograms() {
    currentPage = 1;
    loadPrograms(currentPage);
}

function goToPage(page) {
    currentPage = page;
    loadPrograms(page);
}

// =============================================================================
// METHOD 1: XmlHttpRequest - Used for Refresh button
// =============================================================================
function refreshWithXHR() {
    showLoading();

    const xhr = new XMLHttpRequest();
    const searchTerm = document.getElementById('searchTerm').value;
    const educationType = document.getElementById('filterEducationType').value;
    const status = document.getElementById('filterStatus').value;

    const url = `/TrainingProgram/GetTrainingPrograms?searchTerm=${encodeURIComponent(searchTerm)}&educationType=${educationType}&status=${status}&page=${currentPage}&pageSize=${currentPageSize}`;

    xhr.open('GET', url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                const data = JSON.parse(xhr.responseText);
                renderPrograms(data);
                renderPagination(data);
                updateShowingInfo(data);
                showToast('Data refreshed using XmlHttpRequest!', 'success');
            } else {
                showError('Error loading programs: ' + xhr.statusText);
            }
        }
    };

    xhr.onerror = function () {
        showError('Network error occurred');
    };

    xhr.send();
}

// =============================================================================
// METHOD 2: Fetch API - Used for Search and Pagination
// =============================================================================
async function loadPrograms(page) {
    const searchTerm = document.getElementById('searchTerm').value;
    const educationType = document.getElementById('filterEducationType').value;
    const status = document.getElementById('filterStatus').value;

    const url = `/TrainingProgram/GetTrainingPrograms?searchTerm=${encodeURIComponent(searchTerm)}&educationType=${educationType}&status=${status}&page=${page}&pageSize=${currentPageSize}`;

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
        renderPrograms(data);
        renderPagination(data);
        updateShowingInfo(data);
    } catch (error) {
        console.error('Error:', error);
        showError('Failed to load programs: ' + error.message);
    }
}

// =============================================================================
// METHOD 3: jQuery AJAX - Used for CRUD operations
// =============================================================================
function loadFilters() {
    $.ajax({
        url: '/TrainingProgram/GetEducationTypes',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            const select = document.getElementById('filterEducationType');
            data.forEach(function (type) {
                const option = document.createElement('option');
                option.value = type.value;
                option.textContent = type.text;
                select.appendChild(option);
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading education types:', error);
        }
    });
}

function openCreateModal() {
    modalMode = 'create';
    editingProgramId = null;
    document.getElementById('modalTitle').textContent = 'Create Training Program';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Save (jQuery AJAX)';

    $.ajax({
        url: '/TrainingProgram/GetStatuses',
        method: 'GET',
        dataType: 'json',
        success: function (statuses) {
            const html = generateFormHtml(null, statuses);
            document.getElementById('modalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('programModal'));
            modal.show();
        },
        error: function () {
            showError('Error loading form data');
        }
    });
}

function openEditModal(programId) {
    modalMode = 'edit';
    editingProgramId = programId;
    document.getElementById('modalTitle').textContent = 'Edit Training Program';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Update (jQuery AJAX)';

    $.ajax({
        url: '/TrainingProgram/GetById',
        method: 'GET',
        data: { id: programId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                $.ajax({
                    url: '/TrainingProgram/GetStatuses',
                    method: 'GET',
                    dataType: 'json',
                    success: function (statuses) {
                        const html = generateFormHtml(response.data, statuses);
                        document.getElementById('modalBody').innerHTML = html;
                        const modal = new bootstrap.Modal(document.getElementById('programModal'));
                        modal.show();
                    }
                });
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError('Error loading program data');
        }
    });
}

function openDetailModal(programId) {
    $.ajax({
        url: '/TrainingProgram/GetById',
        method: 'GET',
        data: { id: programId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                const p = response.data;
                const html = `
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Code:</strong> ${escapeHtml(p.programCode)}</p>
                            <p><strong>Name:</strong> ${escapeHtml(p.programName)}</p>
                            <p><strong>Type:</strong> ${getEducationTypeName(p.educationType)}</p>
                            <p><strong>Duration:</strong> ${escapeHtml(p.durationText || '-')}</p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Tuition Fee:</strong> ${formatCurrency(p.tuitionFee)}</p>
                            <p><strong>Quota:</strong> ${p.quota}</p>
                            <p><strong>Status:</strong> ${getStatusBadge(p.status)}</p>
                            <p><strong>Managing Unit:</strong> ${escapeHtml(p.managingUnit || '-')}</p>
                        </div>
                    </div>
                    <div class="row mt-2">
                        <div class="col-12">
                            <p><strong>Description:</strong></p>
                            <p class="text-muted">${escapeHtml(p.description || 'No description')}</p>
                        </div>
                    </div>
                    <div class="row mt-2">
                        <div class="col-12">
                            <p><strong>Display Order:</strong> ${p.displayOrder}</p>
                        </div>
                    </div>
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

function generateFormHtml(program, statuses) {
    const isEdit = program !== null;
    const p = program || {};

    let statusOptions = '';
    statuses.forEach(function (s) {
        const selected = (p.status === s.value) ? 'selected' : '';
        statusOptions += `<option value="${s.value}" ${selected}>${s.text}</option>`;
    });

    return `
        <form id="programForm">
            <input type="hidden" id="programId" value="${isEdit ? p.id : ''}">
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Program Code <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="programCode" value="${p.programCode || ''}" required maxlength="50">
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Program Name <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="programName" value="${p.programName || ''}" required maxlength="255">
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Education Type <span class="text-danger">*</span></label>
                        <select class="form-select" id="educationType" required>
                            <option value="">-- Select Type --</option>
                            <option value="CAO_DANG" ${p.educationType === 'CAO_DANG' ? 'selected' : ''}>Cao đẳng</option>
                            <option value="TRUNG_CAP" ${p.educationType === 'TRUNG_CAP' ? 'selected' : ''}>Trung cấp</option>
                            <option value="LIEN_THONG" ${p.educationType === 'LIEN_THONG' ? 'selected' : ''}>Liên thông</option>
                            <option value="VAN_BANG_2" ${p.educationType === 'VAN_BANG_2' ? 'selected' : ''}>Văn bằng 2</option>
                            <option value="TU_XA" ${p.educationType === 'TU_XA' ? 'selected' : ''}>Từ xa</option>
                        </select>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Duration</label>
                        <input type="text" class="form-control" id="durationText" value="${p.durationText || ''}" placeholder="e.g., 2.5 years">
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Tuition Fee (VND)</label>
                        <input type="number" class="form-control" id="tuitionFee" value="${p.tuitionFee || ''}" min="0" step="1000">
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Quota <span class="text-danger">*</span></label>
                        <input type="number" class="form-control" id="quota" value="${p.quota || 100}" required min="1">
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Managing Unit</label>
                        <input type="text" class="form-control" id="managingUnit" value="${p.managingUnit || ''}">
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Status</label>
                        <select class="form-select" id="status">
                            ${statusOptions}
                        </select>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Display Order</label>
                        <input type="number" class="form-control" id="displayOrder" value="${p.displayOrder || 0}" min="0">
                    </div>
                </div>
            </div>
            <div class="mb-3">
                <label class="form-label">Description</label>
                <textarea class="form-control" id="description" rows="3">${p.description || ''}</textarea>
            </div>
        </form>
    `;
}

function saveProgram() {
    const form = document.getElementById('programForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const data = {
        programCode: document.getElementById('programCode').value,
        programName: document.getElementById('programName').value,
        educationType: document.getElementById('educationType').value,
        durationText: document.getElementById('durationText').value,
        tuitionFee: parseFloat(document.getElementById('tuitionFee').value) || null,
        quota: parseInt(document.getElementById('quota').value),
        managingUnit: document.getElementById('managingUnit').value,
        status: document.getElementById('status').value,
        displayOrder: parseInt(document.getElementById('displayOrder').value) || 0,
        description: document.getElementById('description').value
    };

    if (modalMode === 'create') {
        $.ajax({
            url: '/TrainingProgram/Create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('programModal')).hide();
                    showSuccess('Training program created successfully!');
                    loadPrograms(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error creating program: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    } else {
        data.id = document.getElementById('programId').value;
        $.ajax({
            url: '/TrainingProgram/Edit',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('programModal')).hide();
                    showSuccess('Training program updated successfully!');
                    loadPrograms(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error updating program: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    }
}

function openDeleteModal(programId, programName) {
    document.getElementById('deleteProgramId').value = programId;
    document.getElementById('deleteProgramName').textContent = programName;
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    modal.show();
}

function confirmDelete() {
    const programId = document.getElementById('deleteProgramId').value;

    $.ajax({
        url: '/TrainingProgram/Delete',
        method: 'POST',
        data: { id: programId },
        success: function (response) {
            if (response.success) {
                bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
                showSuccess('Training program deleted successfully!');
                loadPrograms(currentPage);
            } else {
                showError(response.message);
            }
        },
        error: function (xhr) {
            showError('Error deleting program: ' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

// =============================================================================
// RENDER FUNCTIONS
// =============================================================================
function renderPrograms(data) {
    const tbody = document.getElementById('programTableBody');

    if (!data.items || data.items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="9" class="text-center text-muted py-4">
                    <i class="bi bi-inbox" style="font-size: 2rem;"></i>
                    <p class="mt-2">No training programs found</p>
                </td>
            </tr>
        `;
        return;
    }

    let html = '';
    data.items.forEach(function (program) {
        const statusBadge = getStatusBadge(program.status);
        const typeBadge = getTypeBadge(program.educationType);

        html += `
            <tr>
                <td><strong>${escapeHtml(program.programCode)}</strong></td>
                <td>${escapeHtml(program.programName)}</td>
                <td>${typeBadge}</td>
                <td>${escapeHtml(program.durationText || '-')}</td>
                <td>${formatCurrency(program.tuitionFee)}</td>
                <td class="text-center">${program.quota}</td>
                <td class="text-center">
                    <span class="badge bg-info">${program.majorCount}</span>
                </td>
                <td>${statusBadge}</td>
                <td>
                    <button class="btn btn-sm btn-info me-1" onclick="openDetailModal('${program.id}')" title="View Details">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-primary me-1" onclick="openEditModal('${program.id}')" title="Edit">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${program.id}', '${escapeHtml(program.programName)}')" title="Delete">
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
        'ACTIVE': '<span class="badge bg-success">Active</span>',
        'INACTIVE': '<span class="badge bg-secondary">Inactive</span>',
        'DRAFT': '<span class="badge bg-warning text-dark">Draft</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

function getTypeBadge(type) {
    const badges = {
        'CAO_DANG': '<span class="badge bg-primary">Cao đẳng</span>',
        'TRUNG_CAP': '<span class="badge bg-info">Trung cấp</span>',
        'LIEN_THONG': '<span class="badge bg-success">Liên thông</span>',
        'VAN_BANG_2': '<span class="badge bg-warning text-dark">Văn bằng 2</span>',
        'TU_XA': '<span class="badge bg-secondary">Từ xa</span>'
    };
    return badges[type] || `<span class="badge bg-secondary">${type}</span>`;
}

function getEducationTypeName(type) {
    const names = {
        'CAO_DANG': 'Cao đẳng',
        'TRUNG_CAP': 'Trung cấp',
        'LIEN_THONG': 'Liên thông',
        'VAN_BANG_2': 'Văn bằng 2',
        'TU_XA': 'Từ xa'
    };
    return names[type] || type;
}

function formatCurrency(value) {
    if (!value) return '-';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showLoading() {
    const tbody = document.getElementById('programTableBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="9" class="text-center text-muted py-4">
                <div class="spinner-border text-success" role="status"></div>
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
