# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:

[Step] → verify: [check]
[Step] → verify: [check]
[Step] → verify: [check]


## 5. .NET Core & Web API Standards

When developing or modifying backend code in this project, strictly follow these architectural and coding standards:

### A. Architecture & RESTful Design
- **Layered Architecture:** Never write business logic directly inside Controllers. Controllers only handle HTTP requests; Service layer manages business rules, and Repository / EF Core handles data access.
- **RESTful Standards:** Use plural nouns for endpoints (e.g., `/customers`, `/products`). Do not put action verbs (Get, Insert, Update) in URIs; rely entirely on HTTP methods (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`).

### B. HTTP Status Codes
Always return semantically correct status codes:
- `200 OK`: Successful `GET`, `PUT`, or `PATCH`.
- `201 Created`: Successful `POST` (resource created).
- `204 No Content`: Successful `DELETE` (with no response body).
- `400 Bad Request`: Invalid model/payload.
- `401 Unauthorized`: Authentication missing or invalid.
- `403 Forbidden`: Authenticated but insufficient permissions.
- `404 Not Found`: Resource not found.
- `500 Internal Server Error`: Unhandled server-side exception.

### C. Data Security, Validation & Payload
- **Validation:** Use **FluentValidation** for all request models.
- **Over-posting Prevention:** Never pass domain/entity models directly to/from endpoints. Always use **DTOs (Data Transfer Objects)** or **Records** to transfer only required data.
- **Auth:** Ensure endpoints requiring protection use **JWT (JSON Web Token)** authentication/authorization attributes.

### D. Exception Handling
- **Global Error Handling:** Do not write repetitive `try-catch` blocks in every method. Use a centralized global exception handler (`IExceptionHandler` or middleware).
- **Production Safety:** Never expose raw stack traces in production responses; always return a standardized, clean error model.

### E. Performance & Scalability
- **Dependency Injection:** Register services with the correct lifetime (`AddTransient`, `AddScoped`, `AddSingleton`) to prevent thread-safety issues or memory leaks.
- **Async/Await:** All database and I/O operations **must** use `async`/`await` to avoid thread pool starvation and maximize throughput.
- **Caching:** Implement **Redis** caching for large, frequently read, and rarely changing datasets.

### F. Documentation & Versioning
- **Swagger / OpenAPI:** Keep Swagger integration updated and active for all API endpoints.
- **API Versioning:** Plan and implement URL, Query String, or Header-based versioning (`v1`, `v2`) if breaking changes are anticipated.


### G. N-Tier Project Structure & PostgreSQL Rules
- **Strict 5-Layer Architecture:**
  1. `EntityLayer`: Pure domain entities and enums only. Zero external package dependencies.
  2. `DtoLayer`: Request/Response DTOs or `record` types grouped by feature (e.g., `Auth`, `Movies`, `Reservations`).
  3. `DataAccessLayer`: `AppDbContext`, EF Core `IEntityTypeConfiguration<T>` classes, Migrations, Seed Data, and Repositories.
  4. `BusinessLayer`: Business services (`Interfaces` & `Concrete` managers), `FluentValidation` validators, and custom domain exceptions (`NotFoundException`, `ConflictException`, `BadRequestException`).
  5. `MovieReservationAPI`: Thin controllers, Global `IExceptionHandler`, JWT configuration, DI extensions, and Swagger setup.
- **PostgreSQL Conventions:** Always use `DateTimeOffset` or UTC `DateTime` (`timestamp with time zone`). Configure constraints, indexes, and relationships explicitly using Fluent API (`IEntityTypeConfiguration`), never data annotations on entities.
- **Deferred Features:** Do NOT add Redis caching or API Versioning unless explicitly asked in the current step's prompt.

