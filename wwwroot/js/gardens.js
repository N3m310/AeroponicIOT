const API_BASE = window.API_BASE || (window.location.origin + '/api');

let gardens = [];
let crops = [];
let userRole = 'Farmer';
let isAdmin = false;

let farmers = [];

document.addEventListener('DOMContentLoaded', () => {
    if (!checkAuthentication()) {
        return;
    }
    setupEventListeners();
    resetGardenForm();
    loadFarmers();
    loadCrops().then(() => {
        loadGardens();
    });
});

async function loadFarmers() {
    if (!isAdmin) {
        const list = document.getElementById('gardenOwnersList');
        if (list) {
            const fg = list.closest('.form-group');
            if (fg) fg.style.display = 'none';
        }
        return;
    }
    try {
        const response = await fetch(`${API_BASE}/users`, { headers: getAuthHeaders() });
        if (response.status === 401) {
            logout();
            return;
        }
        if (!response.ok) throw new Error('Không thể tải danh sách tài khoản');

        const json = await response.json();
        const allUsers = Auth.unwrapApiData(json) || [];
        farmers = allUsers.filter(u => u.role === 'Farmer');
        renderFarmersList();
    } catch (error) {
        console.error('Error loading farmers:', error);
        const list = document.getElementById('gardenOwnersList');
        if (list) {
            list.innerHTML = '<p style="color: red; margin: 0; font-size: 0.9rem;">Lỗi tải danh sách Farmer</p>';
        }
    }
}

function renderFarmersList(checkedIds = []) {
    const list = document.getElementById('gardenOwnersList');
    if (!list) return;
    if (!farmers.length) {
        list.innerHTML = '<p style="color: #666; margin: 0; font-size: 0.9rem;">Không có tài khoản Farmer nào</p>';
        return;
    }
    list.innerHTML = farmers.map(farmer => {
        const isChecked = checkedIds.includes(farmer.id);
        return `
            <label style="display: flex; align-items: center; gap: 0.5rem; cursor: pointer; font-weight: normal; margin: 0;">
                <input type="checkbox" name="gardenOwner" value="${farmer.id}" ${isChecked ? 'checked' : ''} style="cursor: pointer; margin: 0;">
                <span>${escapeHtml(farmer.username)} (${escapeHtml(farmer.email || 'Không có email')})</span>
            </label>
        `;
    }).join('');
}

async function loadCrops() {
    try {
        const response = await fetch(`${API_BASE}/crop`, { headers: getAuthHeaders() });
        if (response.status === 401) {
            logout();
            return;
        }
        if (!response.ok) throw new Error('Không thể tải danh sách cây trồng');

        const json = await response.json();
        crops = Auth.unwrapApiData(json) || [];
        populateCropSelect();
    } catch (error) {
        console.error('Error loading crops:', error);
    }
}

function populateCropSelect() {
    const select = document.getElementById('gardenCropSelect');
    if (!select) return;
    
    select.innerHTML = '<option value="">-- Chưa gán cây trồng --</option>';
    crops.forEach(crop => {
        const option = document.createElement('option');
        option.value = crop.id;
        option.textContent = crop.name;
        select.appendChild(option);
    });
}

function checkAuthentication() {
    const token = Auth.getStoredToken();
    if (!token) {
        Auth.clearAuthStorage();
        window.location.href = 'login.html';
        return false;
    }

    const username = localStorage.getItem('username');
    userRole = localStorage.getItem('role') || 'Farmer';
    isAdmin = typeof userRole === 'string' && userRole.trim().toLowerCase() === 'administrator';

    const header = document.getElementById('gardensHeader');
    header.innerHTML = `
        <span>🏡 Quản lý khu vườn <small>(${username} - ${userRole})</small></span>
        <div>
            <button id="backDashboardBtn" class="btn-secondary" type="button">← Bảng điều khiển</button>
            ${isAdmin ? '<button id="goDevicesBtn" class="btn-secondary" type="button">🔧 Thiết bị</button>' : ''}
            <button id="goCropsBtn" class="btn-secondary" type="button">🌿 Cây trồng</button>
            <button id="logoutBtn" class="btn-secondary" type="button">Đăng xuất</button>
        </div>
    `;

    document.getElementById('backDashboardBtn').addEventListener('click', () => window.location.href = 'index.html');
    document.getElementById('goDevicesBtn')?.addEventListener('click', () => window.location.href = 'devices.html');
    document.getElementById('goCropsBtn').addEventListener('click', () => window.location.href = 'crops.html');
    document.getElementById('logoutBtn').addEventListener('click', logout);

    // Show/hide admin notices
    const adminNotice = document.getElementById('adminNotice');
    if (adminNotice) {
        if (!isAdmin) {
            adminNotice.style.display = 'inline-block';
            adminNotice.textContent = '🔒 Chế độ xem & Thêm mới (Chỉ quản trị viên mới được sửa/xóa)';
        } else {
            adminNotice.style.display = 'none';
        }
    }
    return true;
}

