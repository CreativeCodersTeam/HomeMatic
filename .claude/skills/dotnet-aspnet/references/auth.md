# Authentication & Authorization

## JWT Bearer Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });
```

## Authorization Policies

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanEditOrder", policy =>
        policy.Requirements.Add(new OrderOwnerRequirement()));
```

- Use policy-based authorization over role checks in attributes
- Apply `[Authorize]` at controller level, `[AllowAnonymous]` for exceptions
- Implement `IAuthorizationHandler` for resource-based authorization
- For minimal APIs use `.RequireAuthorization("PolicyName")`
