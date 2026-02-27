// Dashboard JavaScript
const API_BASE = 'http://localhost:5062/api'; // Adjust port as needed

// Notification settings
let lastUnreadCount = 0;
let soundEnabled = localStorage.getItem('notificationSoundEnabled') !== 'false'; // Default to true

// DOM elements
const refreshBtn = document.getElementById('refreshBtn');
const lastUpdate = document.getElementById('lastUpdate');
const totalDevices = document.getElementById('totalDevices');
const activeDevices = document.getElementById('activeDevices');
const activeAlerts = document.getElementById('activeAlerts');
const devicesContainer = document.getElementById('devicesContainer');
const alertsContainer = document.getElementById('alertsContainer');
const zoneTabs = document.getElementById('zoneTabs');
const sensorQuickGrid = document.getElementById('sensorQuickGrid');
const actuatorQuickGrid = document.getElementById('actuatorQuickGrid');
const deviceSelect = document.getElementById('deviceSelect');
const controlForm = document.getElementById('controlForm');
const deviceModal = document.getElementById('deviceModal');
const deviceModalTitle = document.getElementById('deviceModalTitle');
const deviceModalContent = document.getElementById('deviceModalContent');
const closeModal = document.querySelector('.close');

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    checkAuthentication();
    
    loadDashboardData();
    setInterval(loadDashboardData, 30000); // Refresh every 30 seconds
});

// Check if user is authenticated
function checkAuthentication() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    // Add user info to header
    const username = localStorage.getItem('username');
    const role = localStorage.getItem('role');
    
    // Create user info element
    const headerControls = document.querySelector('.header-controls');
    const userInfo = document.createElement('div');
    userInfo.className = 'user-info';
    userInfo.innerHTML = `
        <div class="notification-bell">
            <button id="notificationBell" class="bell-button">
                🔔
                <span id="notificationBadge" class="notification-badge" style="display:none;">0</span>
            </button>
            <button id="soundToggle" class="sound-toggle" title="Toggle notification sound">
                ${soundEnabled ? '🔊' : '🔇'}
            </button>
            <div id="notificationDropdown" class="notification-dropdown" style="display:none;">
                <div class="notification-header">
                    <h3>Thông báo</h3>
                    <button id="clearNotificationsBtn" class="clear-btn">Xóa tất cả</button>
                </div>
                <div id="notificationsList" class="notifications-list">
                    <p class="no-notifications">Không có thông báo mới</p>
                </div>
            </div>
        </div>
        <span>${username} <small>(${role})</small></span>
        <button id="logoutBtn" class="btn-secondary">Đăng xuất</button>
    `;
    headerControls.appendChild(userInfo);
    
    // Load notifications
    loadNotifications();
    setInterval(loadNotifications, 10000); // Check every 10 seconds
    
    // Notification bell click handler
    document.getElementById('notificationBell').addEventListener('click', toggleNotificationDropdown);
    document.getElementById('clearNotificationsBtn').addEventListener('click', clearAllNotifications);
    document.getElementById('soundToggle').addEventListener('click', toggleNotificationSound);
    
    document.getElementById('logoutBtn').addEventListener('click', logout);
}

// Toggle notification dropdown
function toggleNotificationDropdown() {
    const dropdown = document.getElementById('notificationDropdown');
    dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
}

// Load notifications
async function loadNotifications() {
    try {
        const response = await fetch(`${API_BASE}/notification/unread`, {
            headers: getAuthHeaders()
        });
        
        if (response.status === 401) {
            logout();
            return;
        }
        
        if (!response.ok) return;
        
        const data = await response.json();
        const badge = document.getElementById('notificationBadge');
        const notificationsList = document.getElementById('notificationsList');
        
        // Check if new notifications arrived
        if (data.unreadCount > lastUnreadCount && soundEnabled && data.unreadCount > 0) {
            playNotificationSound();
        }
        lastUnreadCount = data.unreadCount;
        
        if (data.unreadCount > 0) {
            badge.textContent = data.unreadCount;
            badge.style.display = 'inline-block';
            
            notificationsList.innerHTML = '';
            data.notifications.forEach(notification => {
                const notifElement = document.createElement('div');
                notifElement.className = `notification-item notification-${notification.type.toLowerCase()}`;
                notifElement.innerHTML = `
                    <div class="notification-content">
                        <h4>${notification.title}</h4>
                        <p>${notification.message}</p>
                        <small>${new Date(notification.createdAt).toLocaleString()}</small>
                    </div>
                    <button class="mark-read-btn" onclick="markNotificationAsRead(${notification.id})">✓</button>
                `;
                notificationsList.appendChild(notifElement);
            });
        } else {
            badge.style.display = 'none';
            notificationsList.innerHTML = '<p class="no-notifications">Không có thông báo mới</p>';
        }
    } catch (error) {
        console.error('Error loading notifications:', error);
    }
}

