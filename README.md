# 🎬 Movie Reservation API

A REST API for managing movies, showtimes and seat reservations in a cinema. Built with ASP.NET Core 9, Entity Framework Core, PostgreSQL and Redis using a layered architecture.

It prevents the same seat from being sold twice and overlapping showtimes from being scheduled in the same hall at the database level. It also provides occupancy and revenue reports for administrators.

## Live Demo

| | URL |
|---|---|
| Swagger UI | https://demo.yasinozgurcakmak.com.tr/movie-reservation-api/swagger/index.html |
| API base URL | https://demo.yasinozgurcakmak.com.tr/movie-reservation-api |


Demo account:

| Email | Password |
|---|---|
| `demouser@gmail.com` | `DemoUser0!` |

**Try it in Swagger:**

1. Open Swagger UI and expand `POST /api/auth/login`.
2. Click **Try it out** and send:
   ```json
   {
     "email": "demouser@gmail.com",
     "password": "DemoUser0!"
   }
   ```
3. Copy the `accessToken` from the response.
4. Click **Authorize** at the top of the page, paste the token and confirm.
5. You can now call the protected endpoints.

**Or with curl:**

```bash
curl -X POST https://demo.yasinozgurcakmak.com.tr/movie-reservation-api/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demouser@gmail.com","password":"DemoUser0!"}'
```


## Table of Contents