function setupEventListeners() {
    document.getElementById('gardenForm').addEventListener('submit', saveGarden);
    document.getElementById('resetFormBtn').addEventListener('click', resetGardenForm);
}

function getAuthHeaders() {
    return Auth.getAuthHeaders();
}

async function loadGardens() {
    try {
        const response = await fetch(`${API_BASE}/garden`, { headers: getAuthHeaders() });
        if (response.status === 401) {
            logout();
            return;
        }
        if (!response.ok) throw new Error('Không thể tải danh sách khu vườn');

        gardens = Auth.unwrapApiData(await response.json());
        if (!Array.isArray(gardens)) {
            gardens = [];
        }
        renderGardens();
    } catch (error) {
        console.error('Error loading gardens:', error);
        showError('Không thể tải danh sách khu vườn');
    }
}

function renderGardens() {
    const grid = document.getElementById('gardensGrid');
    if (!gardens.length) {
        grid.innerHTML = '<p class="no-devices">Chưa có khu vườn nào. Hãy tạo khu vườn đầu tiên của bạn.</p>';
        return;
    }

    grid.innerHTML = '';
    gardens.forEach(garden => {
        const card = document.createElement('div');
        card.className = 'garden-card';
        
        let actionButtons = '';
        if (isAdmin) {
            actionButtons = `
                <div class="card-actions">
                    <button class="btn-small edit" type="button" onclick="editGarden(${garden.id})">✏️ Sửa</button>
                    <button class="btn-small delete" type="button" onclick="deleteGarden(${garden.id}, '${escapeJs(garden.name)}')">🗑️ Xóa</button>
                </div>
            `;
        }

        const ownersText = garden.ownerNames && garden.ownerNames.length > 0 
            ? garden.ownerNames.join(', ') 
            : 'Chưa gán người sở hữu';

        card.innerHTML = `
            <div>
                <h3>${escapeHtml(garden.name)}</h3>
                <div class="garden-meta">
                    <div><strong>📍 Vị trí:</strong> ${escapeHtml(garden.location || 'Chưa thiết lập')}</div>
                    <div><strong>👥 Người sở hữu:</strong> ${escapeHtml(ownersText)}</div>
                    <div><strong>🌿 Cây trồng:</strong> ${escapeHtml(garden.cropName || 'Chưa gán')}</div>
                    <div><strong>🔧 Số thiết bị liên kết:</strong> ${garden.deviceCount} thiết bị</div>
                    <div><strong>📝 Mô tả:</strong> ${escapeHtml(garden.description || 'Không có mô tả')}</div>
                </div>
            </div>
            ${actionButtons}
        `;
        grid.appendChild(card);
    });
}

