# SoftOne — Task Management API

A .NET 8 Minimal API backend for task management, built as a technical assessment project. Clean architecture, HTTP Basic Authentication, EF Core + SQL Server, and full unit test coverage.

---

## What this does

- Full **CRUD** for tasks (create, read, update, soft delete)
- Mark a task as **completed**
- **Filter** by completion status or priority
- **Sort** by created date or due date
- **HTTP Basic Authentication** on all task endpoints
- Soft delete — deleted tasks are hidden, not permanently removed
- Validation via **FluentValidation** before any business logic runs
- Interactive **Swagger UI** in development

---

## Tech stack

| Concern | Library / Tool |
|---------|----------------|
| Runtime | .NET 8 |
| API style | ASP.NET Core Minimal API |
| Database | SQL Server + EF Core 8 |
| Auth | HTTP Basic Auth + BCrypt (no JWT) |
| Validation | FluentValidation 11 |
| Docs | Swashbuckle / Swagger UI |
| Testing | xUnit + Moq + FluentAssertions |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A running SQL Server instance accessible on `localhost,1433`

---

## Getting started

### 1. Clone and restore

```bash
git clone https://github.com/AsiriZenith/softone-task-management-api.git
cd softone-task-management-api
dotnet restore
```

### 2. Configure the connection string

Open `src/SoftOne.Api/appsettings.json` and set your SQL Server credentials:

```json
"DefaultConnection": "Server=localhost,1433;Database=SoftOneDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
```

### 3. Apply the database migration

```bash
dotnet ef database update --project src/SoftOne.Api
```

This creates the **SoftOneDb** database with the `Tasks` table automatically.

### 4. Run the API

For local development with an Angular frontend on `http://localhost:4200`, use the **http** launch profile (no HTTPS redirect):

```bash
dotnet run --project src/SoftOne.Api --launch-profile http
```

The API starts at **http://localhost:5298**. Open Swagger UI: **http://localhost:5298/swagger**

For HTTPS in development (optional):

```bash
dotnet run --project src/SoftOne.Api --launch-profile https
```

---

## Frontend integration (Angular)

See **[docs/frontend-integration.md](docs/frontend-integration.md)** for a full copy-paste guide for the frontend team.

The API allows cross-origin requests from **`http://localhost:4200`** in development via CORS. Configure your Angular app to call:

```
http://localhost:5298
```

Send credentials on each request using HTTP Basic Auth (e.g. Angular `HttpClient` with an `Authorization` header). Preflight `OPTIONS` requests are allowed without authentication.

To add another origin, update `Cors:AllowedOrigins` in `src/SoftOne.Api/appsettings.json`.

---

## Authentication

All task endpoints require **HTTP Basic Authentication**. A hardcoded `admin` account is used for this assessment (credentials are configured in `SoftOne.Auth/AuthCredentials.cs`).

### How to authenticate

**Using curl:**

```bash
curl -u admin:<password> http://localhost:5298/api/tasks
```

**Using Swagger UI:**
Click the **Authorize** button (🔒) and enter the credentials.

**Using the `.http` file:**
Open `src/SoftOne.Api/SoftOne.Api.http` in VS Code or Rider — update the `@password` variable at the top.

> No tokens are issued. Credentials are sent with every request via the `Authorization: Basic` header.

---

## API endpoints

### Authentication

| Method | Route | Auth required | Description |
|--------|-------|:---:|-------------|
| POST | `/api/auth/login` | No | Validate credentials |

### Tasks

| Method | Route | Auth required | Description |
|--------|-------|:---:|-------------|
| GET | `/api/tasks` | Yes | List active tasks (paginated) |
| GET | `/api/tasks/{id}` | Yes | Get a task by ID |
| POST | `/api/tasks` | Yes | Create a task |
| PUT | `/api/tasks/{id}` | Yes | Update a task |
| PATCH | `/api/tasks/{id}/status` | Yes | Update task status |
| DELETE | `/api/tasks/{id}` | Yes | Soft delete a task |

### Query parameters for `GET /api/tasks`

