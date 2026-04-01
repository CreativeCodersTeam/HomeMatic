# Concurrency Control

Use `[Timestamp]` for automatic row-version concurrency (SQL Server / PostgreSQL with `rowversion` or `xmin`):

```csharp
public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
```

Use `[ConcurrencyCheck]` to protect individual properties without a row version column:

```csharp
public class Product
{
    public int Id { get; set; }

    [ConcurrencyCheck]
    public decimal Price { get; set; }
}
```

Or configure via fluent API:

```csharp
modelBuilder.Entity<Order>()
    .Property(o => o.RowVersion)
    .IsRowVersion();

modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .IsConcurrencyToken();
```

Catch `DbUpdateConcurrencyException` at the call site and implement a retry or conflict-resolution strategy:

```csharp
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Reload the entity and resolve the conflict, or inform the user
    await ex.Entries.Single().ReloadAsync();
}
```
