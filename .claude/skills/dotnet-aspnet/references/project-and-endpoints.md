# Project Structure & Endpoints

## Project Structure

- Organize code by feature/domain, not by layer (avoid generic `Controllers/`, `Services/`, `Models/` folders)
- Use `Program.cs` as the composition root — register services, configure middleware pipeline
- Keep `Program.cs` lean by extracting registration into extension methods (e.g., `AddApplicationServices()`, `AddAuthenticationServices()`)
- Use feature folders:
  ```
  Features/
    Orders/
      OrdersController.cs
      CreateOrderRequest.cs
      OrderResponse.cs
      OrderService.cs
    Products/
      ...
  ```

## Controllers vs Minimal APIs

### Controllers

Use controllers for larger APIs with shared conventions, filters, and complex routing:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }
}
```

- Always use `[ApiController]` attribute — enables automatic model validation, binding source inference, and ProblemDetails responses
- Use primary constructor injection for dependencies
- Return `IActionResult` or `ActionResult<T>` for endpoints with multiple response types
- Accept `CancellationToken` on all async endpoints

### Minimal APIs

Use minimal APIs for simple endpoints, microservices, or when startup performance matters:

```csharp
var group = app.MapGroup("/api/orders")
    .WithTags("Orders")
    .RequireAuthorization();

group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
{
    var order = await service.GetByIdAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
.WithName("GetOrderById")
.Produces<OrderResponse>()
.ProducesProblem(StatusCodes.Status404NotFound);
```

- Use `MapGroup()` to share route prefixes, filters, and metadata
- Use `TypedResults` instead of `Results` for compile-time response type verification
- Add `.WithName()` for OpenAPI operation IDs and link generation
- Use endpoint filters for cross-cutting concerns

## Routing & Endpoints

- Use attribute routing on controllers (`[Route]`, `[HttpGet]`, etc.)
- Apply route constraints: `{id:guid}`, `{page:int:min(1)}`, `{slug:regex(^[a-z-]+$)}`
- Use API versioning via URL segment (`/api/v1/orders`) or header-based versioning with `Asp.Versioning.Http`
- Keep route templates consistent — plural nouns for resources (`/api/orders`, not `/api/order`)
- Use `LinkGenerator` for generating URLs to named endpoints

## API Conventions & Response Types

- Annotate endpoints with `[ProducesResponseType]` for OpenAPI documentation
- Use `[Consumes]` and `[Produces]` for content type negotiation
- Return consistent response envelopes or use ProblemDetails for errors
- Use `TypedResults` in minimal APIs for compile-time response verification:

```csharp
group.MapPost("/", async (CreateOrderRequest req, IOrderService service, CancellationToken ct) =>
{
    var id = await service.CreateAsync(req, ct);
    return TypedResults.Created($"/api/orders/{id}", new { id });
});
```
