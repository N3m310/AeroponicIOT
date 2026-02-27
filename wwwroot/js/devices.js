// Device Management JavaScript
const API_BASE = 'http://localhost:5062/api';
let devices = [];
let crops = [];
let editingDeviceId = null;

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    checkAuthentication();
    loadCrops();
    loadDevices();
    setupEventListeners();
});

// Check authentication
function checkAuthentication() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    const username = localStorage.getItem('username');
    const role = localStorage.getItem('role');
    const header = document.getElementById('devicesHeader');
    header.innerHTML = `
        <span>${username} <small>(${role})</small></span>
        <button id="backBtn" class="btn-secondary" onclick="goBack()">← Về trang tổng quan</button>
        <button id="logoutBtn" class="btn-secondary">Đăng xuất</button>
    `;
    document.getElementById('logoutBtn').addEventListener('click', logout);
}

// Get authorization headers
function getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// Setup event listeners
function setupEventListeners() {
    document.getElementById('addDeviceForm').addEventListener('submit', createDevice);
    document.getElementById('editDeviceForm').addEventListener('submit', updateDevice);
}

// Load crops
async function loadCrops() {
    try {
        const response = await fetch(`${API_BASE}/crop`, {
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể tải cây trồng');

        crops = await response.json();
        populateCropSelects();
    } catch (error) {
        console.error('Lỗi tải cây trồng:', error);
    }
}

// Populate crop dropdowns
function populateCropSelects() {
    const select1 = document.getElementById('cropSelect');
    const select2 = document.getElementById('editCropSelect');

    [select1, select2].forEach(select => {
        select.innerHTML = '<option value="">Chưa gán cây trồng</option>';
        crops.forEach(crop => {
            const option = document.createElement('option');
            option.value = crop.id;
            option.textContent = crop.name;
            select.appendChild(option);
        });
    });
}

// Load devices
async function loadDevices() {
    try {
        const response = await fetch(`${API_BASE}/device`, {
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể tải thiết bị');

        devices = await response.json();
        displayDevices();
    } catch (error) {
        console.error('Lỗi tải thiết bị:', error);
        showError('Không thể tải thiết bị');
    }
}

// Display devices
function displayDevices() {
    const activeDevices = devices.filter(d => d.isActive);
    const inactiveDevices = devices.filter(d => !d.isActive);

    // Active devices
    const grid = document.getElementById('devicesGrid');
    if (activeDevices.length === 0) {
        grid.innerHTML = '<p class="no-devices">Không có thiết bị hoạt động. Hãy tạo thiết bị mới để bắt đầu!</p>';
    } else {
        grid.innerHTML = '';
        activeDevices.forEach(device => {
            grid.appendChild(createDeviceCard(device));
        });
    }

    // Inactive devices
    const inactiveGrid = document.getElementById('inactiveDevicesGrid');
    if (inactiveDevices.length === 0) {
        inactiveGrid.innerHTML = '<p class="no-devices">Không có thiết bị không hoạt động</p>';
    } else {
        inactiveGrid.innerHTML = '';
        inactiveDevices.forEach(device => {
            inactiveGrid.appendChild(createDeviceCard(device, true));
        });
    }
}

// Create device card
function createDeviceCard(device, isInactive = false) {
    const card = document.createElement('div');
    card.className = 'device-card';
    if (isInactive) card.classList.add('inactive');

    const lastSeen = device.lastSeen ? new Date(device.lastSeen).toLocaleString() : 'Chưa từng';
    const createdAt = device.createdAt ? new Date(device.createdAt).toLocaleDateString() : 'Không rõ';
    const cropDisplay = device.cropName || 'Chưa gán';
    const statusClass = device.isActive ? 'active' : 'inactive';

    card.innerHTML = `
        <div class="card-header">
            <h3>${device.name}</h3>
            <span class="status-badge ${statusClass}">${device.isActive ? '🟢 Hoạt động' : '🔴 Không hoạt động'}</span>
        </div>
        <div class="card-details">
            <div class="detail-row">
                <span class="label">Địa chỉ MAC:</span>
                <span class="value mac">${device.macAddress}</span>
            </div>
            <div class="detail-row">
                <span class="label">Cây trồng:</span>
                <span class="value">${cropDisplay}</span>
            </div>
            <div class="detail-row">
                <span class="label">Trạng thái:</span>
                <span class="value">${device.status || 'Không rõ'}</span>
            </div>
            <div class="detail-row">
                <span class="label">Ngày tạo:</span>
                <span class="value">${createdAt}</span>
            </div>
            <div class="detail-row">
                <span class="label">Lần thấy gần nhất:</span>
                <span class="value">${lastSeen}</span>
            </div>
        </div>
        <div class="card-actions">
            <button class="btn-small edit" onclick="openEditModal(${device.id})">✏️ Sửa</button>
            <button class="btn-small delete" onclick="deleteDevice(${device.id})">🗑️ Xóa</button>
        </div>
    `;

    return card;
}

// Create device
async function createDevice(e) {
    e.preventDefault();

    const deviceData = {
        name: document.getElementById('deviceName').value,
        macAddress: document.getElementById('macAddress').value,
        currentCropId: document.getElementById('cropSelect').value ? 
            parseInt(document.getElementById('cropSelect').value) : null
    };

    // Validate MAC address format
    if (!/^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$/.test(deviceData.macAddress)) {
        showError('Định dạng địa chỉ MAC không hợp lệ (dùng AA:BB:CC:DD:EE:FF)');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/device`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(deviceData)
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.detail || 'Không thể tạo thiết bị');
        }

        showSuccess('Tạo thiết bị thành công!');
        document.getElementById('addDeviceForm').reset();
        loadDevices();
    } catch (error) {
        console.error('Lỗi tạo thiết bị:', error);
        showError(error.message || 'Không thể tạo thiết bị');
    }
}

// Open edit modal
async function openEditModal(deviceId) {
    const device = devices.find(d => d.id === deviceId);
    if (!device) return;

    editingDeviceId = deviceId;
    document.getElementById('editDeviceId').value = deviceId;
    document.getElementById('editDeviceName').value = device.name;
    document.getElementById('editCropSelect').value = device.currentCropId || '';
    document.getElementById('editStatus').value = device.status || 'active';

    document.getElementById('editModal').style.display = 'flex';
}

// Close edit modal
function closeEditModal() {
    document.getElementById('editModal').style.display = 'none';
    editingDeviceId = null;
}

// Update device
async function updateDevice(e) {
    e.preventDefault();

    const deviceId = parseInt(document.getElementById('editDeviceId').value);
    const updateData = {
        name: document.getElementById('editDeviceName').value,
        currentCropId: document.getElementById('editCropSelect').value ? 
            parseInt(document.getElementById('editCropSelect').value) : null,
        status: document.getElementById('editStatus').value
    };

    try {
        const response = await fetch(`${API_BASE}/device/${deviceId}`, {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify(updateData)
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể cập nhật thiết bị');

        showSuccess('Cập nhật thiết bị thành công!');
        closeEditModal();
        loadDevices();
    } catch (error) {
        console.error('Lỗi cập nhật thiết bị:', error);
        showError('Không thể cập nhật thiết bị');
    }
}

// Delete device
async function deleteDevice(deviceId) {
    const device = devices.find(d => d.id === deviceId);
    if (!device) return;

    if (!confirm(`Xóa thiết bị "${device.name}"? Hành động này không thể hoàn tác.`)) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/device/${deviceId}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể xóa thiết bị');

        showSuccess('Đã xóa thiết bị thành công');
        loadDevices();
    } catch (error) {
        console.error('Lỗi xóa thiết bị:', error);
        showError('Không thể xóa thiết bị');
    }
}

// Logout
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');
    window.location.href = 'login.html';
}

// Go back to dashboard
function goBack() {
    window.location.href = 'index.html';
}

// Show success
function showSuccess(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #4CAF50;
        color: white;
        padding: 1rem;
        border-radius: 4px;
        z-index: 1001;
        max-width: 400px;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        document.body.removeChild(notification);
    }, 3000);
}

// Show error
function showError(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #f44336;
        color: white;
        padding: 1rem;
        border-radius: 4px;
        z-index: 1001;
        max-width: 400px;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        document.body.removeChild(notification);
    }, 5000);
}

// Close modal when clicking outside
window.addEventListener('click', function(e) {
    const modal = document.getElementById('editModal');
    if (e.target === modal) {
        closeEditModal();
    }
});
