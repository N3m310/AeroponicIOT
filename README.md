# Smart Farm IoT — AeroponicIOT

A comprehensive IoT system for monitoring and controlling **aeroponic farming environments** using ASP.NET Core Web API, MQTT, and a modern web dashboard.

---

## Features

- **Real-time Sensor Monitoring** — Collect pH, TDS/EC, temperature, humidity, and light intensity from IoT devices via MQTT or HTTP
- **Device Provisioning** — Self-registration with shared key + claim-code flow for secure device onboarding
- **Automatic Control** — Smart actuator control based on crop-specific thresholds
- **Manual Control** — Web-based toggle of pumps, fans, lights, heaters, roof, and float switch
- **Automation Rules** — Schedule-based and threshold-based automatic device control with priority ordering
- **Crop Management** — Configurable crop types (Lettuce, Basil, Strawberry) with growth stages and environmental parameters
- **Garden Management** — Organize devices into gardens with filtering on the dashboard
- **Alert System** — Automatic alerts when environmental conditions exceed crop-stage optimal ranges
- **Notification System** — In-app dashboard notifications + email (SMTP via MailKit)
- **AI-Powered Suggestions** — Optional GPT-4o-mini analysis of sensor trends with actionable farming advice
- **Analytics & Charts** — Real-time graphs and historical data analysis with Chart.js
- **Web Dashboard** — Full Vietnamese-language dashboard with KPI cards, device health, and controls
- **REST API** — Complete OpenAPI/Swagger-documented API
- **MQTT Broker** — Built-in MQTTnet broker (port 1883) with TLS/mTLS, topic ACLs, and Zigbee2MQTT bridge
- **User Authentication** — JWT-based login with role-based access control (Farmer, Administrator)
- **User Management** — Admin panel for managing users and roles
- **Observability** — OpenTelemetry tracing/metrics, performance budgets, structured logging, health checks
- **System Health** — Live/ready probes for database and MQTT, Grafana dashboard included

---

## Architecture

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8 Web API with Entity Framework Core 8 |
| **IoT Messaging** | MQTT Broker (MQTTnet) on port 1883 |
| **Database** | SQL Server 2022 |
| **Cache** | Redis 7 (optional, falls back to in-memory) |
| **Frontend** | HTML5, CSS3, JavaScript (Vanilla) with Chart.js |
| **Authentication** | JWT with role-based access (Farmer, Administrator) |
| **Notifications** | In-app + SMTP email (MailKit) |
| **AI** | OpenAI-compatible API (configurable endpoint/model) |
| **Observability** | OpenTelemetry (OTLP exporter), OpenMetrics |
| **Containerization** | Docker, Docker Compose |

---

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or containerized)

### Installation & Run

```bash
cd src/AeroponicIOT
dotnet run
```

Access the dashboard at **http://localhost:5062**

> On startup, EF Core migrations are applied automatically and default crops (Lettuce, Basil, Strawberry) are seeded if the database is available.

### Docker Compose (full stack)

```bash
docker compose up -d
```

This starts:
- **app** — ASP.NET Core API (ports 5062:80, 1883:1883)
- **db** — SQL Server 2022 (port 1433)

Optional profiles:
```bash
# With Redis cache
docker compose --profile redis up -d

# With Zigbee2MQTT coordinator
docker compose --profile zigbee up -d
```

---

## API Overview

All endpoints return a standard `ApiResponse<T>` envelope:
```json
{
  "success": true,
  "message": "Success",
  "data": { ... },
  "timestamp": "2026-06-27T00:00:00Z"
}
```

