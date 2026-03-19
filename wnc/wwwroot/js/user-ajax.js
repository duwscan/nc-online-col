let currentPage = 1;
let currentPageSize = 10;
let modalMode = 'create';
let editingUserId = null;

document.addEventListener('DOMContentLoaded', function () {
    loadRoles();
    loadUsers(currentPage);
});

function handleSearchKeyup(event) {
    if (event.key === 'Enter') {
        searchUsers();
    }
}

function changePageSize() {
    currentPageSize = parseInt(document.getElementById('pageSize').value);
    currentPage = 1;
    loadUsers(currentPage);
}

function searchUsers() {
    currentPage = 1;
    loadUsers(currentPage);
}

function goToPage(page) {
    currentPage = page;
    loadUsers(page);
}

// =============================================================================
// METHOD 1: XmlHttpRequest - Used for Refresh button
// =============================================================================
function refreshWithXmlHttpRequest() {
    const xhr = new XMLHttpRequest();
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const role = document.getElementById('filterRole').value;

    const url = `/User/GetUsers?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&role=${role}&page=${currentPage}&pageSize=${currentPageSize}`;

    xhr.open('GET', url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                const data = JSON.parse(xhr.responseText);
                renderUsers(data);
                renderPagination(data);
                updateShowingInfo(data);
            } else {
                showError('Error loading users: ' + xhr.statusText);
            }
        }
    };

    xhr.onerror = function () {
        showError('Network error occurred');
    };

    xhr.send();
}

// =============================================================================
// METHOD 2: Fetch API - Used for Search button
// =============================================================================
async function loadUsers(page) {
    const searchTerm = document.getElementById('searchTerm').value;
    const status = document.getElementById('filterStatus').value;
    const role = document.getElementById('filterRole').value;

    const url = `/User/GetUsers?searchTerm=${encodeURIComponent(searchTerm)}&status=${status}&role=${role}&page=${page}&pageSize=${currentPageSize}`;

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
        renderUsers(data);
        renderPagination(data);
        updateShowingInfo(data);
    } catch (error) {
        console.error('Error:', error);
        showError('Failed to load users: ' + error.message);
    }
}

