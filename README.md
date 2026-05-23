# Security (ASP.NET Core)

This solution covers common **web security vulnerabilities** and how to prevent them in ASP.NET Core: **CORS misconfiguration**, **DDoS / rate limiting**, **parameter tampering**, **sensitive data exposure**, **SQL injection**, and **XSS**.

It has two parts:

- `demo/` — a fully working API with side-by-side broken and correct implementations for each vulnerability. We'll walk through each pair live and explain what the attack looks like and why the fix works.
- `exercise/` — a student lab. The app starts and responds, but every endpoint has a vulnerability. Your task is to fix all of them.

---

## Prerequisites

- .NET 10
- Docker

---

## Running the Projects

For the demo:

```powershell
cd demo
docker compose up --build -d
```

For the exercise:

```powershell
cd exercise
docker compose up --build -d
```

This starts:

- A SQL Server container on port `1433`
- An ASP.NET Web API container on port `8080`

Both run in the Development environment, so OpenAPI and the Scalar UI are enabled. Open `http://localhost:8080/scalar/v1`.

> You can also run locally with `dotnet run`, but you'll need a SQL Server instance matching the connection string in `appsettings*.json`. Always prefer Docker.

---

## Part 1: `demo`

### What's registered in `Program.cs`

```csharp
builder
    .Services
    .AddSingleton<HtmlSanitizer>()
    .AddOpenApi()
    .AddDbContext<ProductsDbContext>(...)
    .AddCors(options =>
    {
        options.AddPolicy(CorsPolicies.Broken,  /* AllowAnyOrigin / AllowAnyMethod / AllowAnyHeader */);
        options.AddPolicy(CorsPolicies.Correct, /* WithOrigins / WithMethods / WithHeaders */);
    })
    .AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(RateLimiterPolicies.LoginFixedWindow, /* 5 req / 10 s */);
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    })
    .AddControllers();
```

On startup, it applies migrations and seeds some demo data.

### Endpoints

| Method | Route | Description |
| ------ | ----- | ----------- |
| `GET` | `/api/weather/all` | Uses the active global CORS policy |
| `GET` | `/api/weather/public` | Always allows cross-origin (`[EnableCors]`) |
| `GET` | `/api/weather/internal` | Always blocks cross-origin (`[DisableCors]`) |
| `GET` | `/api/broken/products/search?query=` | SQL injection — raw string interpolation |
| `GET` | `/api/correct/products/search?query=` | Parameterized query |
| `GET` | `/api/broken/comments` | XSS — returns raw HTML with unsanitized input |
| `POST` | `/api/broken/comments` | Stores a comment |
| `GET` | `/api/correct/comments` | Sanitized with `HtmlSanitizer` before rendering |
| `POST` | `/api/correct/comments` | Stores a comment |
| `GET` | `/api/broken/users/{id}` | Sensitive data — returns the full DB entity |
| `GET` | `/api/correct/users/{id}` | Maps to a response model with only safe fields |
| `GET` | `/api/broken/hello` | DDoS — no rate limiting |
| `GET` | `/api/correct/hello` | Protected with a fixed window rate limiter |
| `GET` | `/api/broken/medicals/{id}` | Parameter tampering — no ownership check |
| `GET` | `/api/correct/medicals/{id}` | Returns `403 Forbidden` if the record doesn't belong to the caller |

### Seeded data

Products: Laptop Pro 15, Wireless Mouse, Standing Desk, Ergonomic Chair, USB-C Hub, Notebook A5

---

## Part 2: `exercise`

The app starts and all endpoints respond — but every one has a security flaw. There are also two middleware pieces that need to be wired up in `Program.cs` before the controller fixes will pass.

### What's missing

1. `Program.cs` has no CORS configuration (no `.AddCors()` or `.UseCors()`)
2. `Program.cs` has no rate limiter configuration (no `.AddRateLimiter()` or `.UseRateLimiter()`)
3. `HtmlSanitizer` is not registered as a singleton
4. `BooksController` — `Public()` and `Internal()` are missing `[EnableCors]` / `[DisableCors]` attributes
5. `HelloController` — `Hello()` is missing `[EnableRateLimiting]`
6. `TransactionsController` — `GetById()` returns any transaction to any caller with no ownership check
7. `OrdersController` — `GetById()` returns `OrderDbModel` directly, leaking internal fields
8. `EmployeesController` — `GetByDepartment()` interpolates the query string directly into raw SQL
9. `ReviewsController` — `GetAll()` renders user input as raw HTML, enabling XSS