Error responses follow [RFC 7231 Problem Details](https://httpstatuses.com/) (`application/problem+json`).

### Authentication

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/authentication/register` | POST | Anonymous (rate-limited) | Register a new user |
| `/api/authentication/login` | POST | Anonymous (rate-limited) | Login, receive JWT |
| `/api/authentication/me` | GET | `[Authorize]` | Get current user info |

### Devices

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/device/self-register` | POST | `X-Device-Key` (rate-limited) | Device self-registration |
| `/api/device/claim` | POST | `[Authorize]` (rate-limited) | Claim a pending device |
| `/api/device` | GET | `[Authorize]` | List user's devices |
| `/api/device/pending` | GET | AdminOnly | List unclaimed devices |
| `/api/device/{id}` | GET | `[Authorize]` | Get device details |
| `/api/device` | POST | `[Authorize]` | Create device (admin) |
| `/api/device/{id}` | PUT | `[Authorize]` | Update device |
| `/api/device/{id}` | DELETE | `[Authorize]` | Delete device |

### Sensor Data

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/sensor` | POST | `X-Device-Key` or JWT | Receive sensor readings |

### Dashboard

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/dashboard/latest` | GET | `[Authorize]` | Paginated devices + latest sensor data + active alerts |
| `/api/dashboard/kpi` | GET | `[Authorize]` | System-wide KPIs (total devices, active devices, alerts summary) |
| `/api/dashboard/health` | GET | `[Authorize]` | System health overview (DB, MQTT, email status) |
| `/api/dashboard/history/{deviceId}` | GET | `[Authorize]` | Historical sensor data for a device (24h to 30d) |

### Actuator Control

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/actuator/control` | POST | `[Authorize]` | Send ON/OFF command via MQTT |
| `/api/actuator/logs/{deviceId}` | GET | `[Authorize]` | Actuator command history |

### Crops

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/crop` | GET | `[Authorize]` | List all crops |
| `/api/crop/{id}` | GET | `[Authorize]` | Get crop with stages + device count |
| `/api/crop` | POST | `[Authorize]` | Create crop with stages |
| `/api/crop/{id}` | PUT | `[Authorize]` | Update crop |
| `/api/crop/{id}` | DELETE | `[Authorize]` | Delete crop (fails if devices assigned) |

### Gardens

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/garden` | GET | `[Authorize]` | List gardens |
| `/api/garden/{id}` | GET | `[Authorize]` | Get garden details |
| `/api/garden` | POST | `[Authorize]` | Create garden |
| `/api/garden/{id}` | PUT | AdminOnly | Update garden |
| `/api/garden/{id}` | DELETE | AdminOnly | Delete garden (detaches devices) |
| `/api/garden/{id}/devices` | GET | `[Authorize]` | Devices in a garden |

### Automation Rules

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/automation/rules` | GET | `[Authorize]` | List rules (user-scoped) |
| `/api/automation/rules/{id}` | GET | `[Authorize]` | Get rule by ID |
| `/api/automation/rules` | POST | `[Authorize]` | Create rule |
| `/api/automation/rules/{id}` | PUT | `[Authorize]` | Update rule |
| `/api/automation/rules/{id}` | DELETE | `[Authorize]` | Delete rule |
| `/api/automation/rules/{id}/toggle` | PUT | `[Authorize]` | Toggle rule active/inactive |
| `/api/automation/rules/device/{deviceId}` | GET | `[Authorize]` | Rules for a specific device |

### MQTT

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/mqtt/status` | GET | FarmerOrAdmin | Broker + Zigbee bridge status |
| `/api/mqtt/publish` | POST | AdminOnly | Publish raw MQTT message |

### Notifications

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/notification/unread` | GET | `[Authorize]` | Unread notifications |
| `/api/notification/{id}/read` | POST | `[Authorize]` | Mark as read |
| `/api/notification/clear` | DELETE | `[Authorize]` | Clear all notifications |
| `/api/notification/email-health` | GET | AdminOnly | Email service health |
| `/api/notification/test-email` | POST | AdminOnly | Send test email |

### AI Suggestions

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/aisuggestion/analyze/{deviceId}` | POST | AdminOnly | Trigger AI analysis |

### Users (Admin)

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/users` | GET | AdminOnly | List all users |
| `/api/users/{id}` | GET | AdminOnly | Get user |
| `/api/users/{id}` | PUT | AdminOnly | Update user role/email |
| `/api/users/{id}` | DELETE | AdminOnly | Delete user |

### Debug (Development only)

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/debug/create-test-user` | POST | Development only | Create `devuser` / `P@ssw0rd1` (Administrator) |

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `GET /health/live` | Liveness probe |
| `GET /health/ready` | Readiness probe (DB + MQTT) |
| `GET /health` | Alias for readiness |

### Swagger UI

Available at **http://localhost:5062/swagger** (development only).

---

## Device Provisioning Flow

```
1. ESP32 powers on
   └─ POST /api/device/self-register  (with X-Device-Key header)
      └─ Returns claim code (6 chars, e.g. "XK9M2B")
2. User logs into dashboard
   └─ POST /api/device/claim  { claimCode: "XK9M2B", currentCropId, gardenId }
      └─ Device is now owned by the user
3. Device connects via MQTT
   └─ Username = MAC address, Password = HMAC-SHA256(MAC, SharedKey)
      └─ Starts publishing sensor data
```

### HTTP Sensor Ingestion

```bash
curl -X POST http://localhost:5062/api/sensor \
  -H "Content-Type: application/json" \
  -H "X-Device-Key: YOUR_PROVISIONING_SHARED_KEY" \
  -d '{
    "macAddress": "AA:BB:CC:DD:EE:01",
    "ph": 6.2,
    "tds": 950,
    "waterTemperature": 22.5,
    "airHumidity": 68.0,
    "lightIntensity": 12000
  }'
