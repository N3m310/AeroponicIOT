# AeroponicIOT — System Structure

> **Last updated:** 2026-06-27

Comprehensive reference for the AeroponicIOT smart farming system architecture, data model, API surface, frontend structure, configuration, deployment, and firmware.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Directory Map](#2-directory-map)
3. [Data Model (Entity Relationship)](#3-data-model-entity-relationship)
4. [Database Schema](#4-database-schema)
5. [API Controllers & Endpoints](#5-api-controllers--endpoints)
6. [Services Layer](#6-services-layer)
7. [Middleware Pipeline](#7-middleware-pipeline)
8. [Frontend (Dashboard)](#8-frontend-dashboard)
9. [Configuration Options](#9-configuration-options)
10. [DTOs (Data Transfer Objects)](#10-dtos-data-transfer-objects)
11. [Background Services](#11-background-services)
12. [Firmware](#12-firmware)
13. [DevOps & CI/CD](#13-devops--cicd)
14. [Authorization Matrix](#14-authorization-matrix)
15. [Health Checks & Observability](#15-health-checks--observability)

---

## 1. Project Overview

**AeroponicIOT** is a full-stack IoT platform for monitoring and controlling aeroponic farming environments. It connects ESP32/ESP8266 sensor nodes (and optionally Zigbee devices) to a cloud-like backend via MQTT and HTTP, providing real-time dashboards, automation, AI-powered suggestions, and alerting.

**Core capabilities:**
- Real-time sensor data ingestion (pH, TDS, temperature, humidity, light) via MQTT or HTTP
- Device self-registration and provisioning with claim-code flow
- Crop management with growth-stage-specific environmental thresholds
- Automatic actuator control (pumps, fans, lights, heaters) via schedule and threshold rules
- Manual actuator override from the dashboard
- AI-powered analysis and suggestions (OpenAI-compatible API)
- Multi-channel notifications (in-app + email via SMTP)
- Role-based access control (Farmer, Administrator)
- Full observability with OpenTelemetry metrics, traces, and structured logging

**Technology stack:**
- **Runtime:** .NET 8 (C#), ASP.NET Core
- **ORM:** Entity Framework Core 8, SQL Server 2022
- **Cache:** Redis 7 (optional, falls back to `IDistributedMemoryCache`)
- **IoT Messaging:** MQTTnet (built-in MQTT broker on port 1883)
- **Frontend:** Vanilla HTML5 / CSS3 / JavaScript, Chart.js
- **Auth:** JWT Bearer tokens, BCrypt password hashing
- **Email:** MailKit (SMTP)
- **AI:** OpenAI-compatible API (configurable endpoint/model)
- **Observability:** OpenTelemetry (OTLP exporter), OpenMetrics
- **Testing:** xUnit
- **Containerization:** Docker, Docker Compose (app, SQL Server, Redis, Zigbee2MQTT)
- **CI:** GitHub Actions

---

## 2. Directory Map

```
AeroponicIOT/
├── .github/workflows/          # GitHub Actions CI
│   └── ci.yml
├── Controllers/                # API controllers (13 controllers)
│   ├── ActuatorController.cs
│   ├── AISuggestionController.cs
│   ├── AuthenticationController.cs
│   ├── AutomationController.cs
│   ├── CropController.cs
│   ├── DashboardController.cs
│   ├── DebugController.cs
│   ├── DeviceController.cs
│   ├── GardenController.cs
│   ├── MqttController.cs
│   ├── NotificationController.cs
│   ├── ProblemResponseFactory.cs
│   ├── SensorController.cs
│   └── UsersController.cs
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
├── DTOs/                        # Request/response models
│   ├── ActuatorControlDto.cs
│   ├── ActuatorLogDto.cs
│   ├── ApiResponse.cs
│   ├── AuthDto.cs               # LoginRequest, RegisterRequest, TokenResponse, UserInfoDto
│   ├── AutomationRuleDtos.cs    # CreateAutomationRuleDto, UpdateAutomationRuleDto
│   ├── CropDto.cs               # CropUpsertDto, CropStageUpsertDto
│   ├── DashboardDto.cs          # DeviceStatusDto, DashboardDto
│   ├── DeviceDto.cs             # DeviceDto, CreateDeviceDto, UpdateDeviceDto, etc.
│   ├── GardenDto.cs
│   ├── SensorDataDto.cs
│   └── UserAdminDto.cs          # UserAdminListItemDto, UpdateUserAdminRequest
├── Exceptions/
│   ├── DomainValidationException.cs
│   └── ResourceNotFoundException.cs
├── Middleware/
│   ├── CorrelationIdMiddleware.cs
│   ├── GlobalExceptionHandlingMiddleware.cs
│   ├── PerformanceBudgetMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Migrations/                  # EF Core migrations (8 migrations)
│   ├── 20260225071153_InitialCreate.cs
│   ├── 20260227033528_AddGardensAndLightSupport.cs
│   ├── 20260302031406_AddLogIndexes.cs
│   ├── 20260316031951_AddCropAssignedAt.cs
│   ├── 20260316035304_AddDeviceProvisioningFlow.cs
│   ├── 20260323025623_AddUniqueConstraintsAndFixDeviceUserFk.cs
│   ├── 20260323031135_ConfigureDecimalPrecisionWarnings.cs
│   ├── 20260325030105_AddSensorRawPrecisionFields.cs
│   ├── 20260327023000_AddDeviceProtocolType.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── Models/                      # Entity models (10 entities)
│   ├── ActuatorLog.cs
│   ├── Alert.cs
│   ├── AutomationRule.cs
│   ├── Crop.cs
│   ├── CropStage.cs
│   ├── Device.cs
│   ├── DeviceStatus.cs          # DeviceLifecycleStatus enum + DeviceStatusValues
│   ├── Garden.cs
│   ├── Notification.cs
│   ├── SensorLog.cs
│   └── User.cs
├── Options/                     # Strongly-typed configuration options
│   ├── AppUrlsOptions.cs
│   ├── CorsOptions.cs
│   ├── EmailSettingsOptions.cs
│   ├── JwtSettingsOptions.cs
│   ├── MqttSettingsOptions.cs
│   ├── OnboardingProtectionOptions.cs
│   ├── PerformanceBudgetOptions.cs
│   └── ProvisioningOptions.cs
├── Services/
│   ├── AI/
│   │   ├── AIAnalysisBackgroundService.cs
│   │   ├── AIOptions.cs
│   │   ├── AISuggestionService.cs
│   │   └── IAISuggestionService.cs
│   ├── Automation/
│   │   └── AutomationBackgroundService.cs
│   ├── Maintenance/
│   │   └── LogRetentionBackgroundService.cs
│   ├── Mqtt/
│   │   ├── IMqttService.cs
│   │   ├── MqttHealthCheck.cs
│   │   ├── MqttService.cs
│   │   └── ZigbeePayloadMapper.cs
│   ├── Notifications/
│   │   ├── EmailService.cs
│   │   ├── IEmailService.cs
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   ├── Security/
│   │   ├── DeviceIdentityNormalizer.cs
│   │   ├── DistributedOnboardingAttemptTracker.cs
│   │   ├── HttpCurrentUserService.cs
│   │   ├── ICurrentUserService.cs
│   │   └── IOnboardingAttemptTracker.cs
│   └── Sensors/
│       ├── ISensorIngestionService.cs
│       ├── SensorIngestionService.cs
│       └── ThresholdChecker.cs
├── firmware/
│   └── esp32_self_register_example.ino   # ESP32/ESP8266 firmware
├── wwwroot/                     # Static dashboard files
│   ├── index.html               # Main dashboard (Vietnamese)
│   ├── login.html               # Login page
│   ├── register.html            # Registration page
│   ├── devices.html             # Device management
│   ├── crops.html               # Crop management
│   ├── automation.html          # Automation rules
│   ├── charts.html              # Analytics & charts
│   ├── health.html              # System health
│   ├── users.html               # User admin (admin only)
│   ├── css/
│   │   ├── dashboard.css
│   │   ├── automation.css
│   │   ├── charts.css
│   │   ├── crops.css
│   │   ├── devices.css
│   │   ├── register.css
│   │   └── users.css
│   └── js/
│       ├── auth.js              # Auth helpers (shared)
│       ├── config.js            # API base URL config
│       ├── dashboard.js         # Main dashboard logic
│       ├── devices.js           # Device management
│       ├── crops.js             # Crop management
│       ├── automation.js        # Automation rules
│       ├── charts.js            # Chart.js analytics
│       ├── health.js            # System health page
│       └── users.js             # User admin
├── tests/
│   └── AeroponicIOT.Tests/      # xUnit test project
├── docs/
│   ├── api-reference.md         # API documentation
│   ├── tech-stack.md            # Technology stack (Vietnamese)
│   ├── system-structure.md      # THIS FILE
│   └── observability/
│       └── grafana-aeroponiciot-performance.json  # Grafana dashboard
├── Program.cs                   # Application entry point
├── AeroponicIOT.csproj          # Project file (.NET 8)
├── AeroponicIOT.sln             # Solution file
├── Dockerfile                   # Docker image build
├── docker-compose.yml           # Multi-service deployment
├── appsettings.json             # Base configuration
├── appsettings.Development.json # Development overrides
└── README.md                    # Project README
```

---

## 3. Data Model (Entity Relationship)

```
┌───────────┐       ┌──────────────┐       ┌───────────────┐
│   User    │──1:N──│   Device     │──1:N──│  SensorLog    │
└───────────┘       │              │       └───────────────┘
                    │              │──1:N──┐
┌───────────┐       │              │       │  ┌───────────────┐
│   Crop    │──1:N──│              │       └──│  ActuatorLog  │
└───────────┘       │              │          └───────────────┘
       │            │              │──1:N──┐
       │1:N         └──────────────┘       │  ┌───────────────┐
┌───────────┐              │               └──│ AutomationRule│
│ CropStage │              │                  └───────────────┘
└───────────┘              │1:N
                    ┌───────────────┐
                    │    Garden     │
                    └───────────────┘

┌───────────┐
│   Alert   │──N:1──Device (optional)
└───────────┘

┌──────────────┐
│ Notification │──N:1──User
└──────────────┘
```

**Relationships:**
- **User** 1:N **Device** — A user owns multiple devices
- **Crop** 1:N **CropStage** — A crop has multiple growth stages
- **Crop** 1:N **Device** — A crop can be assigned to multiple devices
- **Garden** 1:N **Device** — A garden contains multiple devices
- **Device** 1:N **SensorLog** — A device produces many sensor readings
- **Device** 1:N **ActuatorLog** — A device records many actuator commands
- **Device** 1:N **AutomationRule** — A device can have many automation rules
- **Device** 1:N **Alert** — A device can trigger alerts
- **User** 1:N **Notification** — A user receives notifications

**Delete behavior:** Cascade for child logs/rules; SetNull for optional references (Crop, Garden, User on Device).

---

## 4. Database Schema

### Table: `users`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `username` | nvarchar(100) | Unique, nullable |
| `email` | nvarchar(256) | Unique, nullable |
| `password_hash` | nvarchar(max) | nullable |
| `role` | nvarchar(50) | nullable (Farmer, Administrator) |
| `created_at` | datetime2 | nullable |
| `last_login` | datetime2 | nullable |

### Table: `devices`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `device_name` | nvarchar(100) | nullable |
| `mac_address` | nvarchar(50) | **Unique, required** |
| `current_crop_id` | int | FK → crops.id, nullable |
| `garden_id` | int | FK → gardens.id, nullable |
| `user_id` | int | FK → users.id, nullable |
| `status` | nvarchar(20) | nullable (Pending, Active, Online, Offline, Inactive) |
| `created_at` | datetime2 | nullable |
| `last_seen` | datetime2 | nullable |
| `crop_assigned_at` | datetime2 | nullable |
| `chip_id` | nvarchar(100) | nullable |
| `firmware_version` | nvarchar(50) | nullable |
| `provisioned_at` | datetime2 | nullable |
| `claim_code` | nvarchar(16) | Unique, nullable |
| `claim_code_expires_at` | datetime2 | nullable |
| `protocol_type` | nvarchar(20) | nullable ("wifi" or "zigbee") |

### Table: `sensor_logs`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `device_id` | int | FK → devices.id, required |
| `timestamp` | datetime2 | required |
| `ph` | decimal(18,2) | nullable |
| `tds_ppm` | int | nullable |
| `tds_raw` | decimal(18,2) | nullable |
| `water_temp` | int | nullable |
| `water_temp_raw` | decimal(18,2) | nullable |
| `humidity` | int | nullable |
| `humidity_raw` | decimal(18,2) | nullable |
| `light_intensity` | int | nullable |
| `light_intensity_raw` | decimal(18,2) | nullable |
| Index: `(device_id, timestamp)` |

### Table: `actuator_logs`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `device_id` | int | FK → devices.id, required |
| `timestamp` | datetime2 | required |
| `actuator_type` | nvarchar(50) | nullable |
| `action` | nvarchar(10) | nullable (ON, OFF) |
| `duration_minutes` | int | nullable |
| Index: `(device_id, timestamp)` |

### Table: `crops`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `name` | nvarchar(100) | required |
| `description` | nvarchar(max) | nullable |
| `total_days_est` | int | nullable |
| `created_at` | datetime2 | nullable |

### Table: `crop_stages`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `crop_id` | int | FK → crops.id, required, Cascade |
| `stage_name` | nvarchar(100) | nullable |
| `day_start` | int | nullable |
| `day_end` | int | nullable |
| `ph_min` | decimal(18,2) | nullable |
| `ph_max` | decimal(18,2) | nullable |
| `ppm_min` | int | nullable |
| `ppm_max` | int | nullable |
| `water_temp_min` | int | nullable |
| `water_temp_max` | int | nullable |
| `humidity_min` | int | nullable |
| `humidity_max` | int | nullable |
| `pump_on_minutes` | int | nullable |
| `pump_off_minutes` | int | nullable |

### Table: `gardens`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `name` | nvarchar(100) | required |
| `location` | nvarchar(200) | nullable |
| `description` | nvarchar(max) | nullable |
| `created_at` | datetime2 | nullable |

### Table: `automation_rules`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `device_id` | int | FK → devices.id, required, Cascade |
| `rule_name` | nvarchar(100) | required |
| `rule_type` | int | required (0=Schedule, 1=Threshold, 2=Time-based) |
| `actuator_type` | int | required (0=Pump, 1=Fan, 2=Light, 3=Heater) |
| `action` | nvarchar(10) | required (ON, OFF, PULSE) |
| `condition_parameter` | nvarchar(50) | nullable (pH, TDS, Temperature, Humidity) |
| `condition_value` | decimal(18,2) | nullable |
| `condition_operator` | nvarchar(10) | nullable (>, <, ==, >=, <=) |
| `schedule_time` | time | nullable |
| `schedule_days` | nvarchar(50) | nullable (comma-separated day names) |
| `duration_minutes` | int | nullable |
| `is_active` | bit | required |
| `priority` | int | required (1-10) |
| `created_at` | datetime2 | required |
| `last_executed` | datetime2 | nullable |

### Table: `alerts`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `device_id` | int | FK → devices.id, nullable, SetNull |
| `timestamp` | datetime2 | required |
| `alert_type` | nvarchar(50) | nullable |
| `message` | nvarchar(500) | nullable |
| `severity` | nvarchar(20) | nullable |
| `is_resolved` | bit | required |

### Table: `notifications`
| Column | Type | Constraints |
|--------|------|------------|
| `id` | int | PK, identity |
| `user_id` | int | FK → users.id, nullable |
| `title` | nvarchar(200) | nullable |
| `message` | nvarchar(max) | nullable |
| `type` | int | required (0=Info, 1=Warning, 2=Error, 3=Alert) |
| `is_read` | bit | required |
| `created_at` | datetime2 | required |
| `read_at` | datetime2 | nullable |

---

## 5. API Controllers & Endpoints

### 5.1 Authentication — `POST /api/authentication`
| Endpoint | Auth | Rate Limit | Description |
|----------|------|-----------|-------------|
| `POST /register` | Anonymous | `auth` (10 req/60s) | Register a new user |
| `POST /login` | Anonymous | `auth` (10 req/60s) | Login, get JWT token |
| `GET /me` | `[Authorize]` | — | Get current user info |

**Response format:** All endpoints return `ApiResponse<T>` envelope `{ success, message, data, timestamp }`.  
**Error format:** RFC 7231 Problem Details (`application/problem+json`).  
**Legacy response:** `?legacyAuthResponse=true` query param returns bare `TokenResponse`.

### 5.2 Actuator — `api/actuator`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /control` | `[Authorize]` | Send ON/OFF command to device actuator via MQTT |
| `GET /logs/{deviceId}` | `[Authorize]` | Get actuator logs (query: `days`, default 7, max 90) |

### 5.3 AI Suggestion — `api/aisuggestion`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /analyze/{deviceId}` | `[Authorize(Policy = "AdminOnly")]` | Manually trigger AI analysis for a device |

### 5.4 Automation — `api/automation`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /rules` | `[Authorize]` | Get all rules (user-scoped or admin-all) |
| `GET /rules/{id}` | `[Authorize]` | Get rule by ID |
| `POST /rules` | `[Authorize]` | Create a new rule |
| `PUT /rules/{id}` | `[Authorize]` | Update a rule |
| `DELETE /rules/{id}` | `[Authorize]` | Delete a rule |
| `PUT /rules/{id}/toggle` | `[Authorize]` | Toggle rule active/inactive |
| `GET /rules/device/{deviceId}` | `[Authorize]` | Get rules for a specific device |

### 5.5 Crop — `api/crop`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /` | `[Authorize]` | Get all crops |
| `GET /{id}` | `[Authorize]` | Get crop by ID (includes stages, device count) |
| `POST /` | `[Authorize]` | Create a new crop with stages |
| `PUT /{id}` | `[Authorize]` | Update a crop |
| `DELETE /{id}` | `[Authorize]` | Delete a crop (fails if devices assigned) |

### 5.6 Dashboard — `api/dashboard`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /latest` | `[Authorize]` | Paginated devices with latest sensor data + active alerts |
| `GET /kpi` | `[Authorize]` | System-wide KPIs (avg pH/TDS/temp, health %, alert counts) |

### 5.7 Debug — `api/debug`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /create-test-user` | Development only | Create `devuser`/`P@ssw0rd1` (Administrator) |

### 5.8 Device — `api/device`
| Endpoint | Auth | Rate Limit | Description |
|----------|------|-----------|-------------|
| `GET /` | `[Authorize]` | — | Get user's devices (or all for admin) |
| `GET /pending` | `[Authorize(Policy = "AdminOnly")]` | — | Get unclaimed devices |
| `POST /self-register` | `[AllowAnonymous]` | `device-onboarding` (per-IP) | Device self-registration with provisioning key |
| `POST /claim` | `[Authorize]` | `device-onboarding` | Claim a device with claim code |
| `GET /{id}` | `[Authorize]` | — | Get device by ID |
| `POST /` | `[Authorize]` | — | Create device (admin provisioning) |
| `PUT /{id}` | `[Authorize]` | — | Update device |
| `DELETE /{id}` | `[Authorize]` | — | Delete device and all logs |

### 5.9 Garden — `api/garden`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /` | `[Authorize]` | Get all gardens (with device count) |
| `GET /{id}` | `[Authorize]` | Get garden by ID |
| `POST /` | `[Authorize]` | Create a garden |
| `PUT /{id}` | `[Authorize(Policy = "AdminOnly")]` | Update a garden |
| `DELETE /{id}` | `[Authorize(Policy = "AdminOnly")]` | Delete a garden (detaches devices) |
| `GET /{id}/devices` | `[Authorize]` | Get devices in a garden |

### 5.10 MQTT — `api/mqtt`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /status` | `[Authorize(Policy = "FarmerOrAdmin")]` | MQTT broker status + Zigbee bridge status |
| `POST /publish` | `[Authorize(Policy = "AdminOnly")]` | Publish raw MQTT message |

### 5.11 Notification — `api/notification`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /email-health` | `[Authorize(Policy = "AdminOnly")]` | Check email service health |
| `GET /unread` | `[Authorize]` | Get unread notifications for current user |
| `POST /{notificationId}/read` | `[Authorize]` | Mark notification as read |
| `DELETE /clear` | `[Authorize]` | Clear all notifications for current user |
| `POST /test-email` | `[Authorize(Policy = "AdminOnly")]` | Send test email |

### 5.12 Sensor — `api/sensor`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST /` | `[AllowAnonymous]` (with `X-Device-Key`) or `[Authorize]` | Receive sensor data from device |

**Processing pipeline:** Device lookup → LastSeen update → SensorLog creation → Threshold check → Alert generation → Alert notification → AI analysis (fire-and-forget).

### 5.13 Users — `api/users`
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /` | `[Authorize(Policy = "AdminOnly")]` | List all users |
| `GET /{id}` | `[Authorize(Policy = "AdminOnly")]` | Get user by ID |
| `PUT /{id}` | `[Authorize(Policy = "AdminOnly")]` | Update user role/email (cannot demote self) |
| `DELETE /{id}` | `[Authorize(Policy = "AdminOnly")]` | Delete user + notifications (cannot delete self) |

---

## 6. Services Layer

### 6.1 Sensor Ingestion (`Services/Sensors/`)
- **`ISensorIngestionService` / `SensorIngestionService`** — Centralized service for processing sensor data regardless of transport (HTTP, MQTT). Handles device lookup, data sanitization (pH 0-14, TDS 0-50000, temp -20-100, humidity 0-100), sensor log creation, threshold checking, alert generation, notification dispatch, and fire-and-forget AI analysis.
- **`ThresholdChecker`** — Compares sensor readings against crop stage optimal ranges, generates alerts for out-of-range values.

### 6.2 MQTT Broker (`Services/Mqtt/`)
- **`IMqttService` / `MqttService`** — Built-in MQTT broker using MQTTnet. Manages client connections, authentication (username/password, client certificates), topic ACL enforcement, Zigbee2MQTT bridge integration, and forwards device sensor data to `SensorIngestionService`.
- **`MqttHealthCheck`** — `IHealthCheck` implementation for MQTT broker readiness probe.
- **`ZigbeePayloadMapper`** — Translates Zigbee2MQTT ZCL payloads into standardized `SensorDataDto`.

### 6.3 AI Suggestions (`Services/AI/`)
- **`IAISuggestionService` / `AISuggestionService`** — Calls an OpenAI-compatible API with sensor data + crop context + growth stage ranges. Returns structured JSON suggestions (`{ title, message, type, suggested_action, confidence }`). Features cooldown per device, caching, configurable model/temperature/tokens. Delivers suggestions as notifications.
- **`AIAnalysisBackgroundService`** — Background service that runs every 15 minutes (when enabled) to analyze all recently-active devices for gradual trends.
- **`AIOptions`** — Configuration class for AI feature (`Enabled`, `Endpoint`, `ApiKey`, `Model`, `MaxTokens`, `Temperature`, `CooldownMinutes`, `SystemPromptOverride`, `ProxyUrl`).

### 6.4 Automation (`Services/Automation/`)
- **`AutomationBackgroundService`** — Runs every 60 seconds. Evaluates all active automation rules:
  - **Schedule rules** (type 0): Trigger based on `ScheduleTime` + `ScheduleDays`.
  - **Threshold rules** (type 1): Trigger when latest sensor reading crosses `ConditionValue` with `ConditionOperator`.
  - **Timer rules** (type 2): Trigger based on elapsed time since last execution.
  - Executes matched rules by publishing actuator commands via MQTT.

### 6.5 Notifications (`Services/Notifications/`)
- **`INotificationService` / `NotificationService`** — Creates in-app notifications and optionally sends email via SMTP. Supports `SendNotificationAsync` (general) and `SendAlertNotificationAsync` (alert-specific).
- **`IEmailService` / `EmailService`** — SMTP email sending via MailKit. Supports health checks with optional connectivity test.

### 6.6 Security (`Services/Security/`)
- **`ICurrentUserService` / `HttpCurrentUserService`** — Extracts current user context (UserId, Role) from the HTTP context's JWT claims.
- **`DeviceIdentityNormalizer`** — Normalizes MAC addresses (uppercase, colon-separated).
- **`IOnboardingAttemptTracker` / `DistributedOnboardingAttemptTracker`** — Tracks failed provisioning attempts per IP/client for rate limiting and cooldown, using `IDistributedCache`.

### 6.7 Maintenance (`Services/Maintenance/`)
- **`LogRetentionBackgroundService`** — Runs daily. Deletes sensor logs older than `DataRetention:SensorLogDays` (default 90) and actuator logs older than `DataRetention:ActuatorLogDays` (default 180).

---

## 7. Middleware Pipeline

The middleware pipeline is configured in `Program.cs` in this order:

```
CorrelationIdMiddleware
  ↓
RequestLoggingMiddleware
  ↓
PerformanceBudgetMiddleware
  ↓
GlobalExceptionHandlingMiddleware
  ↓
HttpsRedirection (skipped in Testing environment)
  ↓
Cors ("ConfiguredOrigins")
  ↓
RateLimiter
  ↓
Authentication
  ↓
Authorization
  ↓
StaticFiles
  ↓
HealthChecks (/health/live, /health/ready, /health)
  ↓
MapGet("/") → wwwroot/index.html
  ↓
MapControllers
```

### 7.1 CorrelationIdMiddleware
Reads `X-Correlation-ID` from request header or generates a new GUID. Sets it on the response header and in the logging scope. All logs within a request share the same correlation ID.

### 7.2 RequestLoggingMiddleware
Logs structured HTTP request data: Method, Path, StatusCode, ElapsedMs, CorrelationId, User, RemoteIp. Skips `/health`, `/favicon.ico`, `/css`, `/js`, `/lib` prefixes. Logs >=500 at Error level, others at Information.

### 7.3 PerformanceBudgetMiddleware
Tracks endpoint latency for two budgeted endpoints:
- `GET /api/dashboard/latest` — budget: `PerformanceBudgets:DashboardLatestP95Ms` (default 300ms)
- `POST /api/sensor` — budget: `PerformanceBudgets:SensorIngestP95Ms` (default 150ms)

Records duration in OpenTelemetry `Histogram` (`aeroponic.endpoint.duration.ms`) and counts violations in `Counter` (`aeroponic.endpoint.budget_violations`). Logs warnings when budget is exceeded.

### 7.4 GlobalExceptionHandlingMiddleware
Catches all unhandled exceptions and returns RFC 7231 Problem Details JSON. Maps exception types to HTTP status codes:
| Exception | Status | Error Code |
|-----------|--------|-----------|
| `DomainValidationException` | 400 | `bad_request` |
| `ResourceNotFoundException` | 404 | `not_found` |
| `ArgumentException` | 400 | `bad_request` |
| `KeyNotFoundException` | 404 | `not_found` |
| `UnauthorizedAccessException` | 401 | `unauthorized` |
| `DbUpdateException` | 409 | `conflict` |
| `InvalidOperationException` | 409 | `conflict` |
| Any other | 500 | `internal_error` |

---

## 8. Frontend (Dashboard)

The frontend is a vanilla HTML/CSS/JS single-page application served from `wwwroot/`. All pages share common authentication via `auth.js`.

### Page Structure

| Page | File | Description |
|------|------|-------------|
| **Dashboard** | `index.html` + `dashboard.js` | Main dashboard: KPI cards, device health grid, active alerts, manual actuator controls, garden management |
| **Login** | `login.html` | User login form |
| **Register** | `register.html` | User registration form |
| **Devices** | `devices.html` + `devices.js` | Device list, assign crops/gardens, claim codes |
| **Crops** | `crops.html` + `crops.js` | Crop CRUD with growth stage management |
| **Automation** | `automation.html` + `automation.js` | Automation rule creation and management |
| **Charts** | `charts.html` + `charts.js` | Historical sensor data visualization with Chart.js |
| **Health** | `health.html` + `health.js` | System health check results |
| **Users** | `users.html` + `users.js` | User management (admin only) |

### Key Dashboard Features (index.html)
- **Auto-refresh:** Polls `/api/dashboard/latest` every 10 seconds
- **Garden filter:** Pills UI for filtering by garden
- **Device health cards:** Color-coded by status (healthy/warning/critical/offline)
- **Manual controls:** Toggle switches for Light, Fan, Roof, FloatSwitch, Pump
- **Alerts panel:** Active alerts list
- **Navigation:** Buttons to all sub-pages
- **KPI cards:** Average temperature, humidity, light, pH, TDS

### Authentication Flow
1. User logs in via `login.html` → JWT token stored in `localStorage`
2. `auth.js` provides shared helpers: `getStoredToken()`, `getAuthHeaders()`, `saveAuthSession()`, `clearAuthStorage()`
3. Every page checks authentication on load; redirects to `login.html` if no valid token

---

## 9. Configuration Options

All configuration is in `appsettings.json` (base) and `appsettings.Development.json` (development overrides). Environment variables override settings via `__` separator (e.g., `JwtSettings__SecretKey`).

### `JwtSettings`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `SecretKey` | string | `""` | **Required.** JWT signing key (min 32 chars) |
| `ExpirationMinutes` | int | `1440` | Token lifetime (5-10080 min) |
| `Issuer` | string | `"AeroponicIOT"` | JWT issuer |
| `Audience` | string | `"AeroponicIOT"` | JWT audience |

### `MqttSettings`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Port` | int | `1883` | MQTT plaintext port |
| `Host` | string | `"localhost"` | MQTT bind address |
| `Username` | string | `""` | Admin username for client auth |
| `Password` | string | `""` | Admin password for client auth |
| `RequireClientAuthentication` | bool | `true` | Require MQTT clients to authenticate |
| `EnableTls` | bool | `false` | Enable TLS endpoint |
| `TlsPort` | int | `8883` | MQTT TLS port |
| `DisablePlaintextEndpoint` | bool | `false` | Disable port 1883 (TLS-only) |
| `ServerCertificatePath` | string | `""` | Path to PFX certificate for TLS |
| `ServerCertificatePassword` | string | `""` | PFX certificate password |
| `RequireClientCertificate` | bool | `false` | Require mTLS client certificates |
| `AllowedClientCertificateIssuers` | string[] | `[]` | Allowed client cert issuer DNs |
| `AllowedClientCertificateThumbprints` | string[] | `[]` | Allowed client cert thumbprints |
| `EnforceTopicAcl` | bool | `true` | Enforce topic access control |
| `DeviceTopicPrefix` | string | `"devices"` | Device topic prefix |
| `EnableZigbee2MqttBridge` | bool | `false` | Enable Zigbee bridge integration |
| `Zigbee2MqttTopicPrefix` | string | `"zigbee2mqtt"` | Zigbee2MQTT topic prefix |
| `EnforceZigbeeTopicAcl` | bool | `true` | Enforce ACL on Zigbee topics |
| `ZigbeeBridgeUsername` | string | `""` | Allowed bridge MQTT username |
| `ZigbeeBridgeClientId` | string | `""` | Allowed bridge client ID |

### `Provisioning`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `SharedKey` | string | `""` | **Required.** Shared secret for device self-registration |
| `ClaimCodeMinutes` | int | `10` | Claim code validity duration |

### `EmailSettings`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `false` | Enable email notifications |
| `SmtpHost` | string | `"smtp.gmail.com"` | SMTP server hostname |
| `SmtpPort` | int | `587` | SMTP port |
| `SmtpUsername` | string | `""` | SMTP auth username |
| `SmtpPassword` | string | `""` | SMTP auth password |
| `FromEmail` | string | `"noreply@smartfarmiot.com"` | From email address |
| `FromName` | string | `"Smart Farm IoT System"` | From display name |

### `AppUrls`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `DashboardBaseUrl` | string | `"http://localhost:5062"` | Base URL for dashboard links in emails |

### `OnboardingProtection`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `FailedAttemptThreshold` | int | `5` | Max failed attempts before cooldown |
| `FailedAttemptWindowSeconds` | int | `300` | Window for counting failed attempts |
| `FailedAttemptCooldownSeconds` | int | `120` | Cooldown duration after threshold hit |
| `StateTtlSeconds` | int | `900` | Distributed cache entry TTL |

### `PerformanceBudgets`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `DashboardLatestP95Ms` | int | `300` | P95 latency budget for dashboard endpoint (ms) |
| `SensorIngestP95Ms` | int | `150` | P95 latency budget for sensor ingest (ms) |

### `OpenTelemetry`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `ServiceName` | string | `"AeroponicIOT"` | OpenTelemetry service name |
| `Tracing:SampleRatio` | double | `0.25` | Trace sampling ratio (0.0-1.0) |
| `ExcludedPaths` | string[] | `["/health", "/health/live", "/health/ready"]` | Paths excluded from tracing |
| `Otlp:Endpoint` | string | `""` | OTLP exporter endpoint (e.g., Grafana, Jaeger) |

### `AISuggestions`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `false` | Enable AI suggestion feature |
| `Endpoint` | string | `"https://api.openai.com/v1/chat/completions"` | OpenAI-compatible API endpoint |
| `ApiKey` | string | `""` | API key |
| `Model` | string | `"gpt-4o-mini"` | Model name |
| `MaxTokens` | int | `500` | Max response tokens (50-4000) |
| `Temperature` | double | `0.3` | Response creativity (0.0-2.0) |
| `CooldownMinutes` | int | `60` | Min time between AI analyses per device (1-1440) |
| `SystemPromptOverride` | string | `""` | Custom system prompt (empty = default) |
| `ProxyUrl` | string | `""` | HTTP proxy for AI API calls |

### `Cors`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `AllowedOrigins` | string[] | `[]` | Allowed CORS origins (empty = development all-origins) |

### `Redis`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Configuration` | string | `""` | Redis connection string (empty = in-memory cache) |

### `Startup`
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `ApplyMigrationsOnStartup` | bool | `true` | Auto-apply EF Core migrations |
| `SeedDefaultCropsOnStartup` | bool | `true` | Seed Lettuce, Basil, Strawberry crops |
| `FailFastOnInitializationError` | bool | `true` (prod) | Throw on init failure (vs. continue) |

### `ConnectionStrings`
| Key | Description |
|-----|-------------|
| `DefaultConnection` | SQL Server connection string |

---

## 10. DTOs (Data Transfer Objects)

| DTO | File | Purpose |
|-----|------|---------|
| `ApiResponse<T>` | `ApiResponse.cs` | Standard API response envelope |
| `SensorDataDto` | `SensorDataDto.cs` | Sensor reading from device (MAC, pH, TDS, temp, humidity, light) |
| `DeviceDto`, `CreateDeviceDto`, `UpdateDeviceDto` | `DeviceDto.cs` | Device CRUD |
| `DeviceSelfRegisterRequestDto`, `DeviceSelfRegisterResponseDto` | `DeviceDto.cs` | Device provisioning |
| `ClaimDeviceRequestDto` | `DeviceDto.cs` | Device claiming |
| `PendingDeviceDto` | `DeviceDto.cs` | Unclaimed device list item |
| `DeviceStatusDto` | `DashboardDto.cs` | Device with latest sensor data for dashboard |
| `DashboardDto` | `DashboardDto.cs` | Dashboard aggregate data |
| `LoginRequest`, `RegisterRequest` | `AuthDto.cs` | Authentication requests |
| `TokenResponse`, `UserInfoDto` | `AuthDto.cs` | Authentication responses |
| `ActuatorControlDto` | `ActuatorControlDto.cs` | Actuator control command |
| `ActuatorLogDto` | `ActuatorLogDto.cs` | Actuator log entry |
| `CropUpsertDto`, `CropStageUpsertDto` | `CropDto.cs` | Crop create/update |
| `GardenDto` | `GardenDto.cs` | Garden with device count |
| `CreateAutomationRuleDto`, `UpdateAutomationRuleDto` | `AutomationRuleDtos.cs` | Automation rule CRUD |
| `UserAdminListItemDto`, `UpdateUserAdminRequest` | `UserAdminDto.cs` | User admin |

---

## 11. Background Services

| Service | File | Schedule | Description |
|---------|------|----------|-------------|
| **AutomationBackgroundService** | `Services/Automation/AutomationBackgroundService.cs` | Every 60 seconds | Evaluates and executes automation rules |
| **AIAnalysisBackgroundService** | `Services/AI/AIAnalysisBackgroundService.cs` | Every 15 minutes | Background AI analysis of active devices |
| **LogRetentionBackgroundService** | `Services/Maintenance/LogRetentionBackgroundService.cs` | Daily | Deletes old sensor/actuator logs |

---

## 12. Firmware

File: `firmware/esp32_self_register_example.ino`

An Arduino-compatible firmware sketch for ESP32 and ESP8266 microcontrollers.

**Capabilities:**
- WiFi connectivity
- Self-registration with backend provisioning key (`X-Device-Key`)
- Claim code flow
- MQTT communication after claim (`PubSubClient`)
- Sensor reading:
  - pH (analog)
  - TDS / EC (analog)
  - Temperature & humidity (DHT22)
  - Light intensity (BH1750)
- Configurable ADC calibration values
- Supports conditional compilation for ESP32 vs ESP8266

---

## 13. DevOps & CI/CD

### Docker Compose (`docker-compose.yml`)

| Service | Image | Ports | Description |
|---------|-------|-------|-------------|
| `app` | `n3mnuonghn/aeroponiciot:1.0.0` | 5062:80, 1883:1883 | ASP.NET Core API |
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433:1433 | SQL Server 2022 |
| `redis` | `redis:7-alpine` | 6379:6379 | Redis cache (profile: `redis`) |
| `zigbee2mqtt` | `koenkk/zigbee2mqtt:latest` | — | Zigbee2MQTT bridge (profile: `zigbee`) |

Start with profiles:
```bash
# Core stack
docker compose up -d

# With Redis
docker compose --profile redis up -d

# With Zigbee coordinator
docker compose --profile zigbee up -d
```

### Dockerfile
Multi-stage build targeting `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/aspnet:8.0`.

### GitHub Actions (`.github/workflows/ci.yml`)
```yaml
Trigger: push to main/master, any PR
Steps:
  1. Checkout
  2. Setup .NET 10.0
  3. dotnet restore
  4. dotnet build --configuration Release
  5. dotnet format --verify-no-changes --severity error
  6. dotnet test --configuration Release
  7. dotnet list package --vulnerable --include-transitive
```

---

## 14. Authorization Matrix

| Controller | Anonymous | `[Authorize]` | `FarmerOrAdmin` | `AdminOnly` |
|-----------|-----------|---------------|-----------------|-------------|
| Authentication | `register`, `login` | `me` | — | — |
| Actuator | — | `control`, `logs/{id}` | — | — |
| AI Suggestion | — | — | — | `analyze/{id}` |
| Automation | — | All CRUD | — | — |
| Crop | — | All CRUD | — | — |
| Dashboard | — | `latest`, `kpi` | — | — |
| Debug | `create-test-user` (dev only) | — | — | — |
| Device | `self-register` | `claim`, CRUD (own) | — | `pending` |
| Garden | — | `getAll`, `getById`, `create`, `getDevices` | — | `update`, `delete` |
| MQTT | — | — | `status` | `publish` |
| Notification | — | `unread`, `read`, `clear` | — | `email-health`, `test-email` |
| Sensor | `POST /` (with device key) | `POST /` (with JWT) | — | — |
| Users | — | — | — | All CRUD |

**Role hierarchy:**
- **Farmer:** Default role. Can manage own devices, crops, gardens, and automation rules.
- **Administrator:** Full system access. Can manage all users, devices, pending devices, and system configuration.

---

## 15. Health Checks & Observability

### Health Endpoints
| Endpoint | Tag | Description |
|----------|-----|-------------|
| `GET /health/live` | `live` | Liveness probe — always returns healthy if app is running |
| `GET /health/ready` | `ready` | Readiness probe — checks database + MQTT connectivity |
| `GET /health` | `ready` | Same as `/health/ready` |

**Response format:**
```json
{
  "status": "Healthy",
  "totalDurationMs": 12.34,
  "timestamp": "2026-06-27T00:00:00Z",
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "Application is running",
      "durationMs": 0.12,
      "error": null,
      "data": {}
    }
  ]
}
```

### OpenTelemetry
- **Tracing:** ASP.NET Core and HTTP client instrumentation. Sampled via `TraceIdRatioBasedSampler` (default 25%). Excluded paths: `/health`, `/health/live`, `/health/ready`.
- **Metrics:** ASP.NET Core hosting metrics, Kestrel, HTTP client, custom `PerformanceBudgetMiddleware` meter (`aeroponic.endpoint.duration.ms`, `aeroponic.endpoint.budget_violations`), runtime instrumentation.
- **Export:** OTLP exporter (configurable endpoint — Grafana, Jaeger, etc.).

### Grafana Dashboard
A pre-built Grafana dashboard for performance monitoring is available at:
`docs/observability/grafana-aeroponiciot-performance.json`

### Logging
- Structured JSON-style logging via `ILogger<T>`
- Correlation IDs propagated through all logs via `CorrelationIdMiddleware`
- Request logging middleware excludes static files and health endpoints
- LoggerMessage delegates for high-performance, strongly-typed logging

---

*Last updated: 2026-06-27*
