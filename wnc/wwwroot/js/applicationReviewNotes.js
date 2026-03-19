(function () {
    let currentPage = 1;
    const pageSize = 10;

    function getAntiForgeryToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function getQueryParams() {
        const search = document.getElementById('search').value.trim();
        const noteType = document.getElementById('noteType').value.trim();
        const visibleToCandidate = document.getElementById('visibleToCandidate').value;
        const sortBy = document.getElementById('sortBy').value;
        const descending = document.getElementById('descending').checked;
        const pageSizeValue = document.getElementById('pageSize').value;

        const params = new URLSearchParams();
        if (search) params.append('Search', search);
        if (noteType) params.append('NoteType', noteType);
        if (visibleToCandidate !== '') params.append('VisibleToCandidate', visibleToCandidate);
        params.append('SortBy', sortBy);
        params.append('Descending', descending);
        params.append('Page', currentPage);
        params.append('PageSize', pageSizeValue);

        return params.toString();
    }

    function renderTable(items) {
        const tbody = document.getElementById('notesTbody');
        if (!items || items.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4">No records found.</td></tr>';
            return;
        }

        const html = items.map(item => {
            const visibleText = item.isVisibleToCandidate ? 'Yes' : 'No';
            const date = new Date(item.createdAt);
            const dateStr = date.toISOString().slice(0, 16).replace('T', ' ');
            const contentPreview = item.contentPreview || '';

            return `<tr data-id="${item.id}">
                <td>${escapeHtml(item.applicationCode)}</td>
                <td>${escapeHtml(item.noteType)}</td>
                <td>${escapeHtml(item.authorDisplay)}</td>
                <td>${visibleText}</td>
                <td>${dateStr}</td>
                <td>${escapeHtml(contentPreview)}</td>
                <td class="text-end">
                    <a href="/ApplicationReviewNotes/Details/${item.id}" class="btn btn-sm btn-outline-secondary">Details</a>
                    <a href="/ApplicationReviewNotes/Edit/${item.id}" class="btn btn-sm btn-outline-primary">Edit</a>
                    <button type="button" class="btn btn-sm btn-outline-danger delete-btn" data-id="${item.id}">Delete</button>
                </td>
            </tr>`;
        }).join('');

        tbody.innerHTML = html;
        attachDeleteHandlers();
    }

    function renderPager(results) {
        const pager = document.getElementById('pager');
        const page = results.page;
        const totalPages = results.totalPages;
        const totalCount = results.totalCount;

        const prevDisabled = page <= 1 ? 'disabled' : '';
        const nextDisabled = page >= totalPages ? 'disabled' : '';

        pager.innerHTML = `
            <span class="text-muted">Page ${page} of ${totalPages}, total ${totalCount} records</span>
            <div class="btn-group">
                <button type="button" class="btn btn-outline-secondary ${prevDisabled}" id="prevPage">Previous</button>
                <button type="button" class="btn btn-outline-secondary ${nextDisabled}" id="nextPage">Next</button>
            </div>
        `;

        document.getElementById('prevPage').addEventListener('click', () => {
            if (currentPage > 1) {
                currentPage--;
                loadPageXHR();
            }
        });

        document.getElementById('nextPage').addEventListener('click', () => {
            if (currentPage < totalPages) {
                currentPage++;
                loadPageXHR();
            }
        });
    }

    function loadPageXHR() {
        const xhr = new XMLHttpRequest();
        const url = '/ApplicationReviewNotes/ListJson?' + getQueryParams();

        xhr.open('GET', url, true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try {
                        const results = JSON.parse(xhr.responseText);
                        renderTable(results.items);
                        renderPager(results);
                    } catch (e) {
                        console.error('Failed to parse JSON:', e);
                    }
                } else {
                    console.error('XHR failed:', xhr.status, xhr.statusText);
                }
            }
        };
        xhr.send();
    }

    function searchFetch() {
        const url = '/ApplicationReviewNotes/SearchJson?' + getQueryParams();

        fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(results => {
                currentPage = results.page || 1;
                renderTable(results.items);
                renderPager(results);
            })
            .catch(error => {
                console.error('Fetch error:', error);
            });
    }

    function createNoteJQuery() {
        const form = document.getElementById('createForm');
        const formData = new FormData(form);
        const data = {};
        formData.forEach((value, key) => {
            if (key === 'IsVisibleToCandidate') {
                data[key] = true;
            } else {
                data[key] = value;
            }
        });

        if (!form.querySelector('#createIsVisibleToCandidate').checked) {
            data.IsVisibleToCandidate = false;
        }

        clearCreateErrors();

        $.ajax({
            url: '/ApplicationReviewNotes/CreateAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
            success: function (result) {
                if (result.success) {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('createModal'));
                    modal.hide();
                    form.reset();
                    document.getElementById('createNoteType').value = 'GENERAL';
                    setDefaultCreatedAt();
                    currentPage = 1;
                    loadPageXHR();
                } else {
                    showCreateErrors(result.errors || {}, result.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Create error:', error);
                showCreateErrors({}, 'An error occurred while creating the note.');
            }
        });
    }

    function deleteNoteJQuery(id) {
        if (!confirm('Delete this record?')) return;

        $.ajax({
            url: '/ApplicationReviewNotes/DeleteAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
            success: function (result) {
                if (result.success) {
                    const row = document.querySelector(`tr[data-id="${id}"]`);
                    if (row) row.remove();
                    const tbody = document.getElementById('notesTbody');
                    if (tbody.children.length === 0) {
                        loadPageXHR();
                    }
                } else {
                    alert(result.message || 'Failed to delete record.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Delete error:', error);
                alert('An error occurred while deleting the record.');
            }
        });
    }

    function attachDeleteHandlers() {
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const id = this.getAttribute('data-id');
                deleteNoteJQuery(id);
            });
        });
    }

    function clearCreateErrors() {
        document.getElementById('createErrorSummary').classList.add('d-none');
        document.getElementById('createErrorSummary').textContent = '';
        document.querySelectorAll('[id$="Error"]').forEach(el => el.textContent = '');
    }

    function showCreateErrors(errors, generalMessage) {
        if (generalMessage) {
            const summary = document.getElementById('createErrorSummary');
            summary.textContent = generalMessage;
            summary.classList.remove('d-none');
        }

        for (const [field, messages] of Object.entries(errors || {})) {
            const errorEl = document.getElementById(`create${field}Error`);
            if (errorEl && messages.length > 0) {
                errorEl.textContent = messages.join(' ');
            }
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function setDefaultCreatedAt() {
        const now = new Date();
        const offset = now.getTimezoneOffset();
        const local = new Date(now.getTime() - offset * 60 * 1000);
        document.getElementById('createCreatedAt').value = local.toISOString().slice(0, 16);
    }

    function loadLookupOptions() {
        fetch('/ApplicationReviewNotes/GetCreateLookups')
            .then(response => response.json())
            .then(data => {
                const appSelect = document.getElementById('createApplicationId');
                const userSelect = document.getElementById('createAuthorUserId');

                appSelect.innerHTML = '<option value="">Select application...</option>';
                userSelect.innerHTML = '<option value="">Select author...</option>';

                if (data.applications) {
                    data.applications.forEach(app => {
                        const opt = document.createElement('option');
                        opt.value = app.id;
                        opt.textContent = app.code;
                        appSelect.appendChild(opt);
                    });
                }

                if (data.users) {
                    data.users.forEach(user => {
                        const opt = document.createElement('option');
                        opt.value = user.id;
                        opt.textContent = user.display;
                        userSelect.appendChild(opt);
                    });
                }
            })
            .catch(err => console.error('Failed to load lookups:', err));
    }

    document.addEventListener('DOMContentLoaded', function () {
        loadPageXHR();
        setDefaultCreatedAt();

        document.getElementById('searchBtn').addEventListener('click', function () {
            currentPage = 1;
            searchFetch();
        });

        document.getElementById('resetBtn').addEventListener('click', function () {
            document.getElementById('search').value = '';
            document.getElementById('noteType').value = '';
            document.getElementById('visibleToCandidate').value = '';
            document.getElementById('sortBy').value = 'createdat';
            document.getElementById('descending').checked = false;
            document.getElementById('pageSize').value = '10';
            currentPage = 1;
            loadPageXHR();
        });

        document.getElementById('createSaveBtn').addEventListener('click', function () {
            createNoteJQuery();
        });

        document.getElementById('createModal').addEventListener('show.bs.modal', function () {
            clearCreateErrors();
            setDefaultCreatedAt();
            loadLookupOptions();
        });

        document.getElementById('search').addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                currentPage = 1;
                searchFetch();
            }
        });
    });
})();
