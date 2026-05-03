# Review Checklist — Performance

## async/await

- No `.Result`, `.Wait()`, `GetAwaiter().GetResult()` in async code paths.
- `async void` only on event handlers.
- `Task.Run` not used to "fake async" over CPU-bound work that already runs on a worker thread (e.g., inside an existing async pipeline).
- `ConfigureAwait(false)` on library code (not application code in modern ASP.NET).
- `ValueTask` for hot paths that frequently complete synchronously; do not consume `ValueTask` more than once.
- `IAsyncEnumerable<T>` for streaming; flag `List<T>` accumulation when callers can stream.

## Allocations

- `string` concatenation in loops → `StringBuilder` or `string.Create`.
- `string.Format` with hot-path frequency → interpolated handlers.
- LINQ `ToList()` / `ToArray()` immediately followed by another enumeration — extra allocation for nothing.
- `params object[]` on hot paths — allocates per call. Use overloads.
- Closures capturing `this` in hot lambdas — flag if measurably hot.
- Boxing: `int` → `object`, generic constraints missing.

## Span / Memory

- Parsing/slicing strings: `ReadOnlySpan<char>` over `Substring`.
- File / network buffers: `Memory<byte>` / `IBufferWriter<byte>` over `byte[]`.
- `stackalloc` for small fixed buffers in hot paths (with bounds check).

## EF Core

- N+1 queries — flag `foreach (entity) { ctx.Related.Where(...) }` patterns.
- Missing `.AsNoTracking()` on read-only queries.
- `Include` chains pulling unused columns — projection (`Select`) preferred for read paths.
- Filters applied client-side after `.ToList()` — push to SQL.
- `ChangeTracker` not cleared on long-lived contexts.
- Missing indexes on filtered/joined columns (call out in review when obvious from query shape).

## Hot-Path Heuristics

A method is "hot" if any of:
- It's on a request path of an HTTP server.
- It's inside a `for` / `while` over a user-sized collection.
- It's called from `Hosted` / `BackgroundService` loops.
- A comment or naming suggests perf-sensitive use ("inner loop", "hot", "fast path").

## Concurrency

- Shared mutable state without synchronization (lock, `Interlocked`, immutable patterns).
- `ConcurrentDictionary` misuse — `GetOrAdd` with allocating factory called on every hit.
- `SemaphoreSlim` not awaited in `using` / `try/finally`.
- `Task.WhenAll` with thousands of tasks → bound concurrency (`Parallel.ForEachAsync` with `MaxDegreeOfParallelism`).
