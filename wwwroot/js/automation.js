// Automation Rules JavaScript
const API_BASE = 'http://localhost:5062/api';
let rules = [];

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    checkAuthentication();
    loadDevices();
    loadRules();
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
    const header = document.getElementById('automationHeader');
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
    document.getElementById('newRuleForm').addEventListener('submit', createRule);
    document.getElementById('ruleType').addEventListener('change', updateRuleTypeFields);
}

// Load devices
async function loadDevices() {
    try {
        const response = await fetch(`${API_BASE}/dashboard/latest`, {
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể tải thiết bị');

        const data = await response.json();
        const deviceSelect = document.getElementById('deviceSelect');
        deviceSelect.innerHTML = '';

        data.devices.forEach(device => {
            const option = document.createElement('option');
            option.value = device.id;
            option.textContent = `${device.name} (${device.macAddress})`;
            deviceSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Lỗi tải thiết bị:', error);
        showError('Không thể tải thiết bị');
    }
}

// Update rule type fields
function updateRuleTypeFields() {
    const ruleType = document.getElementById('ruleType').value;
    document.getElementById('scheduleFields').style.display = ruleType === '0' ? 'block' : 'none';
    document.getElementById('thresholdFields').style.display = ruleType === '1' ? 'block' : 'none';
}

// Create new rule
async function createRule(e) {
    e.preventDefault();

    const ruleType = parseInt(document.getElementById('ruleType').value);
    const ruleName = document.getElementById('ruleName').value;
    const deviceId = parseInt(document.getElementById('deviceSelect').value);
    const actuatorType = parseInt(document.getElementById('actuatorType').value);
    const action = document.getElementById('action').value;
    const priority = parseInt(document.getElementById('priority').value);

    let rule = {
        ruleName,
        deviceId,
        ruleType,
        actuatorType,
        action,
        priority,
        isActive: true,
        durationMinutes: null,
        scheduleTime: null,
        scheduleDays: null,
        conditionParameter: null,
        conditionOperator: null,
        conditionValue: null
    };

    // Schedule rule
    if (ruleType === 0) {
        const scheduleTime = document.getElementById('scheduleTime').value;
        const durationMinutes = parseInt(document.getElementById('durationMinutes').value);
        const days = Array.from(document.querySelectorAll('input[name="days"]:checked'))
            .map(cb => cb.value)
            .join(',');

        if (!scheduleTime || !durationMinutes || !days) {
            showError('Vui lòng nhập đầy đủ thông tin lịch trình');
            return;
        }

        rule.scheduleTime = scheduleTime;
        rule.durationMinutes = durationMinutes;
        rule.scheduleDays = days;
    }

    // Threshold rule
    if (ruleType === 1) {
        const conditionParameter = document.getElementById('conditionParameter').value;
        const conditionOperator = document.getElementById('conditionOperator').value;
        const conditionValue = parseFloat(document.getElementById('conditionValue').value);
        const durationMinutes = parseInt(document.getElementById('durationMinutes2').value);

        if (!conditionParameter || !conditionOperator || !conditionValue || !durationMinutes) {
            showError('Vui lòng nhập đầy đủ thông tin ngưỡng');
            return;
        }

        rule.conditionParameter = conditionParameter;
        rule.conditionOperator = conditionOperator;
        rule.conditionValue = conditionValue;
        rule.durationMinutes = durationMinutes;
    }

    // Submit rule
    try {
        const response = await fetch(`${API_BASE}/automation/rules`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(rule)
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể tạo luật');

        showSuccess('Tạo luật thành công!');
        document.getElementById('newRuleForm').reset();
        loadRules();
    } catch (error) {
        console.error('Lỗi tạo luật:', error);
        showError('Không thể tạo luật');
    }
}

// Load rules
async function loadRules() {
    try {
        const response = await fetch(`${API_BASE}/automation/rules`, {
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể tải luật');

        rules = await response.json();
        displayRules();
    } catch (error) {
        console.error('Lỗi tải luật:', error);
        showError('Không thể tải luật');
    }
}

// Display rules
function displayRules() {
    const activeRules = rules.filter(r => r.isActive);
    const inactiveRules = rules.filter(r => !r.isActive);

    // Active rules
    const activeList = document.getElementById('activeRulesList');
    if (activeRules.length === 0) {
        activeList.innerHTML = '<p>Không có luật đang hoạt động</p>';
    } else {
        activeList.innerHTML = '';
        activeRules.forEach(rule => {
            activeList.appendChild(createRuleCard(rule));
        });
    }

    // Inactive rules
    const inactiveList = document.getElementById('inactiveRulesList');
    if (inactiveRules.length === 0) {
        inactiveList.innerHTML = '<p>Không có luật không hoạt động</p>';
    } else {
        inactiveList.innerHTML = '';
        inactiveRules.forEach(rule => {
            inactiveList.appendChild(createRuleCard(rule, true));
        });
    }
}

// Create rule card
function createRuleCard(rule, isInactive = false) {
    const card = document.createElement('div');
    card.className = 'rule-card';
    if (isInactive) card.classList.add('inactive');

    const ruleTypeText = ['Lịch trình', 'Ngưỡng', 'Hẹn giờ'][rule.ruleType] || 'Không xác định';
    const actuatorText = ['Bơm', 'Quạt', 'Đèn', 'Sưởi'][rule.actuatorType];
    const statusClass = rule.isActive ? 'active' : 'inactive';
    const lastExecuted = rule.lastExecuted ? new Date(rule.lastExecuted).toLocaleString() : 'Chưa từng';

    let conditionText = '';
    if (rule.ruleType === 0) {
        // Schedule
        const days = rule.scheduleDays.split(',').slice(0, 3).join(', ');
        conditionText = `${rule.scheduleTime} vào ${days}...`;
    } else if (rule.ruleType === 1) {
        // Threshold
        conditionText = `Khi ${rule.conditionParameter} ${rule.conditionOperator} ${rule.conditionValue}`;
    }

    card.innerHTML = `
        <div class="rule-header">
            <h3>${rule.ruleName}</h3>
            <span class="rule-status ${statusClass}">${rule.isActive ? '✓ Hoạt động' : '✕ Không hoạt động'}</span>
        </div>
        <div class="rule-details">
            <div class="detail-item">
                <span class="label">Loại:</span>
                <span class="value">${ruleTypeText}</span>
            </div>
            <div class="detail-item">
                <span class="label">Thiết bị chấp hành:</span>
                <span class="value">${actuatorText}</span>
            </div>
            <div class="detail-item">
                <span class="label">Hành động:</span>
                <span class="value">${rule.action}</span>
            </div>
            <div class="detail-item">
                <span class="label">Điều kiện:</span>
                <span class="value">${conditionText}</span>
            </div>
            <div class="detail-item">
                <span class="label">Ưu tiên:</span>
                <span class="value">${rule.priority}/10</span>
            </div>
            <div class="detail-item">
                <span class="label">Lần thực thi gần nhất:</span>
                <span class="value">${lastExecuted}</span>
            </div>
        </div>
        <div class="rule-actions">
            <button class="btn-small" onclick="toggleRuleStatus(${rule.id})">
                ${rule.isActive ? '🔇 Tắt' : '🔊 Bật'}
            </button>
            <button class="btn-small danger" onclick="deleteRule(${rule.id})">🗑️ Xóa</button>
        </div>
    `;

    return card;
}

// Toggle rule status
async function toggleRuleStatus(ruleId) {
    try {
        const response = await fetch(`${API_BASE}/automation/rules/${ruleId}/toggle`, {
            method: 'PUT',
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể thay đổi trạng thái luật');

        showSuccess('Đã cập nhật luật');
        loadRules();
    } catch (error) {
        console.error('Lỗi cập nhật trạng thái luật:', error);
        showError('Không thể cập nhật luật');
    }
}

// Delete rule
async function deleteRule(ruleId) {
    if (!confirm('Bạn có chắc chắn muốn xóa luật này?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/automation/rules/${ruleId}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) throw new Error('Không thể xóa luật');

        showSuccess('Đã xóa luật');
        loadRules();
    } catch (error) {
        console.error('Lỗi xóa luật:', error);
        showError('Không thể xóa luật');
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
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        document.body.removeChild(notification);
    }, 5000);
}