```

---

## MQTT Integration

### Broker Details
- **Plaintext port:** 1883 (default)
- **TLS port:** 8883 (when enabled)
- **Host:** 0.0.0.0 (configurable)
- **Protocol:** MQTT v5 via MQTTnet

### Device Topics

| Direction | Topic | Payload |
|-----------|-------|---------|
| Device → Server | `devices/{macAddress}/sensor` | `{ macAddress, ph, tds, waterTemperature, airHumidity, lightIntensity }` |
| Server → Device | `devices/{macAddress}/control` | `{ actuatorType, action }` |

### Authentication

Devices authenticate with:
- **Username:** MAC address (e.g., `AA:BB:CC:DD:EE:01`)
- **Password:** `HMAC-SHA256(MAC, Provisioning:SharedKey)`

### Security Options

- **TLS encryption** with server certificate
- **mTLS** with client certificate allowlisting (by issuer DN or thumbprint)
- **Topic ACL enforcement** — devices can only publish/subscribe to their own topics
- **Zigbee2MQTT bridge** with optional dedicated ACL
- **Disable plaintext** endpoint for TLS-only operation

---

## Dashboard

The web dashboard is served from `wwwroot/` and includes:

| Page | URL | Description |
|------|-----|-------------|
| **Dashboard** | `/` | KPI cards, device health grid, active alerts, manual controls, garden filter |
| **Charts** | `/charts.html` | Historical sensor data visualizations (24h to 30d) |
| **Devices** | `/devices.html` | Device list, assign crops/gardens, view claim codes |
| **Crops** | `/crops.html` | Crop CRUD with growth stage editor |
| **Automation** | `/automation.html` | Automation rule management |
| **Health** | `/health.html` | System health check results |
| **Users** | `/users.html` | User management (admin only) |
| **Login** | `/login.html` | User authentication |
| **Register** | `/register.html` | New user registration |

### Manual Controls

Toggle switches for: Light, Fan, Roof (mái che), FloatSwitch (phao điện tử), Pump (bơm nước).

---

## Configuration

All settings are in `appsettings.json` with environment variable overrides (via `__` separator).

### Required Settings (must be configured for production)

| Key | Environment Variable | Description |
|-----|---------------------|-------------|
| `JwtSettings:SecretKey` | `JwtSettings__SecretKey` | JWT signing key (min 32 chars) |
| `Provisioning:SharedKey` | `Provisioning__SharedKey` | Device self-registration shared secret (min 8 chars) |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQL Server connection string |

### Optional But Useful

| Key | Default | Description |
|-----|---------|-------------|
| `Redis:Configuration` | `""` | Redis connection string (empty = in-memory cache) |
| `OpenTelemetry:Otlp:Endpoint` | `""` | OTLP exporter endpoint for traces/metrics |
| `EmailSettings:Enabled` | `false` | Enable SMTP email notifications |
| `AISuggestions:Enabled` | `false` | Enable GPT-4o-mini analysis |
| `PerformanceBudgets:DashboardLatestP95Ms` | `300` | Dashboard P95 latency budget (ms) |
| `PerformanceBudgets:SensorIngestP95Ms` | `150` | Sensor ingestion P95 latency budget (ms) |
| `MqttSettings:EnableTls` | `false` | Enable MQTT TLS |
| `MqttSettings:ServerCertificatePath` | `""` | Path to PFX certificate for MQTT TLS |

---

## Observability

### Health Checks
- `GET /health/live` — always healthy if app is running
- `GET /health/ready` — checks database + MQTT broker connectivity

### OpenTelemetry
- **Tracing**: ASP.NET Core + HTTP client, sampled (default 25%)
- **Metrics**: ASP.NET Core hosting, Kestrel, HTTP client, runtime, custom performance budgets
- **Export**: OTLP (compatible with Grafana, Jaeger, Datadog, etc.)

### Performance Budgets
The middleware tracks latency for two critical endpoints:
- `GET /api/dashboard/latest` — budget `DashboardLatestP95Ms` (default 300ms)
- `POST /api/sensor` — budget `SensorIngestP95Ms` (default 150ms)

Exceeding budget → metric recorded + warning log.

### Grafana Dashboard
Pre-built dashboard JSON at `docs/observability/grafana-aeroponiciot-performance.json`.

---

## Firmware

An ESP32/ESP8266 Arduino sketch is available at `firmware/esp32_self_register_example.ino`:

- WiFi connectivity
- Self-registration with shared key
- Claim-code provisioning flow
- MQTT sensor publishing
- DHT22 temperature/humidity, BH1750 light sensor support
- Analog pH and TDS/EC sensor calibration

---

## Project Structure

```
├── Controllers/          # 13 API controllers
├── Data/                 # EF Core DbContext
├── DTOs/                 # Request/response models
├── Exceptions/           # Custom exception types
├── Middleware/           # 4 custom middleware components
├── Migrations/           # EF Core migrations (9 migrations)
├── Models/               # 10 entity models
├── Options/              # 8 strongly-typed config options
├── Services/             # 7 service categories
│   ├── AI/               # OpenAI-compatible suggestion engine
│   ├── Automation/       # Rules evaluation engine
│   ├── Maintenance/      # Log retention cleanup
│   ├── Mqtt/            # MQTT broker + Zigbee bridge
│   ├── Notifications/   # In-app + email notifications
│   ├── Security/        # Auth, onboarding protection
│   └── Sensors/         # Sensor data ingestion pipeline
├── firmware/             # ESP32/ESP8266 Arduino example
├── wwwroot/              # Web dashboard (9 HTML pages, 7 CSS, 9 JS)
├── tests/                # xUnit integration tests (20 test files)
├── docs/                 # API reference, API samples (.http), tech stack, system structure, business logic, Grafana dashboard
```

---

## Documentation

| Document | Description |
|----------|-------------|
| `docs/api-reference.md` | Full API endpoint reference with request/response examples |
| `docs/tech-stack.md` | Technology stack overview (Vietnamese) |
| `docs/system-structure.md` | Comprehensive system architecture, data model, configuration |
| `docs/nghiep-vu-va-luong-chay-he-thong.md` | Business logic & system flow explanation (Vietnamese) |
| `docs/observability/grafana-aeroponiciot-performance.json` | Grafana dashboard JSON |

---

## License

MIT