async function editGarden(gardenId) {
    if (!isAdmin) {
        showError('Bạn không có quyền chỉnh sửa khu vườn!');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/garden/${gardenId}`, { headers: getAuthHeaders() });
        if (response.status === 401) {
            logout();
            return;
        }
        if (!response.ok) throw new Error('Không thể tải thông tin khu vườn');

        const garden = Auth.unwrapApiData(await response.json());
        document.getElementById('gardenId').value = garden.id;
        document.getElementById('gardenName').value = garden.name || '';
        document.getElementById('gardenLocation').value = garden.location || '';
        document.getElementById('gardenCropSelect').value = garden.currentCropId || '';
        document.getElementById('gardenDescription').value = garden.description || '';
        renderFarmersList(garden.ownerIds || []);

        document.getElementById('formTitle').textContent = 'Chỉnh sửa khu vườn';
        document.getElementById('submitBtn').textContent = 'Cập nhật khu vườn';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    } catch (error) {
        console.error('Error loading garden details:', error);
        showError('Không thể tải chi tiết khu vườn');
    }
}

async function saveGarden(e) {
    e.preventDefault();

    const id = document.getElementById('gardenId').value;
    const name = document.getElementById('gardenName').value.trim();
    const location = document.getElementById('gardenLocation').value.trim();
    const cropIdVal = document.getElementById('gardenCropSelect').value;
    const currentCropId = cropIdVal ? parseInt(cropIdVal, 10) : null;
    const description = document.getElementById('gardenDescription').value.trim();

    if (!name) {
        showError('Tên khu vườn không được để trống');
        return;
    }

    const ownerCheckboxes = document.querySelectorAll('input[name="gardenOwner"]:checked');
    const ownerIds = Array.from(ownerCheckboxes).map(cb => parseInt(cb.value, 10));

    const payload = {
        name,
        location: location || null,
        currentCropId: currentCropId,
        description: description || null,
        ownerIds: ownerIds
    };

    const isEditing = !!id;

    if (isEditing && !isAdmin) {
        showError('Chỉ có Quản trị viên mới được quyền chỉnh sửa khu vườn!');
        return;
    }

    try {
        let url = `${API_BASE}/garden`;
        let method = 'POST';

        if (isEditing) {
            url = `${API_BASE}/garden/${id}`;
            method = 'PUT';
        }

        const response = await fetch(url, {
            method: method,
            headers: getAuthHeaders(),
            body: JSON.stringify(payload)
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.detail || 'Không thể lưu khu vườn');
        }

        showSuccess(isEditing ? 'Cập nhật khu vườn thành công!' : 'Tạo khu vườn mới thành công!');
        resetGardenForm();
        loadGardens();
    } catch (error) {
        console.error('Error saving garden:', error);
        showError(error.message || 'Không thể lưu khu vườn');
    }
}

async function deleteGarden(id, name) {
    if (!isAdmin) {
        showError('Chỉ có Quản trị viên mới được quyền xóa khu vườn!');
        return;
    }

    if (!confirm(`Bạn có chắc chắn muốn xóa khu vườn "${name}" không?\nLưu ý: Các thiết bị liên kết với khu vườn này sẽ bị hủy liên kết chứ không bị xóa.`)) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/garden/${id}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.detail || 'Không thể xóa khu vườn');
        }

        showSuccess('Xóa khu vườn thành công!');
        loadGardens();
        if (document.getElementById('gardenId').value == id) {
            resetGardenForm();
        }
    } catch (error) {
        console.error('Error deleting garden:', error);
        showError(error.message || 'Không thể xóa khu vườn');
    }
}

function resetGardenForm() {
    document.getElementById('gardenForm').reset();
    document.getElementById('gardenId').value = '';
    document.getElementById('formTitle').textContent = 'Tạo khu vườn mới';
    document.getElementById('submitBtn').textContent = 'Lưu khu vườn';
    renderFarmersList([]);
}

function showSuccess(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #2e7d32;
        color: white;
        padding: 1rem;
        border-radius: 4px;
        z-index: 1001;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);
    setTimeout(() => {
        document.body.removeChild(notification);
    }, 3000);
}

function showError(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #c62828;
        color: white;
        padding: 1rem;
        border-radius: 4px;
        z-index: 1001;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);
    setTimeout(() => {
        document.body.removeChild(notification);
    }, 5000);
}

function logout() {
    Auth.clearAuthStorage();
    window.location.href = 'login.html';
}

function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function escapeJs(str) {
    if (!str) return '';
    return str
        .replace(/\\/g, '\\\\')
        .replace(/'/g, "\\'")
        .replace(/"/g, '\\"');
}
