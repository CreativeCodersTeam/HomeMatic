# Modern C# / .NET Patterns

Conventions for code written against current .NET. Use these consistently in new code; match existing project style if it predates these features.

## Primary Constructors (C# 12)

Declare constructor parameters directly on the type — captured as private fields, no boilerplate assignments.

```csharp
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => await orderService.GetByIdAsync(id, ct) is { } order
            ? Ok(order)
            : NotFound();
}
```

- Use for classes whose entire purpose is to receive injected dependencies (controllers, services, middleware, handlers).
- The captured parameter is accessible by name throughout the type — no `_orderService` field required.
- For middleware, inject scoped services via `InvokeAsync` parameters, not the primary constructor.

## `required` Properties + `init`-Only Setters

Force callers to supply mandatory values at object initialization; prevent mutation afterward.

```csharp
public record CreateOrderRequest
{
    public required string CustomerId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
    public string? Notes { get; init; }
}

var req = new CreateOrderRequest
{
    CustomerId = "C-123",
    Items = items,
};
```

- Combine with `record` for value-semantics DTOs.
- `required` runs at compile time — the compiler refuses initialization expressions that omit a required member.
- Use `init` (not `set`) for immutable-after-construction shape.

## Nullable Reference Types

Enable project-wide in the `.csproj`:

```xml
<Nullable>enable</Nullable>
```

- Declare a reference type as nullable explicitly: `string?` means "may be null", `string` means "guaranteed non-null".
- Treat nullable warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) in new projects.
- For legacy code being migrated, use `<Nullable>annotations</Nullable>` first (annotations only, no warnings) to add types incrementally, then flip to `enable`.
- Use the null-forgiving `!` operator only at provable-non-null boundaries (post-validation, after `ArgumentNullException.ThrowIfNull`). Do not sprinkle it to silence warnings.

## `CancellationToken` Propagation

Every async method takes a `CancellationToken` as its **last** parameter and forwards it to every async call it makes.

```csharp
public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    var dto = await _db.Orders
        .AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == id, ct);
    return dto is null ? null : await _mapper.MapAsync(dto, ct);
}
```

- Parameter is named `ct` or `cancellationToken` consistently within a codebase.
- Default to `default` only for entry points that have no caller-supplied token (e.g. CLI `Main`); library code should require the caller to pass one.
- In ASP.NET Core, `HttpContext.RequestAborted` is automatically bound to action parameters of type `CancellationToken`.
- Never swallow `OperationCanceledException` — let it bubble. The host treats it as expected cancellation.

## File-Scoped Namespaces (C# 10)

```csharp
namespace MyCompany.MyProduct.Orders;

public class OrderService { /* ... */ }
```

Default for all new files. Removes one level of indentation across the file.

## `global using` Directives

Centralize common imports in `GlobalUsings.cs`:

```csharp
global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
```

Use the SDK-provided `<ImplicitUsings>enable</ImplicitUsings>` for the default set; add project-specific globals via explicit `global using` declarations.
