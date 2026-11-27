# Task Management Tool

Cohort 10 — .NET Fullstack (.NET + ReactJS) assignment for Ali Hamza.

A task management app where users sign up, create and assign tasks with priorities,
categories and due dates, and track them on a dashboard. Admins see every task;
regular users see the tasks assigned to or created by them.

> **Status:** initial scaffold. Auth (register/login with JWT), the database schema,
> logging, error handling and read-only task endpoints are in place. Task create/update/delete,
> assignment and the full UI come in follow-up feature branches.

## Tech stack

| Layer     | Tech |
|-----------|------|
| Backend   | ASP.NET Core Web API (.NET 10), EF Core 10 + SQL Server, Serilog, JWT bearer auth |
| Frontend  | React 19 + Vite, React Router |
| Tests     | xUnit, EF Core InMemory provider |
| Quality   | SonarQube, CodeRabbit (PR reviews) |

## Repository layout

```
backend/
  TaskManagement.sln
  dotnet-tools.json                  # local dotnet-ef tool
  src/TaskManagement.Api/
    Auth/                            # JWT settings, token service, role names
    Controllers/                     # AuthController, TasksController
    Data/                            # AppDbContext + EF Core migrations
    Dtos/
    Entities/                        # User, TaskItem, Category, enums
    Exceptions/                      # AppException types mapped to HTTP status codes
    Middleware/                      # Global exception handling (ProblemDetails)
    Services/                        # AuthService
  tests/TaskManagement.Api.Tests/    # xUnit tests
frontend/                            # React (Vite) client
sonar-project.properties
.coderabbit.yaml
```

## Domain model

- **User**: full name, unique email, hashed password, role (`Admin` or `User`).
- **TaskItem**: title, description, priority (`Low`/`Medium`/`High`),
  status (`Pending`/`InProgress`/`Completed`), due date, category, assignee, creator.
- **Category**: name and description. `Work`, `Personal` and `Learning` are seeded.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server: LocalDB (installed with Visual Studio), SQL Server Express/Developer, or Docker
- [Node.js 20.19+ or 22.12+](https://nodejs.org/) and npm

## Running the backend

```bash
cd backend
dotnet tool restore          # installs dotnet-ef locally
dotnet restore
```

### 1. Configure the database

The default connection string in `src/TaskManagement.Api/appsettings.json` uses LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True
```

Not on Windows? Run SQL Server in Docker and override the connection string with user secrets:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

cd src/TaskManagement.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=<YourStrong!Passw0rd>;TrustServerCertificate=True"
```

### 2. Configure the JWT signing key

`appsettings.Development.json` contains a **development-only** key so the app runs out of the box.
For any other environment, set a random key of at least 32 characters through user secrets or an
environment variable (`Jwt__Key`), never in a committed file:

```bash
dotnet user-secrets set "Jwt:Key" "<at-least-32-random-characters>"
```

The app refuses to start if the key is missing or too short.

### 3. Create the database and run

```bash
# from backend/
dotnet ef database update -p src/TaskManagement.Api
dotnet run --project src/TaskManagement.Api
```

The API listens on `http://localhost:5014`. In Development the OpenAPI document is at
`http://localhost:5014/openapi/v1.json`, and `src/TaskManagement.Api/TaskManagement.Api.http`
has ready-made requests (VS Code REST Client / Visual Studio / Rider).

### Endpoints so far

| Method | Route                 | Auth            | Description |
|--------|-----------------------|-----------------|-------------|
| GET    | `/health`             | anonymous       | Health check |
| POST   | `/api/auth/register`  | anonymous       | Create an account (role `User`), returns a JWT |
| POST   | `/api/auth/login`     | anonymous       | Log in, returns a JWT |
| GET    | `/api/tasks`          | bearer token    | Tasks visible to the caller (admins: all) |
| GET    | `/api/tasks/summary`  | bearer token    | Pending / in-progress / completed counts for the dashboard |

Errors are returned as RFC 7807 `application/problem+json` by `ExceptionHandlingMiddleware`.

### Making a user an admin

Sign-ups always get the `User` role. Until an admin-management endpoint exists, promote a user in SQL:

```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'you@example.com';
```

Log in again afterwards so the new role is in the token.

### Adding a migration

```bash
dotnet ef migrations add <Name> -p src/TaskManagement.Api -o Data/Migrations
```

### Logging

Serilog is configured from the `Serilog` section of `appsettings.json`. Logs go to the console and to
rolling daily files in `src/TaskManagement.Api/logs/` (git-ignored). Each HTTP request is logged once
via `UseSerilogRequestLogging`.

## Running the tests

```bash
cd backend
dotnet test
```

## Running the frontend

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite dev server proxies `/api/*` to `http://localhost:5014`,
so start the backend first. To call a different API, copy `.env.example` to `.env.local` and set
`VITE_API_BASE_URL` (the API's CORS list is under `Cors:AllowedOrigins` in `appsettings.json`).

| Route         | Page |
|---------------|------|
| `/login`      | Log in |
| `/signup`     | Sign up |
| `/dashboard`  | Task counts by status (admins see all tasks) |
| `/tasks`      | Task list |
| `/tasks/new`  | New task (stub) |
| `/tasks/:id`  | Task detail (stub) |
| `/profile`    | User profile |

Other scripts: `npm run build`, `npm run lint`, `npm run preview`.

## Code quality

### SonarQube

`sonar-project.properties` is a placeholder; fill in the project key / host for your SonarQube
instance. For the .NET backend use the dotnet scanner:

```bash
dotnet tool install --global dotnet-sonarscanner
cd backend
dotnet sonarscanner begin /k:"cohort-10-dotnet-20926-ali" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="$SONAR_TOKEN"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

### CodeRabbit

`.coderabbit.yaml` configures automatic reviews on pull requests targeting `main` and `develop`.

## Git workflow

- `main`: stable, reviewed code only.
- Feature work happens on branches such as `feature/<short-name>` and is merged through pull
  requests reviewed by CodeRabbit.
- Keep commits small and descriptive (for example `feat: add task CRUD endpoints`).

## Roadmap

- [x] Project scaffold, EF Core schema and initial migration
- [x] Serilog logging and global exception handling
- [x] JWT register/login with Admin/User roles
- [ ] Task CRUD with assignment, priorities, categories and due dates
- [ ] Category management and admin user management
- [ ] Full React dashboard, task list filters, task forms
- [ ] More unit and integration tests, SonarQube quality gate
