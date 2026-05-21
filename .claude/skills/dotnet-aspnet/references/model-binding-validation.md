# Model Binding & Validation

ASP.NET Core-specific concerns around mapping HTTP requests onto typed arguments and validating them.

## Binding Sources

- Use binding source attributes explicitly: `[FromBody]`, `[FromQuery]`, `[FromRoute]`, `[FromHeader]`, `[FromForm]`, `[FromServices]`.
- With `[ApiController]`, the framework infers binding sources: complex types default to `[FromBody]`, simple types to `[FromRoute]` / `[FromQuery]`.
- For minimal APIs, binding is determined by parameter type and route template; use the attributes when inference is ambiguous.

```csharp
[HttpPost]
public IActionResult Create(
    [FromBody] CreateOrderRequest body,
    [FromHeader(Name = "X-Tenant-Id")] string tenantId,
    CancellationToken ct)
{
    // ...
}
```

## Data Annotation Validation

```csharp
public record CreateOrderRequest(
    [Required] string CustomerId,
    [Required, MinLength(1)] List<OrderItemRequest> Items);
```

- With `[ApiController]`, validation failures automatically produce a `400 Bad Request` with a `ValidationProblemDetails` body.
- Combine with `[StringLength]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]` for richer constraints.

## FluentValidation

For validation logic that exceeds attribute capabilities (cross-field rules, async lookups), use FluentValidation:

```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().Length(5, 50);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderItemRequestValidator());
    }
}

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
```

Wire validators into the pipeline via an `IEndpointFilter` (minimal APIs) or a custom MVC filter that runs `IValidator<T>` and returns `ValidationProblemDetails` on failure.

## ProblemDetails for Validation Errors

`[ApiController]` returns `ValidationProblemDetails` (RFC 9457 extension) automatically. For minimal APIs, return it explicitly:

```csharp
app.MapPost("/orders", async (CreateOrderRequest req, IValidator<CreateOrderRequest> validator, CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(req, ct);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToDictionary());

    // proceed
    return Results.Created(...);
});
```

---

**For DI lifetimes, the Options pattern, and configuration (`appsettings.json`, User Secrets, environment variables): see the [`dotnet-fundamentals`](../../dotnet-fundamentals/SKILL.md) skill.**