// Mark notification as read
async function markNotificationAsRead(notificationId) {
    try {
        await fetch(`${API_BASE}/notification/${notificationId}/read`, {
            method: 'POST',
            headers: getAuthHeaders()
        });
        loadNotifications();
    } catch (error) {
        console.error('Error marking notification as read:', error);
    }
}

// Clear all notifications
async function clearAllNotifications() {
    try {
        if (confirm('Bạn có muốn xóa toàn bộ thông báo?')) {
            await fetch(`${API_BASE}/notification/clear`, {
                method: 'DELETE',
                headers: getAuthHeaders()
            });
            loadNotifications();
        }
    } catch (error) {
        console.error('Error clearing notifications:', error);
    }
}

// Toggle notification sound
function toggleNotificationSound() {
    soundEnabled = !soundEnabled;
    localStorage.setItem('notificationSoundEnabled', soundEnabled);
    const toggleBtn = document.getElementById('soundToggle');
    toggleBtn.textContent = soundEnabled ? '🔊' : '🔇';
    showSuccess(`Âm thanh thông báo đã ${soundEnabled ? 'bật' : 'tắt'}`);
}

// Play notification sound using Web Audio API
function playNotificationSound() {
    try {
        // Create audio context
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        
        // Create oscillator and gain nodes for beep sound
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();
        
        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);
        
        // Set frequency and duration for pleasant notification beep
        oscillator.frequency.value = 800; // Hz
        oscillator.type = 'sine';
        
        // Volume envelope
        gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);
        
        // Play sound
        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.3);
    } catch (error) {
        console.log('Could not play notification sound:', error);
    }
}

// Logout function
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');
    window.location.href = 'login.html';
}

// Get authorization header with token
function getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// Event listeners
refreshBtn.addEventListener('click', loadDashboardData);
controlForm.addEventListener('submit', handleControlSubmit);
closeModal.addEventListener('click', () => deviceModal.style.display = 'none');
window.addEventListener('click', (e) => {
    if (e.target === deviceModal) {
        deviceModal.style.display = 'none';
    }
});

