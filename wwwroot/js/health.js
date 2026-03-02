const HEALTH_URL = '/health';

document.addEventListener('DOMContentLoaded', () => {
    loadHealth();
    // Auto-refresh every 10 seconds
    setInterval(loadHealth, 10000);
});

async function loadHealth() {
    const statusEl = document.getElementById('healthStatus');
    const dbEl = document.getElementById('dbStatus');
    const mqttEl = document.getElementById('mqttStatus');
    const tsEl = document.getElementById('healthTimestamp');
    const rawEl = document.getElementById('healthRaw');
    const lastCheckedEl = document.getElementById('healthLastChecked');

    try {
        const response = await fetch(HEALTH_URL, { method: 'GET' });
        const text = await response.text();

        let data = {};
        try {
            data = text ? JSON.parse(text) : {};
        } catch {
            // Non-JSON body; show as raw text only.
            data = {};
        }

        const ok = response.ok;

        statusEl.textContent = data.status || (ok ? 'Unknown' : 'Unhealthy');
        dbEl.textContent = data.db || 'Unknown';
        mqttEl.textContent = data.mqtt || 'Unknown';

        if (data.timestamp) {
            tsEl.textContent = new Date(data.timestamp).toLocaleString();
        } else {
            tsEl.textContent = 'Unknown';
        }

        lastCheckedEl.textContent = `Last checked: ${new Date().toLocaleString()}`;
        rawEl.textContent = text || '(empty response body)';

        // Simple coloring based on status
        statusEl.style.color = ok && data.status === 'Healthy' ? '#4CAF50' : '#f44336';
        dbEl.style.color = (data.db === 'Connected') ? '#4CAF50' : '#f44336';
        mqttEl.style.color = (data.mqtt === 'Running') ? '#4CAF50' : '#f44336';
    } catch (err) {
        statusEl.textContent = 'Unreachable';
        dbEl.textContent = 'Unknown';
        mqttEl.textContent = 'Unknown';
        tsEl.textContent = 'Unknown';
        lastCheckedEl.textContent = `Last checked: ${new Date().toLocaleString()}`;
        rawEl.textContent = `Request failed: ${err}`;

        statusEl.style.color = '#f44336';
        dbEl.style.color = '#f44336';
        mqttEl.style.color = '#f44336';
    }
}

function goBackToDashboard() {
    window.location.href = 'index.html';
}

