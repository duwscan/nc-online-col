let currentPage = 1;
let currentPageSize = 10;

document.addEventListener('DOMContentLoaded', function () {
    loadFilters();
    loadApplications(currentPage);
    setupEventListeners();
});

function setupEventListeners() {
    document.getElementById('btnRefresh').addEventListener('click', refreshWithXHR);
    document.getElementById('btnUpdateStatus').addEventListener('click', updateStatus);
    document.getElementById('newStatus').addEventListener('change', toggleReasonField);
}

function handleSearchKeypress(event) {
    if (event.key === 'Enter') {
        searchApplications();
    }
}

function changePageSize() {
    currentPageSize = parseInt(document.getElementById('pageSize').value);
    currentPage = 1;
    loadApplications(currentPage);
}

function searchApplications() {
    currentPage = 1;
    loadApplications(currentPage);
}

function goToPage(page) {
    currentPage = page;
    loadApplications(page);
}

// =============================================================================
// METHOD 1: XmlHttpRequest - Dùng cho nút Refresh
// =============================================================================
function refreshWithXHR() {
    showLoading();

    const xhr = new XMLHttpRequest();
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const roundProgramId = document.getElementById('filterRoundProgram').value;

    const url = `/AdmissionApplication/GetApplications?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&roundProgramId=${roundProgramId}&page=${currentPage}&pageSize=${currentPageSize}`;

    xhr.open('GET', url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                const data = JSON.parse(xhr.responseText);
                renderApplications(data);
                renderPagination(data);
                updateShowingInfo(data);
                showToast('Data refreshed using XmlHttpRequest!', 'success');
            } else {
                showError('Error loading applications: ' + xhr.statusText);
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
async function loadApplications(page) {
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const roundProgramId = document.getElementById('filterRoundProgram').value;

    const url = `/AdmissionApplication/GetApplications?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&roundProgramId=${roundProgramId}&page=${page}&pageSize=${currentPageSize}`;

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
        renderApplications(data);
        renderPagination(data);
        updateShowingInfo(data);
    } catch (error) {
        console.error('Error:', error);
        showError('Failed to load applications: ' + error.message);
    }
}

// =============================================================================
// METHOD 3: jQuery AJAX - Dùng cho Update Status
// =============================================================================
function loadFilters() {
    $.ajax({
        url: '/AdmissionApplication/GetRoundPrograms',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            const select = document.getElementById('filterRoundProgram');
            data.forEach(function (item) {
                const option = document.createElement('option');
                option.value = item.id;
                option.textContent = `${item.roundName} - ${item.programName} (${item.year})`;
                select.appendChild(option);
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading round programs:', error);
        }
    });
}

function openDetailModal(applicationId) {
    $.ajax({
        url: '/AdmissionApplication/GetById',
        method: 'GET',
        data: { id: applicationId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                const app = response.data;
                let historyHtml = '';
                if (response.histories && response.histories.length > 0) {
                    historyHtml = `
                        <hr>
                        <h6>Status History</h6>
                        <div class="list-group">
                            ${response.histories.map(h => `
                                <div class="list-group-item">
                                    <div class="d-flex justify-content-between">
                                        <span>${getStatusBadge(h.newStatus)}</span>
                                        <small class="text-muted">${formatDateTime(h.changedAt)}</small>
                                    </div>
                                    <small class="text-muted">By: ${h.changedByUserName || 'System'}</small>
                                    ${h.reason ? `<p class="mb-0 mt-1"><small>${escapeHtml(h.reason)}</small></p>` : ''}
                                </div>
                            `).join('')}
                        </div>
                    `;
                }

                const html = `
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Application Code:</strong> ${escapeHtml(app.applicationCode)}</p>
                            <p><strong>Candidate:</strong> ${escapeHtml(app.candidateName)}</p>
                            <p><strong>Email:</strong> ${escapeHtml(app.candidateEmail)}</p>
                            <p><strong>Phone:</strong> ${escapeHtml(app.candidatePhone)}</p>
                            ${app.nationalId ? `<p><strong>CCCD:</strong> ${escapeHtml(app.nationalId)}</p>` : ''}
                        </div>
                        <div class="col-md-6">
                            <p><strong>Program:</strong> ${escapeHtml(app.programName)}</p>
                            <p><strong>Round:</strong> ${escapeHtml(app.roundName)} (${app.admissionYear})</p>
                            <p><strong>Status:</strong> ${getStatusBadge(app.currentStatus)}</p>
                            <p><strong>Submission #:</strong> ${app.submissionNumber}</p>
                            <p><strong>Submitted:</strong> ${app.submittedAt ? formatDateTime(app.submittedAt) : 'Not submitted'}</p>
                        </div>
                    </div>
                    <div class="row mt-2">
                        <div class="col-md-4">
                            <p><strong>Documents:</strong> <span class="badge bg-info">${app.documentCount}</span></p>
                        </div>
                        <div class="col-md-4">
                            <p><strong>Preferences:</strong> <span class="badge bg-secondary">${app.preferenceCount}</span></p>
                        </div>
                        <div class="col-md-4">
                            <p><strong>Last Update:</strong> ${formatDateTime(app.updatedAt)}</p>
                        </div>
                    </div>
                    ${app.rejectionReason ? `
                    <div class="alert alert-danger mt-2">
                        <strong>Rejection Reason:</strong><br>
                        ${escapeHtml(app.rejectionReason)}
                    </div>
                    ` : ''}
                    ${historyHtml}
                `;
                document.getElementById('detailModalBody').innerHTML = html;
                const modal = new bootstrap.Modal(document.getElementById('detailModal'));
                modal.show();
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError('Error loading application details');
        }
    });
}

function openStatusModal(applicationId) {
    document.getElementById('updateStatusAppId').value = applicationId;
    document.getElementById('statusReason').value = '';
    document.getElementById('reasonGroup').style.display = 'none';

    $.ajax({
        url: '/AdmissionApplication/GetStatuses',
        method: 'GET',
        dataType: 'json',
        success: function (statuses) {
            const select = document.getElementById('newStatus');
            select.innerHTML = '';
            statuses.forEach(function (status) {
                const option = document.createElement('option');
                option.value = status.value;
                option.textContent = status.text;
                select.appendChild(option);
            });
            const modal = new bootstrap.Modal(document.getElementById('statusModal'));
            modal.show();
        },
        error: function () {
            showError('Error loading statuses');
        }
    });
}

function toggleReasonField() {
    const status = document.getElementById('newStatus').value;
    const reasonGroup = document.getElementById('reasonGroup');
    reasonGroup.style.display = (status === 'REJECTED') ? 'block' : 'none';
}

function updateStatus() {
    const applicationId = document.getElementById('updateStatusAppId').value;
    const newStatus = document.getElementById('newStatus').value;
    const reason = document.getElementById('statusReason').value;

    if (newStatus === 'REJECTED' && !reason.trim()) {
        showError('Please provide a reason for rejection');
        return;
    }

    const data = {
        applicationId: applicationId,
        status: newStatus,
        reason: reason || null
    };

    $.ajax({
        url: '/AdmissionApplication/UpdateStatus',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                bootstrap.Modal.getInstance(document.getElementById('statusModal')).hide();
                showSuccess('Status updated successfully!');
                loadApplications(currentPage);
            } else {
                showError(response.message);
            }
        },
        error: function (xhr) {
            showError('Error updating status: ' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

function openHistoryModal(applicationId) {
    $.ajax({
        url: '/AdmissionApplication/GetById',
        method: 'GET',
        data: { id: applicationId },
        dataType: 'json',
        success: function (response) {
            if (response.success && response.histories && response.histories.length > 0) {
                let html = `
                    <div class="list-group">
                        ${response.histories.map(h => `
                            <div class="list-group-item">
                                <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                        <span class="badge bg-secondary me-2">${h.oldStatus}</span>
                                        <i class="bi bi-arrow-right mx-1"></i>
                                        <span class="badge bg-primary">${h.newStatus}</span>
                                    </div>
                                    <small class="text-muted">${formatDateTime(h.changedAt)}</small>
                                </div>
                                <small class="text-muted">Changed by: ${h.changedByUserName || 'System'}</small>
                                ${h.reason ? `<p class="mb-0 mt-2"><strong>Reason:</strong> ${escapeHtml(h.reason)}</p>` : ''}
                            </div>
                        `).join('')}
                    </div>
                `;
                document.getElementById('historyModalBody').innerHTML = html;
            } else {
                document.getElementById('historyModalBody').innerHTML = '<p class="text-muted">No status history found</p>';
            }
            const modal = new bootstrap.Modal(document.getElementById('historyModal'));
            modal.show();
        },
        error: function () {
            showError('Error loading history');
        }
    });
}

// =============================================================================
// RENDER FUNCTIONS
// =============================================================================
function renderApplications(data) {
    const tbody = document.getElementById('applicationTableBody');

    if (!data.items || data.items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="bi bi-inbox" style="font-size: 2rem;"></i>
                    <p class="mt-2">No applications found</p>
                </td>
            </tr>
        `;
        return;
    }

    let html = '';
    data.items.forEach(function (app) {
        html += `
            <tr>
                <td><strong>${escapeHtml(app.applicationCode)}</strong></td>
                <td>
                    <div>${escapeHtml(app.candidateName)}</div>
                    <small class="text-muted">${escapeHtml(app.candidateEmail)}</small>
                </td>
                <td>${escapeHtml(app.programName)}</td>
                <td>
                    <div>${escapeHtml(app.roundName)}</div>
                    <small class="text-muted">${app.admissionYear}</small>
                </td>
                <td>${getStatusBadge(app.currentStatus)}</td>
                <td class="text-center">
                    <span class="badge bg-info">${app.documentCount}</span>
                </td>
                <td>
                    ${app.submittedAt ? formatDate(app.submittedAt) : '<span class="text-muted">-</span>'}
                </td>
                <td>
                    <button class="btn btn-sm btn-purple me-1" onclick="openDetailModal('${app.id}')" title="View Details" style="background-color: #6f42c1; color: white;">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning me-1" onclick="openStatusModal('${app.id}')" title="Update Status">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-info" onclick="openHistoryModal('${app.id}')" title="History">
                        <i class="bi bi-clock-history"></i>
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
        'SUBMITTED': '<span class="badge bg-info">Đã nộp</span>',
        'UNDER_REVIEW': '<span class="badge bg-primary">Đang xét duyệt</span>',
        'REQUIRED_SUPPLEMENT': '<span class="badge bg-warning text-dark">Yêu cầu bổ sung</span>',
        'RESUBMITTED': '<span class="badge bg-info">Đã bổ sung</span>',
        'APPROVED': '<span class="badge bg-success">Đã duyệt</span>',
        'REJECTED': '<span class="badge bg-danger">Bị từ chối</span>',
        'CANCELLED': '<span class="badge bg-dark">Đã hủy</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN');
}

function formatDateTime(dateStr) {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showLoading() {
    const tbody = document.getElementById('applicationTableBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center text-muted py-4">
                <div class="spinner-border" style="color: #6f42c1;" role="status"></div>
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