// Load dashboard data
async function loadDashboardData() {
    try {
        const response = await fetch(`${API_BASE}/dashboard/latest`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (response.status === 401) {
            logout();
            return;
        }
        
        if (!response.ok) throw new Error('Không thể tải dữ liệu bảng điều khiển');

        const data = await response.json();
        updateDashboard(data);
        updateLastUpdate();
    } catch (error) {
        console.error('Lỗi tải dữ liệu bảng điều khiển:', error);
        showError('Không thể tải dữ liệu bảng điều khiển');
    }
}

// Update dashboard with data
function updateDashboard(data) {
    // Update overview stats
    totalDevices.textContent = data.totalDevices;
    activeDevices.textContent = data.activeDevices;
    activeAlerts.textContent = data.activeAlerts.length;

    updateZoneTabs(data.devices);
    updateQuickOverview(data.devices);

    // Update devices
    updateDevices(data.devices);

    // Update alerts
    updateAlerts(data.activeAlerts);

    // Update device select for manual control
    updateDeviceSelect(data.devices);
}

function updateZoneTabs(devices) {
    if (!zoneTabs) return;

    if (!devices || devices.length === 0) {
        zoneTabs.innerHTML = '<div class="zone-tab">Chưa có khu vườn</div>';
        return;
    }

    zoneTabs.innerHTML = devices.slice(0, 4).map((device, index) =>
        `<div class="zone-tab">Khu vườn ${index + 1}: ${device.name}</div>`
    ).join('');
}

function updateQuickOverview(devices) {
    if (!sensorQuickGrid || !actuatorQuickGrid) return;

    const activeDevice = devices.find(device => device.isActive) || devices[0];
    const sensorData = activeDevice?.latestSensorData;

    if (!activeDevice || !sensorData) {
        sensorQuickGrid.innerHTML = '<div class="quick-card"><h3>Không có dữ liệu cảm biến</h3><div class="quick-status">Vui lòng kiểm tra thiết bị</div></div>';
    } else {
        sensorQuickGrid.innerHTML = [
            buildQuickCard('🌡️ Cảm biến nhiệt độ', formatSensorValue(sensorData.waterTemperature, '°C', 1)),
            buildQuickCard('💧 Cảm biến độ ẩm không khí', formatSensorValue(sensorData.airHumidity, '%', 1)),
            buildQuickCard('⚗️ Cảm biến pH', formatSensorValue(sensorData.ph, '', 2)),
            buildQuickCard('🧪 Cảm biến TDS', formatSensorValue(sensorData.tds, ' ppm', 0))
        ].join('');
    }

    actuatorQuickGrid.innerHTML = [
        buildQuickCard('💦 Bơm', 'Sẵn sàng'),
        buildQuickCard('🌀 Quạt', 'Sẵn sàng'),
        buildQuickCard('💡 Đèn', 'Sẵn sàng'),
        buildQuickCard('🔥 Sưởi', 'Sẵn sàng')
    ].join('');
}

function buildQuickCard(title, value) {
    return `
        <div class="quick-card">
            <h3>${title}</h3>
            <div class="quick-value">${value}</div>
            <div class="quick-status">Trạng thái: Tốt</div>
        </div>
    `;
}

function formatSensorValue(value, unit, digits) {
    if (value === null || value === undefined || Number.isNaN(value)) return '--';
    return `${Number(value).toFixed(digits)}${unit}`;
}

// Update devices display
function updateDevices(devices) {
    devicesContainer.innerHTML = '';

    if (devices.length === 0) {
        devicesContainer.innerHTML = '<p>Chưa có thiết bị nào được đăng ký</p>';
        return;
    }

    devices.forEach(device => {
        const deviceCard = createDeviceCard(device);
        devicesContainer.appendChild(deviceCard);
    });
}

// Create device card
function createDeviceCard(device) {
    const card = document.createElement('div');
    card.className = 'device-card';

    const statusClass = device.isActive ? 'status-active' : 'status-inactive';
    const statusText = device.isActive ? 'Hoạt động' : 'Không hoạt động';

    let sensorHtml = '<div class="sensor-grid">';
    if (device.latestSensorData) {
        const sensors = [
            { label: 'pH', value: device.latestSensorData.ph, unit: '' },
            { label: 'TDS', value: device.latestSensorData.tds, unit: ' ppm' },
            { label: 'Temp', value: device.latestSensorData.waterTemperature, unit: '°C' },
            { label: 'Humidity', value: device.latestSensorData.airHumidity, unit: '%' }
        ];

        sensors.forEach(sensor => {
            if (sensor.value !== null && sensor.value !== undefined) {
                sensorHtml += `
                    <div class="sensor-item">
                        <div class="sensor-label">${sensor.label}</div>
                        <div class="sensor-value">${sensor.value.toFixed(1)}${sensor.unit}</div>
                    </div>
                `;
            }
        });
    } else {
        sensorHtml += '<p>Không có dữ liệu cảm biến</p>';
    }
    sensorHtml += '</div>';

    const lastSeen = device.lastSeen ? new Date(device.lastSeen).toLocaleString() : 'Chưa từng';

    card.innerHTML = `
        <div class="device-header">
            <div class="device-name">${device.name}</div>
            <div class="device-status ${statusClass}">${statusText}</div>
        </div>
        <div class="device-mac">MAC: ${device.macAddress}</div>
        <div class="device-crop">Cây trồng: ${device.cropName || 'Chưa gán'}</div>
        <div class="device-lastseen">Lần thấy gần nhất: ${lastSeen}</div>
        ${sensorHtml}
        <div class="device-actions">
            <button class="btn-secondary" onclick="showDeviceDetails(${device.id})">Chi tiết</button>
        </div>
    `;

    return card;
}

// Update alerts display
function updateAlerts(alerts) {
    alertsContainer.innerHTML = '';

    if (alerts.length === 0) {
        alertsContainer.innerHTML = '<p>Không có cảnh báo đang hoạt động</p>';
        return;
    }

    alerts.forEach(alert => {
        const alertItem = createAlertItem(alert);
        alertsContainer.appendChild(alertItem);
    });
}

// Create alert item
function createAlertItem(alert) {
    const item = document.createElement('div');
    item.className = 'alert-item';

    let alertClass = 'alert-info';
    if (alert.type === 1) alertClass = 'alert-warning'; // Warning
    if (alert.type === 2) alertClass = 'alert-error';   // Error

    item.classList.add(alertClass);

    const timestamp = new Date(alert.timestamp).toLocaleString();

    item.innerHTML = `
        <div class="alert-title">${alert.title}</div>
        <div class="alert-message">${alert.message || ''}</div>
        <div class="alert-timestamp">${timestamp}</div>
    `;

    return item;
}

// Update device select for manual control
function updateDeviceSelect(devices) {
    deviceSelect.innerHTML = '<option value="">Chọn thiết bị</option>';

    devices.forEach(device => {
        if (device.isActive) {
            const option = document.createElement('option');
            option.value = device.macAddress;
            option.textContent = `${device.name} (${device.macAddress})`;
            deviceSelect.appendChild(option);
        }
    });
}

// Handle manual control form submission
async function handleControlSubmit(e) {
    e.preventDefault();

    const formData = new FormData(controlForm);
    const controlData = {
        macAddress: formData.get('deviceSelect'),
        actuatorType: parseInt(formData.get('actuatorSelect')),
        action: formData.get('actionSelect'),
        controlType: 1, // Manual
        reason: formData.get('reasonInput') || null
    };

    try {
        const response = await fetch(`${API_BASE}/actuator/control`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(controlData)
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể gửi lệnh điều khiển');

        const result = await response.json();
        showSuccess(result.message);

        // Reset form
        controlForm.reset();

        // Refresh data
        loadDashboardData();
    } catch (error) {
        console.error('Lỗi gửi lệnh điều khiển:', error);
        showError('Không thể gửi lệnh điều khiển');
    }
}

// Show device details
async function showDeviceDetails(deviceId) {
    try {
        // Get device history (last 24 hours)
        const response = await fetch(`${API_BASE}/dashboard/history/${deviceId}?hours=24`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (response.status === 401) {
            logout();
            return;
        }
        
        if (!response.ok) throw new Error('Không thể tải lịch sử thiết bị');

        const history = await response.json();

        // Create modal content
        let content = '<h3>Lịch sử cảm biến (24 giờ gần nhất)</h3>';

        if (history.length === 0) {
            content += '<p>Không có dữ liệu cảm biến</p>';
        } else {
            content += '<table style="width: 100%; border-collapse: collapse;">';
            content += '<thead><tr><th>Thời gian</th><th>pH</th><th>TDS</th><th>Nhiệt độ (°C)</th><th>Độ ẩm (%)</th></tr></thead>';
            content += '<tbody>';

            history.forEach(log => {
                const time = new Date(log.timestamp).toLocaleString();
                content += `<tr>
                    <td>${time}</td>
                    <td>${log.ph?.toFixed(2) || '-'}</td>
                    <td>${log.tds?.toFixed(0) || '-'}</td>
                    <td>${log.waterTemperature?.toFixed(1) || '-'}</td>
                    <td>${log.airHumidity?.toFixed(1) || '-'}</td>
                </tr>`;
            });

            content += '</tbody></table>';
        }

        deviceModalContent.innerHTML = content;
        deviceModalTitle.textContent = `Chi tiết thiết bị - ID: ${deviceId}`;
        deviceModal.style.display = 'block';
    } catch (error) {
        console.error('Lỗi tải chi tiết thiết bị:', error);
        showError('Không thể tải chi tiết thiết bị');
    }
}

// Utility functions
function updateLastUpdate() {
    lastUpdate.textContent = `Cập nhật lần cuối: ${new Date().toLocaleString()}`;
}

function showSuccess(message) {
    // Simple success notification
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
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        document.body.removeChild(notification);
    }, 3000);
}

function showError(message) {
    // Simple error notification
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
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        document.body.removeChild(notification);
    }, 5000);
}

// Navigate to charts page
function goToCharts() {
    window.location.href = 'charts.html';
}

// Navigate to automation page
function goToAutomation() {
    window.location.href = 'automation.html';
}

// Navigate to device management page
function goToDevices() {
    window.location.href = 'devices.html';
}