| Parameter | Values | Default | Example |
|-----------|--------|---------|---------|
| `page` | positive integer | `1` | `?page=2` |
| `pageSize` | `1`–`50` | `5` | `?pageSize=10` |
| `isCompleted` | `true` / `false` | — | `?isCompleted=false` |
| `priority` | `Low` / `Medium` / `High` | — | `?priority=High` |
| `sortBy` | `createdAt` (default) / `dueDate` | `createdAt` | `?sortBy=dueDate` |
| `sortDirection` | `asc` (default) / `desc` | `asc` | `?sortDirection=desc` |

### Example requests

```bash
# Login (no auth required)
curl -X POST http://localhost:5298/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"<password>"}'

# Create a task
curl -u admin:<password> -X POST http://localhost:5298/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Finish report","priority":"High","dueDate":"2026-12-31"}'

# List high-priority tasks, newest first
curl -u admin:<password> \
  "http://localhost:5298/api/tasks?page=1&pageSize=10&priority=High&sortBy=createdAt&sortDirection=desc"

# Update task status
curl -u admin:<password> -X PATCH http://localhost:5298/api/tasks/1/status \
  -H "Content-Type: application/json" \
  -d '{"status":"Completed"}'

# Delete a task (soft delete)
curl -u admin:<password> -X DELETE http://localhost:5298/api/tasks/1
```

### Response shape

**Success (single task):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Finish report",
    "description": null,
    "status": "Todo",
    "priority": "High",
    "dueDate": "2026-12-31",
    "createdAt": "2026-05-23T10:00:00Z",
    "updatedAt": null
  }
}
```

**Success (paginated list):**
```json
{
  "success": true,
  "data": {
    "items": [ { "id": 1, "title": "...", "status": "Todo", "priority": "High" } ],
    "page": 1,
    "pageSize": 5,
    "totalCount": 12,
    "totalPages": 3
  }
}
```

**Error (validation):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Title is required.", "Due date cannot be in the past."]
}
```

---

## Project structure

```
SoftOne/
├── src/
│   ├── SoftOne.Api/            # Main Web API project
│   │   ├── DTOs/               # Request & response contracts
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Data/               # EF Core — entities, configs, migrations
│   │   │   ├── Entities/
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Endpoints/          # Minimal API route handlers
│   │   ├── Extensions/         # DI, Swagger, DB registration
│   │   ├── Middleware/         # Auth + global exception handling
│   │   ├── Services/           # Business logic (TaskService)
│   │   ├── Validators/         # FluentValidation validators
│   │   └── Program.cs
│   └── SoftOne.Auth/           # Authentication library
│       ├── Helpers/            # PasswordHasher (BCrypt)
│       ├── Interfaces/         # IAuthService
│       ├── Models/             # AuthResult, LoginRequest
│       └── Services/           # AuthService
└── tests/
    ├── SoftOne.Api.Tests/      # Middleware, service, validator, endpoint tests
    └── SoftOne.Auth.Tests/     # BCrypt & credential validation tests
```

---

## Running the tests

```bash
dotnet test SoftOne.sln
```

| Project | Tests | Covers |
|---------|------:|--------|
| SoftOne.Api.Tests | 43 | Middleware, validators, services, endpoints |
| SoftOne.Auth.Tests | 11 | PasswordHasher, AuthService credentials |
| **Total** | **54** | |

Run a specific layer:

```bash
# Validators only
dotnet test --filter "FullyQualifiedName~Validators"

# Auth library only
dotnet test tests/SoftOne.Auth.Tests
```

---

## Database migrations

```bash
# Apply pending migrations
dotnet ef database update --project src/SoftOne.Api

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/SoftOne.Api \
  --output-dir Data/Migrations

# Remove the last migration (if not yet applied)
dotnet ef migrations remove --project src/SoftOne.Api
```

---

## Stopping the API before rebuilding

The API locks DLLs while it is running. Always stop it before running `dotnet build`:

- **Terminal:** press `Ctrl+C`
- **Visual Studio:** press `Shift+F5`

---