- [Live Demo](#live-demo)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Quick Start (Docker)](#quick-start-docker)
- [Local Development](#local-development)
- [Swagger](#swagger)
- [Configuration](#configuration)
- [Endpoints](#endpoints)
- [Security](#security)
- [Tests](#tests)
- [License](#license)

## Features

### Authentication and users

- Registration/login with JWT; roles: `User`, `Editor`, `Admin`
- Email verification, forgot/reset password, change password
- Existing tokens are revoked immediately when a password or role changes (security stamp)

### Catalog

- Movie and genre management (many-to-many relationship, soft delete and restore)
- Hall management (rows × seats per row)
- Create, update and cancel showtimes. The end time is calculated automatically from the movie's duration.
- Movies and showtimes playing on a given date

### Reservations

- Seat map per showtime (`A1`…`Z50`) with availability
- Create and cancel reservations (only for showtimes that have not started)
- Concurrent requests for the same seat: a unique index lets exactly one succeed
- At most 10 seats per user per showtime
- Reservation confirmation and showtime cancellation emails (sent in the background)

### Administration

- Filterable, paginated list of all reservations
- Occupancy rate and revenue analytics by movie, hall, genre and time slot (JSON or CSV)
- Daily/weekly sales report
- Audit log: who changed what, and when

### Infrastructure

- Redis cache for public catalog listings. If Redis is unreachable, the API keeps working against the database.
- Automatic migrations on startup and one-time creation of the admin account
- `/health` endpoint (PostgreSQL + Redis)

## Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core 9 (Web API) |
| ORM | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL 15 |
| Cache | Redis 7 |
| Validation | FluentValidation |
| Authentication | JWT Bearer, BCrypt |
| Email | MailKit (SMTP) |
| Documentation | OpenAPI + Swagger UI |
| Testing | xUnit, WebApplicationFactory, Testcontainers |
| Containers | Docker, Docker Compose |

## Architecture

The project consists of five layers; dependencies point downwards:

```
MovieReservationAPI   → Thin controllers, global exception handler, JWT, rate limiting, Swagger
BusinessLayer         → Business rules (managers), FluentValidation, domain exceptions
DataAccessLayer       → AppDbContext, Fluent API configurations, migrations, repositories, seed
DtoLayer              → Request/response records
EntityLayer           → Pure domain entities and enums
```

Critical business rules are enforced by the database as well as by application code:

| Rule | Database enforcement |
|---|---|
| A seat can be sold only once per showtime | Partial unique index on non-cancelled seats |
| Showtimes in the same hall cannot overlap | `EXCLUDE` constraint with `btree_gist` |
| Movie/hall updates do not overwrite each other | Optimistic concurrency with `xmin` |

## Quick Start (Docker)

Requirement: Docker Desktop.

```bash
git clone https://github.com/yasinozgurcakmak/Movie-Reservation-Api.git
cd Movie-Reservation-Api
cp .env.example .env
```

Fill in the `.env` file. `JWT_SECRET_KEY` must be at least 32 bytes:

```bash
openssl rand -base64 64
```

Then run:

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Health | http://localhost:5000/health |

On first startup, migrations are applied and the `admin@cinema.com` account is created with the `ADMIN_PASSWORD` password. The account is looked up by email, so it is not created again on later startups.

## Local Development

Requirements: .NET 9 SDK, PostgreSQL and Redis. You can start the database and Redis with Docker:

```bash
docker compose up -d db redis
```

Secrets are not stored in `appsettings.json`; they are set with [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd MovieReservationAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MovieReservationDb;Username=postgres;Password=<password>"
dotnet user-secrets set "Jwt:SecretKey" "<random value of at least 32 bytes>"
dotnet user-secrets set "Admin:Password" "<admin password>"

# Optional: SMTP for sending emails
dotnet user-secrets set "Email:Username" "<smtp username>"
dotnet user-secrets set "Email:Password" "<smtp password>"
dotnet user-secrets set "Email:FromAddress" "<sender address>"

dotnet run --launch-profile https
```

## Swagger

Swagger UI is available in every environment:

| How you run it | Swagger UI | OpenAPI document |
|---|---|---|
| `dotnet run --launch-profile https` | https://localhost:7196/swagger | https://localhost:7196/openapi/v1.json |
| `dotnet run --launch-profile http` | http://localhost:5144/swagger | http://localhost:5144/openapi/v1.json |
| `docker compose up` | http://localhost:5000/swagger | http://localhost:5000/openapi/v1.json |

To try protected endpoints, get a token from `POST /api/auth/login` and paste it into Swagger's **Authorize** button.

## Configuration

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection **(secret)** |
| `ConnectionStrings:Redis` | Redis connection |
| `Jwt:SecretKey` | Token signing key, at least 32 bytes **(secret)** |
| `Jwt:ExpirationMinutes` | Token lifetime (default 60) |
| `Admin:Email` / `Admin:Password` | Initial admin account; the password is only used if the account does not exist **(secret)** |
| `Email:*` | SMTP settings **(secret)** |
| `PasswordReset:ResetUrl` | Frontend link in the password reset email |
| `EmailVerification:VerifyUrl` | Frontend link in the verification email |
| `RateLimiting:AuthPermitLimit` | Auth requests allowed per IP per minute (default 10) |
| `Cinema:TimeZoneId` | The cinema's time zone (default `Europe/Istanbul`) |

In Docker, the same keys are passed as environment variables using `__` (e.g. `Jwt__SecretKey`).

## Endpoints

🔓 public · 🔑 login required · ✏️ Admin/Editor · 🛡️ Admin only

### Auth — `/api/auth`

| Method | Path | Access | Description |
|---|---|---|---|
| POST | `/register` | 🔓 | Register; sends a verification email |
| POST | `/login` | 🔓 | Returns a JWT |
| POST | `/verify-email` | 🔓 | Verify email address |
| POST | `/resend-verification` | 🔓 | Resend the verification email |
| POST | `/forgot-password` | 🔓 | Send a password reset link |
| POST | `/reset-password` | 🔓 | Set a new password |

### Users — `/api/users`

| Method | Path | Access | Description |
|---|---|---|---|
| GET | `/me` | 🔑 | Profile |
| PUT | `/me` | 🔑 | Update profile |
| PUT | `/me/password` | 🔑 | Change password; signs out other sessions and returns a new token |
| GET | `/` | 🛡️ | All users |
| PATCH | `/{id}/role` | 🛡️ | Change role |

### Catalog

| Method | Path | Access | Description |
|---|---|---|---|
| GET | `/api/genres`, `/api/genres/{id}` | 🔓 | Genres |
| POST/PUT/DELETE | `/api/genres[/{id}]` | ✏️ | Manage genres |
| GET | `/api/halls`, `/api/halls/{id}` | 🔓 | Halls |
| POST/PUT/DELETE | `/api/halls[/{id}]` | ✏️ | Manage halls |
| GET | `/api/movies?genreId=&page=&pageSize=` | 🔓 | Paginated movie list |
| GET | `/api/movies/{id}` | 🔓 | Movie details |
| POST/PUT/DELETE | `/api/movies[/{id}]` | ✏️ | Manage movies (delete = soft delete) |
| PATCH | `/api/movies/{id}` | ✏️ | Delete/restore (`{"isDeleted": false}`) |

### Showtimes — `/api/showtimes`

| Method | Path | Access | Description |
|---|---|---|---|
| GET | `?date=YYYY-MM-DD` | 🔓 | Movies and showtimes for that day |
| GET | `/{id}` | 🔓 | Showtime details |
| GET | `/{id}/seats` | 🔓 | Seat map |
| POST | `/` | ✏️ | Create showtime (with overlap check) |
| PUT | `/{id}` | ✏️ | Update showtime (only if it has no reservations) |
| DELETE | `/{id}` | ✏️ | Cancel showtime; reservations are cancelled and customers are emailed |

### Reservations — `/api/reservations`

| Method | Path | Access | Description |
|---|---|---|---|
| POST | `/` | 🔑 | Create reservation (verified email required) |
| GET | `/my` | 🔑 | My reservations |
| PATCH | `/{id}` | 🔑 | Cancel (`{"status": "Cancelled"}`) |

### Administration — `/api/admin`

| Method | Path | Access | Description |
|---|---|---|---|
| GET | `/reservations?status=&showtimeId=&userId=&email=&page=&pageSize=` | 🛡️ | All reservations |
| GET | `/reports/analytics?from=&to=&top=&format=json\|csv` | 🛡️ | Occupancy and revenue analytics |
| GET | `/reports/sales?from=&to=&interval=day\|week` | 🛡️ | Sales report |

Errors are returned in [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) format.

## Security

- Passwords are hashed with BCrypt. Only the SHA-256 hash of reset and verification tokens is stored.
- JWTs are only accepted when signed with HS256. Changing a password or role rotates the security stamp, so older tokens are rejected.
- Auth endpoints and password change are rate limited per IP (429 + `Retry-After`).
- Login response time does not depend on whether the email is registered. Emails are sent in the background, so the "forgot password" response does not reveal whether an account exists either.
- Secrets are not in the repository; they are provided through user-secrets or environment variables. The application refuses to start if the signing key is too short.
- HSTS and the `X-Content-Type-Options`, `X-Frame-Options` and `Referrer-Policy` headers are sent; the `Server` header is removed.
- In Production, error responses contain no internal details.
- CSV export is protected against formula injection.
- The Docker container runs as a non-root user; the database and Redis ports are bound to `localhost` only.

## Tests

Integration tests run against real PostgreSQL and Redis containers (Testcontainers), so Docker must be running:

```bash
dotnet test
```

Main scenarios covered: concurrent reservations for the same seat, concurrent overlapping showtimes, token revocation, the email verification flow, the seat quota, rate limiting, idempotent admin seeding and the audit log.

## License

[MIT](LICENSE) © 2026 Yasin Özgür Çakmak