// =============================================================================
// METHOD 3: jQuery AJAX - Used for Create, Edit, Delete
// =============================================================================
function loadRoles() {
    $.ajax({
        url: '/User/GetRoles',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            const roleSelect = document.getElementById('filterRole');
            data.forEach(function (role) {
                const option = document.createElement('option');
                option.value = role.code;
                option.textContent = role.name;
                roleSelect.appendChild(option);
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading roles:', error);
        }
    });
}

function openCreateModal() {
    modalMode = 'create';
    editingUserId = null;
    document.getElementById('modalTitle').textContent = 'Create User';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Save (jQuery AJAX)';

    $.ajax({
        url: '/User/GetRoles',
        method: 'GET',
        dataType: 'json',
        success: function (roles) {
            const html = generateUserFormHtml(roles, null);
            document.getElementById('modalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('userModal'));
            modal.show();
        },
        error: function () {
            document.getElementById('modalBody').innerHTML = '<div class="alert alert-danger">Error loading form</div>';
        }
    });
}

function openEditModal(userId) {
    modalMode = 'edit';
    editingUserId = userId;
    document.getElementById('modalTitle').textContent = 'Edit User';
    document.getElementById('btnSave').innerHTML = '<i class="bi bi-save"></i> Update (jQuery AJAX)';

    $.ajax({
        url: '/User/GetById',
        method: 'GET',
        data: { id: userId },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                $.ajax({
                    url: '/User/GetRoles',
                    method: 'GET',
                    dataType: 'json',
                    success: function (roles) {
                        const html = generateUserFormHtml(roles, response.data);
                        document.getElementById('modalBody').innerHTML = html;
                        const modal = new bootstrap.Modal(document.getElementById('userModal'));
                        modal.show();
                    }
                });
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError('Error loading user data');
        }
    });
}

function generateUserFormHtml(roles, user) {
    const isEdit = user !== null;
    const username = user ? user.username : '';
    const email = user ? user.email : '';
    const phone = user ? user.phoneNumber : '';
    const status = user ? user.status : 'ACTIVE';
    const selectedRoles = user ? user.selectedRoles : [];

    let rolesHtml = '';
    roles.forEach(function (role) {
        const checked = selectedRoles.includes(role.code) ? 'checked' : '';
        rolesHtml += `
            <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" name="roles" value="${role.code}" id="role_${role.code}" ${checked}>
                <label class="form-check-label" for="role_${role.code}">${role.name}</label>
            </div>
        `;
    });

    return `
        <form id="userForm">
            <input type="hidden" id="userId" value="${isEdit ? user.id : ''}">
            <div class="mb-3">
                <label class="form-label">Username <span class="text-danger">*</span></label>
                <input type="text" class="form-control" id="username" value="${username}" required>
            </div>
            <div class="mb-3">
                <label class="form-label">Email <span class="text-danger">*</span></label>
                <input type="email" class="form-control" id="email" value="${email}" required>
            </div>
            <div class="mb-3">
                <label class="form-label">Phone Number</label>
                <input type="text" class="form-control" id="phoneNumber" value="${phone}">
            </div>
            ${!isEdit ? `
            <div class="mb-3">
                <label class="form-label">Password <span class="text-danger">*</span></label>
                <input type="password" class="form-control" id="password" required minlength="6">
            </div>
            ` : ''}
            <div class="mb-3">
                <label class="form-label">Status</label>
                <select class="form-select" id="status">
                    <option value="ACTIVE" ${status === 'ACTIVE' ? 'selected' : ''}>Active</option>
                    <option value="INACTIVE" ${status === 'INACTIVE' ? 'selected' : ''}>Inactive</option>
                    <option value="SUSPENDED" ${status === 'SUSPENDED' ? 'selected' : ''}>Suspended</option>
                </select>
            </div>
            <div class="mb-3">
                <label class="form-label">Roles</label>
                <div class="border rounded p-2">
                    ${rolesHtml}
                </div>
            </div>
        </form>
    `;
}

// Save User - jQuery AJAX
document.getElementById('btnSave').addEventListener('click', function () {
    const form = document.getElementById('userForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const selectedRoles = [];
    document.querySelectorAll('input[name="roles"]:checked').forEach(function (cb) {
        selectedRoles.push(cb.value);
    });

    if (modalMode === 'create') {
        const data = {
            username: document.getElementById('username').value,
            email: document.getElementById('email').value,
            phoneNumber: document.getElementById('phoneNumber').value,
            password: document.getElementById('password').value,
            status: document.getElementById('status').value,
            selectedRoles: selectedRoles
        };

        $.ajax({
            url: '/User/Create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
                    showSuccess('User created successfully!');
                    loadUsers(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error creating user: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    } else {
        const data = {
            id: document.getElementById('userId').value,
            username: document.getElementById('username').value,
            email: document.getElementById('email').value,
            phoneNumber: document.getElementById('phoneNumber').value,
            status: document.getElementById('status').value,
            selectedRoles: selectedRoles
        };

        $.ajax({
            url: '/User/Edit',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
                    showSuccess('User updated successfully!');
                    loadUsers(currentPage);
                } else {
                    showError(response.message);
                }
            },
            error: function (xhr) {
                showError('Error updating user: ' + (xhr.responseJSON?.message || xhr.statusText));
            }
        });
    }
});

// Delete User - jQuery AJAX
function openDeleteModal(userId) {
    document.getElementById('deleteUserId').value = userId;
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    modal.show();
}

function confirmDelete() {
    const userId = document.getElementById('deleteUserId').value;

    $.ajax({
        url: '/User/Delete',
        method: 'POST',
        data: { id: userId },
        success: function (response) {
            if (response.success) {
                bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
                showSuccess('User deleted successfully!');
                loadUsers(currentPage);
            } else {
                showError(response.message);
            }
        },
        error: function (xhr) {
            showError('Error deleting user: ' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

// =============================================================================
// RENDER FUNCTIONS
// =============================================================================
function renderUsers(data) {
    const tbody = document.getElementById('userTableBody');

    if (!data.items || data.items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    <i class="bi bi-inbox" style="font-size: 2rem;"></i>
                    <p class="mt-2">No users found</p>
                </td>
            </tr>
        `;
        return;
    }

    let html = '';
    data.items.forEach(function (user) {
        const statusBadge = getStatusBadge(user.status);
        const rolesHtml = user.roles && user.roles.length > 0
            ? user.roles.map(r => `<span class="badge bg-secondary me-1">${r}</span>`).join('')
            : '<span class="text-muted">-</span>';

        html += `
            <tr>
                <td><strong>${escapeHtml(user.username || '-')}</strong></td>
                <td>${escapeHtml(user.email || '-')}</td>
                <td>${escapeHtml(user.phoneNumber || '-')}</td>
                <td>${statusBadge}</td>
                <td>${rolesHtml}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="openEditModal('${user.id}')" title="Edit">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${user.id}')" title="Delete">
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

    const disabledPrev = data.pageNumber <= 1 ? 'disabled' : '';
    html += `
        <li class="page-item ${disabledPrev}">
            <a class="page-link" href="#" onclick="goToPage(${data.pageNumber - 1}); return false;">Previous</a>
        </li>
    `;

    for (let i = 1; i <= data.totalPages; i++) {
        const active = i === data.pageNumber ? 'active' : '';
        html += `
            <li class="page-item ${active}">
                <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
            </li>
        `;
    }

    const disabledNext = data.pageNumber >= data.totalPages ? 'disabled' : '';
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
        'SUSPENDED': '<span class="badge bg-danger">Suspended</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showSuccess(message) {
    const alert = document.createElement('div');
    alert.className = 'alert alert-success alert-dismissible fade show position-fixed top-0 end-0 m-3';
    alert.style.zIndex = '9999';
    alert.innerHTML = `
        <i class="bi bi-check-circle"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 3000);
}

function showError(message) {
    const alert = document.createElement('div');
    alert.className = 'alert alert-danger alert-dismissible fade show position-fixed top-0 end-0 m-3';
    alert.style.zIndex = '9999';
    alert.innerHTML = `
        <i class="bi bi-exclamation-circle"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 5000);
}