### Your tasks

**`Program.cs`**

1. Register `HtmlSanitizer` (from `Ganss.Xss`) with Singleton lifetime
2. Add a CORS policy named `CorsPolicies.Correct`:
   - Origins: `"https://trusted-frontend.com"`, `"http://localhost:5173"`
   - Methods: `GET`, `POST`
   - Headers: `Authorization`, `Content-Type`
3. Add a rate limiter policy named `RateLimiterPolicies.LoginFixedWindow`:
   - Type: fixed window
   - Window: 10 seconds, permit limit: 5, queue limit: 0
   - Rejection status code: `429 Too Many Requests`
4. Add `.UseCors(CorsPolicies.Correct)` before `.UseAuthorization()`
5. Add `.UseRateLimiter()` before `.UseAuthorization()`

**`BooksController`**

6. Decorate `Public()` with `[EnableCors(CorsPolicies.Correct)]`
7. Decorate `Internal()` with `[DisableCors]`

**`HelloController`**

8. Decorate `Hello()` with `[EnableRateLimiting(RateLimiterPolicies.LoginFixedWindow)]`

**`TransactionsController`**

9. Read the caller's account id from the `X-Account-Id` request header
10. Return `403 Forbidden` if the transaction's `AccountId` does not match

**`OrdersController`**

11. Introduce an `OrderResponseModel` with only the safe fields: `Id`, `CustomerId`, `Status`, `TotalAmount`, `CreatedAt`
12. Map `OrderDbModel` → `OrderResponseModel` before returning — never return the database entity directly

**`EmployeesController`**

13. Replace the raw `FromSqlRaw` string interpolation with a safe alternative — LINQ `.Where(e => e.Department == department)`, `FromSqlInterpolated`, or `FromSqlRaw` with an explicit `SqlParameter`

**`ReviewsController`**

14. Either change `GetAll()` to return JSON (`ActionResult<ICollection<BookReview>>`), or inject `HtmlSanitizer` and call `sanitizer.Sanitize(review.Body)` before interpolating into the HTML

### You are done when:

- `GET /api/books/all` returns `Access-Control-Allow-Origin` for `https://trusted-frontend.com` but not for untrusted origins
- `GET /api/books/public` always allows cross-origin requests; `GET /api/books/internal` always blocks them
- `GET /api/hello` returns `429 Too Many Requests` after 5 consecutive requests within 10 seconds
- `GET /api/transactions/{id}` returns `403 Forbidden` when the transaction belongs to a different account
- `GET /api/orders/{id}` does not expose `internalStatus`, `costPrice`, `cardLastFour`, `cardToken`, or `billingAddress`
- `GET /api/employees/by-department?department=' OR '1'='1' --` returns an empty array, not the full employee table
- `GET /api/reviews` does not return a raw `<script>` tag when one was submitted

### Seeded data

Employees: Alice Johnson (Engineering), Bob Petrov (Engineering), Clara Ivanova (HR), David Stoev (Finance), Elena Dimitrova (Finance), Martin Georgiev (Engineering)

---

## Grading

| Points | Requirement |
| ------ | ----------- |
| 2 pts | CORS is configured correctly |
| 2 pts | Rate limiting is configured correctly |
| 2 pts | Parameter tampering is prevented |
| 2 pts | Sensitive data is not exposed in the order response |
| 2 pts | SQL injection in employee search is fixed |
| 2 pts | XSS vulnerability in book reviews is fixed |
| **12** | **Total** |

| Points    | Grade |
| --------- | ----- |
| 0–2 pts   | 2     |
| 3–5 pts   | 3     |
| 6–8 pts   | 4     |
| 9–10 pts  | 5     |
| 11–12 pts | 6     |

### Running the grading tests

The exercise comes with an automated grader. You'll need Docker for it.

From the `exercise/` folder:

```bash
docker compose run --rm --build grader
```

Each criterion shows as `[PASS]` or `[FAIL]` with a message explaining what's wrong.
