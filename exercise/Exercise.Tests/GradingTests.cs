namespace Exercise.Tests;

using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Sdk;

public sealed class GradingTests(
    ExerciseWebApplicationFactory factory) : IClassFixture<ExerciseWebApplicationFactory>
{
    private const string TrustedOrigin = "https://trusted-frontend.com";
    private const string UntrustedOrigin = "https://evil.example.com";

    private const string CorsNotConfiguredHint =
        "CORS does not appear to be configured at all. " +
        "Add .AddCors(...) in Program.cs and call app.UseCors(CorsPolicies.Correct) " +
        "before app.UseAuthorization().";

    private readonly HttpClient client = factory.CreateClient();

    [Fact(DisplayName = "[2 points] CORS is configured correctly")]
    public async Task Cors_ShouldBeConfiguredCorrectly()
    {
        var errors = new List<string>();

        var response = await this.SendWithOrigin(
           "/api/books/all",
           TrustedOrigin);

        if (!ContainsACAOHeader(response))
        {
            errors.Add(
                "Prerequisite failed — GET /api/books/all with a trusted origin returned no " +
                "'Access-Control-Allow-Origin' header. " + CorsNotConfiguredHint);

            Verify(errors);
            return;
        }

        var allTrustedResponse = await this.SendWithOrigin(
            "/api/books/all",
            TrustedOrigin);

        if (!ContainsACAOHeader(allTrustedResponse))
        {
            errors.Add(
                $"GET /api/books/all — expected an 'Access-Control-Allow-Origin' header for the trusted " +
                $"origin '{TrustedOrigin}'. " +
                "Register CorsPolicies.Correct with .WithOrigins(...) and activate it with " +
                "app.UseCors(CorsPolicies.Correct) in Program.cs.");
        }

        var allUntrustedResponse = await this.SendWithOrigin(
            "/api/books/all",
            UntrustedOrigin);

        var allEchosUntrustedOrigin =
            allUntrustedResponse
                .Headers
                .TryGetValues("Access-Control-Allow-Origin", out var allValues) &&
            allValues.Any(static value =>
                value == "*" ||
                value.Contains("evil", StringComparison.OrdinalIgnoreCase));

        if (allEchosUntrustedOrigin)
        {
            errors.Add(
                $"GET /api/books/all — must NOT return 'Access-Control-Allow-Origin' for the untrusted " +
                $"origin '{UntrustedOrigin}'. " +
                "Do not use .AllowAnyOrigin() — use .WithOrigins(\"https://trusted-frontend.com\", " +
                "\"http://localhost:5173\") in the CorsPolicies.Correct policy.");
        }

        var publicResponse = await this.SendWithOrigin(
            "/api/books/public",
            TrustedOrigin);

        if (!ContainsACAOHeader(publicResponse))
        {
            errors.Add(
                "GET /api/books/public — expected an 'Access-Control-Allow-Origin' header. " +
                "This endpoint must always allow cross-origin requests regardless of the global policy. " +
                "Decorate the action with [EnableCors(CorsPolicies.Correct)].");
        }

        var internalResponse = await this.SendWithOrigin(
            "/api/books/internal",
            TrustedOrigin);

        if (ContainsACAOHeader(internalResponse))
        {
            errors.Add(
                $"GET /api/books/internal — expected NO 'Access-Control-Allow-Origin' header even for " +
                $"the trusted origin '{TrustedOrigin}'. " +
                "This endpoint must always block cross-origin requests regardless of the global policy. " +
                "Decorate the action with [DisableCors].");
        }

        var wildcardRoutes = new[] { "/api/books/all", "/api/books/public", "/api/books/internal" };
        foreach (var route in wildcardRoutes)
        {
            var wildcardResponse = await this.SendWithOrigin(
                route,
                UntrustedOrigin);

            var isWildcard =
                wildcardResponse
                    .Headers
                    .TryGetValues("Access-Control-Allow-Origin", out var wildcardValues) &&
                wildcardValues.Any(static value => value == "*");

            if (isWildcard)
            {
                errors.Add(
                    $"GET {route} — 'Access-Control-Allow-Origin: *' detected. " +
                    "CorsPolicies.Correct must not use .AllowAnyOrigin(). " +
                    "Use .WithOrigins(\"https://trusted-frontend.com\", \"http://localhost:5173\") " +
                    ".WithMethods(\"GET\", \"POST\") " +
                    ".WithHeaders(\"Authorization\", \"Content-Type\") " +
                    "exactly as shown in the demo.");
            }
        }

        Verify(errors);
    }

    [Fact(DisplayName = "[2 points] Rate limiting is configured correctly")]
    public async Task RateLimiting_ShoudBeConfiguredCorrectly()
    {
        var errors = new List<string>();

        const int RequestsCount = 20;
        const string Route = "/api/hello";

        var responses = new List<HttpResponseMessage>();

        for (var i = 0; i < RequestsCount; i++)
        {
            var response = await this.client.GetAsync(
                new Uri(Route, UriKind.Relative),
                TestContext.Current.CancellationToken);

            responses.Add(response);
        }

        var has429 = responses
            .Any(static response => response.StatusCode == HttpStatusCode.TooManyRequests);

        if (!has429)
        {
            errors.Add(
                $"GET {Route} — no 429 Too Many Requests was returned after {RequestsCount} consecutive requests. " +
                "Rate limiting does not appear to be active. " +
                "Add .AddRateLimiter(...) in Program.cs with a fixed window policy named " +
                "RateLimiterPolicies.LoginFixedWindow, call app.UseRateLimiter() before app.UseAuthorization(), " +
                "and decorate the action with [EnableRateLimiting(RateLimiterPolicies.LoginFixedWindow)].");
        }

        Verify(errors);
    }

    [Fact(DisplayName = "[2 points] Parameter tampering is prevented")]
    public async Task ParameterTampering_ShouldBePrevented()
    {
        static HttpRequestMessage RequestWithAccountId(
            string route,
            int accountId)
        {
            var method = HttpMethod.Get;
            var uri = new Uri(route, UriKind.Relative);

            var request = new HttpRequestMessage(method, uri);

            request.Headers.Add(
                "X-Account-Id",
                accountId.ToString(CultureInfo.InvariantCulture));

            return request;
        }

        var errors = new List<string>();

        var ownResponse = await this.client.SendAsync(
            RequestWithAccountId("/api/transactions/1", accountId: 201),
            TestContext.Current.CancellationToken);

        if (ownResponse.StatusCode != HttpStatusCode.OK)
        {
            errors.Add(
                $"GET /api/transactions/1 with X-Account-Id: 201 — expected 200 OK, " +
                $"got {(int)ownResponse.StatusCode} {ownResponse.StatusCode}. " +
                "A user must be able to read their own transactions.");
        }

        var foreignResponse = await this.client.SendAsync(
            RequestWithAccountId("/api/transactions/3", accountId: 201),
            TestContext.Current.CancellationToken);

        if (foreignResponse.StatusCode != HttpStatusCode.Forbidden)
        {
            errors.Add(
                $"GET /api/transactions/3 with X-Account-Id: 201 — expected 403 Forbidden, " +
                $"got {(int)foreignResponse.StatusCode} {foreignResponse.StatusCode}. " +
                "A user must not be able to read transactions belonging to another account. " +
                "Check that the account id from the X-Account-Id header matches the transaction's AccountId " +
                "and return this.Forbid() if it does not.");
        }

        var missingResponse = await this.client.SendAsync(
            RequestWithAccountId("/api/transactions/99999", accountId: 201),
            TestContext.Current.CancellationToken);


        if (missingResponse.StatusCode != HttpStatusCode.NotFound)
        {
            errors.Add(
                $"GET /api/transactions/99999 with X-Account-Id: 201 — expected 404 Not Found, " +
                $"got {(int)missingResponse.StatusCode} {missingResponse.StatusCode}.");
        }

        Verify(errors);
    }

    [Fact(DisplayName = "[2 points] Sensitive data is not exposed in the order response")]
    public async Task SensitiveDataExposure_ShouldBePrevented()
    {
        var errors = new List<string>();

        var response = await this.client.GetAsync(
            new Uri("/api/orders/1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            errors.Add(
                $"GET /api/orders/1 — expected 200 OK, got {(int)response.StatusCode} {response.StatusCode}.");

            Verify(errors);
            return;
        }

        var json = await response
            .Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var requiredProperties = new[]
        {
            "id",
            "customerId",
            "status",
            "totalAmount",
            "createdAt",
        };

        foreach (var property in requiredProperties)
        {
            if (!root.TryGetProperty(property, out _))
            {
                errors.Add(
                    $"GET /api/orders/1 — expected the response to contain '{property}' but it was missing. " +
                    "The response model must include all safe fields.");
            }
        }

        var forbiddenProperties = new[]
        {
            "internalStatus",
            "costPrice",
            "cardLastFour",
            "cardToken",
            "billingAddress",
        };

        foreach (var property in forbiddenProperties)
        {
            if (root.TryGetProperty(property, out _))
            {
                errors.Add(
                    $"GET /api/orders/1 — the response must NOT contain '{property}'. " +
                    "Introduce an OrderResponseModel with only the safe fields and map to it " +
                    "before returning — never return the database entity directly.");
            }
        }

        Verify(errors);
    }

    [Fact(DisplayName = "[2 points] SQL injection in employee search is fixed")]
    public async Task SqlInjection_ShouldBePrevented()
    {
        var errors = new List<string>();

        var legitimateResponse = await this.client.GetAsync(
            new Uri("/api/employees/by-department?department=Engineering", UriKind.Relative),
            TestContext.Current.CancellationToken);

        if (legitimateResponse.StatusCode != HttpStatusCode.OK)
        {
            errors.Add(
                $"GET /api/employees/by-department?department=Engineering — " +
                $"expected 200 OK, got {(int)legitimateResponse.StatusCode} {legitimateResponse.StatusCode}. " +
                "The endpoint does not appear to exist. " +
                "Make sure the controller is registered and routed at api/correct/employees/by-department.");

            Verify(errors);
            return;
        }

        var legitimateJson = await legitimateResponse
            .Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var legitimateDocument = JsonDocument.Parse(legitimateJson);
        var legitimateCount = legitimateDocument
            .RootElement
            .GetArrayLength();

        if (legitimateCount == 0)
        {
            errors.Add(
                "GET /api/employees/by-department?department=Engineering — " +
                "expected at least one result for the 'Engineering' department but got none. " +
                "Make sure the database is seeded correctly.");

            Verify(errors);
            return;
        }

        const string InjectionPayload = "' OR '1'='1' --";

        var injectionResponse = await this.client.GetAsync(
            new Uri(
                $"/api/employees/by-department?department={Uri.EscapeDataString(InjectionPayload)}",
                UriKind.Relative),
            TestContext.Current.CancellationToken);

        if (injectionResponse.StatusCode != HttpStatusCode.OK)
        {
            errors.Add(
                "GET /api/employees/by-department with injection payload — " +
                $"expected 200 OK, got {(int)injectionResponse.StatusCode} {injectionResponse.StatusCode}. " +
                "The endpoint must remain reachable after the fix.");

            Verify(errors);
            return;
        }

        var injectionJson = await injectionResponse
            .Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var injectionDocument = JsonDocument.Parse(injectionJson);
        var injectionCount = injectionDocument
            .RootElement
            .GetArrayLength();

        if (injectionCount > 0)
        {
            errors.Add(
                "GET /api/employees/by-department — " +
                $"the injection payload '{InjectionPayload}' returned {injectionCount} row(s). " +
                "This means the department parameter is still concatenated directly into the SQL string. " +
                "Switch to LINQ (.Where(e => e.Department == department)), " +
                "FromSqlInterpolated, or FromSqlRaw with an explicit SqlParameter " +
                "so the value is always treated as data, never as SQL.");
        }

        Verify(errors);
    }

    [Fact(DisplayName = "[2 points] XSS vulnerability in book reviews is fixed")]
    public async Task Xss_ShouldBePrevented()
    {
        var errors = new List<string>();

        var xssReview = new { bookTitle = "Clean Code", body = "<script>alert('XSS')</script>" };
        var safeReview = new { bookTitle = "The Pragmatic Programmer", body = "A must-read for every developer." };

        var xssPost = await this.client.PostAsJsonAsync(
            new Uri("http://localhost/api/reviews"),
            xssReview,
            TestContext.Current.CancellationToken);

        if (xssPost.StatusCode != HttpStatusCode.NoContent)
        {
            errors.Add(
                $"POST /api/reviews — expected 204 No Content, " +
                $"got {(int)xssPost.StatusCode} {xssPost.StatusCode}. " +
                "The Create endpoint must accept a BookReview body and return 204.");

            Verify(errors);
            return;
        }

        await this.client.PostAsJsonAsync(
            new Uri("http://localhost/api/reviews"),
            safeReview,
            TestContext.Current.CancellationToken);

        var getResponse = await this.client.GetAsync(
            new Uri("/api/reviews", UriKind.Relative),
            TestContext.Current.CancellationToken);

        if (getResponse.StatusCode != HttpStatusCode.OK)
        {
            errors.Add(
                $"GET /api/reviews — expected 200 OK, " +
                $"got {(int)getResponse.StatusCode} {getResponse.StatusCode}.");

            Verify(errors);
            return;
        }

        var contentType = getResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;

        if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            var json = await getResponse
                .Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken);

            using var document = JsonDocument.Parse(json);

            if (document.RootElement.GetArrayLength() == 0)
            {
                errors.Add(
                    "GET /api/reviews (JSON) — expected at least one review in the response " +
                    "but got an empty array.");
            }

            Verify(errors);
            return;
        }

        if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            var html = await getResponse
                .Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken);

            if (html.Contains("<script>", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    "GET /api/reviews (HTML) — the response contains a raw <script> tag. " +
                    "The XSS payload was not sanitized. " +
                    "Either return JSON (change the return type to ActionResult<ICollection<BookReview>> " +
                    "and return this.Ok(reviews)), " +
                    "or inject HtmlSanitizer and call htmlSanitizer.Sanitize(review.Body) " +
                    "before interpolating into the <li> element.");
            }

            if (!html.Contains("A must-read for every developer.", StringComparison.Ordinal))
            {
                errors.Add(
                    "GET /api/reviews (HTML) — the safe review body was not found in the response. " +
                    "Make sure sanitization only strips malicious content, not all content.");
            }

            Verify(errors);
            return;
        }

        errors.Add(
            $"GET /api/reviews — unexpected Content-Type '{contentType}'. " +
            "Expected either 'application/json' or 'text/html'.");

        Verify(errors);
    }

    private static bool ContainsACAOHeader(HttpResponseMessage response)
        => response
            .Headers
            .Contains("Access-Control-Allow-Origin");

    private static void Verify(List<string> errors)
    {
        if (errors.Count > 0)
        {
            var errorsString = string.Join(
                Environment.NewLine,
                errors);

            throw new XunitException(errorsString);
        }
    }

    private async Task<HttpResponseMessage> SendWithOrigin(
        string route,
        string origin)
    {
        var method = HttpMethod.Get;
        var uri = new Uri(route, UriKind.Relative);

        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("Origin", origin);

        return await this.client.SendAsync(
            request,
            TestContext.Current.CancellationToken);
    }
}
