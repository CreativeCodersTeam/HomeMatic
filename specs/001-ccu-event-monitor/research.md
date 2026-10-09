# Research: CCU Event Monitor CLI Command

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

Labels: **FACT** = checked against source code or package metadata in this session (location given). **UNVERIFIED** = assumption still to be confirmed. The implementation must confirm it, and every one has a fallback.

## R1 — Receiving CCU callbacks (HTTP + XML-RPC server)

**Decision**: Reuse `CcuXmlRpcEventServer` and the package's `XmlRpcServer`. Replace the package's `SimpleHttpServer` with a new HttpListener-based `IHttpServer` in `CreativeCoders.HomeMatic.XmlRpc` (`Server/Http/HttpListenerServer`, derived from the public `HttpServerBase<HttpListenerContext>`).

**Rationale**:
- **FACT** — `XmlRpcServer(IHttpServer, bool)` decodes and dispatches XML-RPC, including `system.multicall` and `system.listMethods` (CreativeCoders.Net.XmlRpc 6.7.3: `XmlRpcServer.cs:34`, `RequestModelReader.cs:32-57`, `XmlRpcMethodExecutor.cs:25`). CCUs send events in `system.multicall` batches, so this is needed.
- **FACT** — `SimpleHttpServer` (Core repo, `Servers/Http/SimpleImpl/SimpleHttpServer.cs`, same code in v6.7.3 and v6.9.1) has three problems:
  1. URL prefixes are added inside `Listen()` on the worker thread after `HttpListener.Start()`, so a "port in use" error never reaches `StartAsync()`. The spec edge case "port already in use" could then not be reported.
  2. `Listen()` runs on a raw `Thread`, loops on a blocking `GetContext()` and has no try/catch. `StopAsync()` calls `Close()`/`Abort()` while `GetContext()` is blocking.
  3. Requests run in an `async void` worker.
- **UNVERIFIED** — problem 2 most likely throws an unhandled exception on the listener thread when stopping, which would crash the process on Ctrl+C (FR-008). This conclusion comes from reading the code, not from a test run.
- **FACT** — `HttpServerBase<THttpContext>`, `StreamRequestBody` and `StreamResponseBody` are public (`CreativeCoders.Net/Servers/Http/`), so a replacement needs about 80 lines and no new dependency.

**Replacement behaviour**:
- Add the prefixes before `Start()`, so bind errors (`HttpListenerException`) come out of `StartAsync`.
- Run an accept loop with `GetContextAsync()` and a `CancellationToken`. The loop ends quietly on stop.
- Handle requests one at a time, so arrival order is kept (FR-006). Catch exceptions per request and answer with status 500.
- Implement `IDisposable`.

**Alternatives considered**:
- *Fix `SimpleHttpServer` upstream in CreativeCoders.Core.* Best in the long run, but it needs a Core release and a package bump before this feature can ship. Suggested as a follow-up; the local server can then be removed.
- *Kestrel / ASP.NET Core.* No URL-ACL issue on Windows, but the library would need the ASP.NET Core shared framework. Too heavy for a callback endpoint.
- *Raw `TcpListener` with hand-written HTTP parsing.* Fragile, and duplicates what HttpListener already does.

**Follow-up (T047)**: Fix `SimpleHttpServer` upstream in CreativeCoders.Core (`source/Net/CreativeCoders.Net/Servers/Http/SimpleImpl/SimpleHttpServer.cs`):
1. Add the prefixes before `HttpListener.Start()`, so bind errors come out of `StartAsync`.
2. Replace the raw `Thread` + blocking `GetContext()` loop with an exception-safe `GetContextAsync` loop that ends quietly on stop.
3. Replace the `async void` worker with an awaited task that catches exceptions per request (HTTP 500).

Once a Core release with these fixes is referenced, `CreativeCoders.HomeMatic.XmlRpc.Server.Http.HttpListenerServer` can be removed and `CcuXmlRpcEventServerFactory` can use the package server again. Related upstream gap: request decoding ignores `XmlRpcServer.Encoding` (see R8).

## R2 — Telling interfaces apart in callbacks

**Decision**:
- Register each interface with its own `interfaceId` of the form `hmc-<8 hex session id>-<CcuDeviceKind>`.
- Change `ICcuEventHandler` so every callback gets `string interfaceId` as its first parameter. `CcuXmlRpcEventServer` passes on the value it already receives.

**Rationale**:
- **FACT** — `CcuXmlRpcEventServer.Event(interfaceId, address, valueKey, value)` receives the id but calls `x.Event(address, valueKey, value)` (`Server/CcuXmlRpcEventServer.cs:83-93`).
- **FACT** — `ICcuEventHandler` and `CcuXmlRpcEventServer` are not used anywhere in the repository. The breaking change affects no code here; it affects only possible outside users of the NuGet package.
- The per-run session id avoids clashes with other clients registered at the same CCU, such as an earlier crashed run (spec edge case "process ends abnormally").

**Alternatives considered**:
- *One server/port per interface.* No API change, but three listeners and three firewall ports.
- *A second, parallel handler interface.* Not breaking, but two handler contracts for the same callbacks.

## R3 — Subscribe and unsubscribe

**Decision**:
- **Subscribe** with `IHomeMaticXmlRpcApi.InitAsync(callbackUrl, interfaceId)`. **Unsubscribe** with `InitAsync(callbackUrl, "")`.
- One API client per interface: `IHomeMaticXmlRpcApiBuilder.ForUrl(new XmlRpcApiAddress(connection.Url, kind)).Build()`.
- Interfaces: `HomeMatic`, `HomeMaticIp`, `HomeMaticWired`. Subscribe and unsubscribe run in parallel. Each call is limited with `Task.WaitAsync(TimeSpan)`: 5 s for subscribe, 2 s for unsubscribe (FR-009, SC-002).
- An interface whose `init` fails is reported as a warning (spec edge case). If none succeeds, the command fails (FR-010).

**Rationale**:
- **FACT** — `IHomeMaticXmlRpcApi.InitAsync(string xmlRpcUrl, string interfaceId)` documents that "Pass an empty string to unregister" (`Client/IHomeMaticXmlRpcApi.cs:80-90`).
- **FACT** — XML-RPC needs no credentials in this library; the builder has no auth (`HomeMaticXmlRpcApiBuilder`). It sets `Encoding.Latin1` (`HomeMaticXmlRpcApiBuilder.cs:54`).
- **FACT** — Ports: Wired 2000, BidCos-RF 2001, HmIP 2010 (`CcuRpcPorts`).
- **FACT** — The API methods take no `CancellationToken`, so `Task.WaitAsync(TimeSpan)` is the only way to limit how long a call takes.
- `Coupled` (9292) is excluded by the spec.

## R4 — Callback endpoint (address and port)

**Decision**:
- **Host**: `--callback-host`. Otherwise the local IP address the OS routes to the CCU. To find it, a UDP socket is "connected" to `<ccu-host>:<port>` (this sends no packet) and `LocalEndPoint` is read.
- **Port**: `--callback-port`. Otherwise a free port: bind a `TcpListener` to port 0, read the port, release it, then bind the HttpListener to it.
- **Listen prefix**: `http://+:<port>/`.

**Rationale**:
- Gives a working default with no setup on typical home networks (SC-005).
- **FACT** — HttpListener prefixes must end in `/`. `SimpleHttpServer` passes URLs through as prefixes.
- **UNVERIFIED** — on Windows, binding `http://+:<port>/` needs administrator rights or a URL ACL (`netsh http add urlacl`). The command maps "access denied" to an error that gives this hint. macOS/Linux use the managed HttpListener, which has no such restriction.
- The small race between releasing the probe port and binding it is accepted. A bind failure then shows as the "port in use" error, with a hint to use `--callback-port`.

**Alternatives considered**: *A fixed default port* such as 8901. Simpler to set up in a firewall, but it clashes when two monitors run at once. Users who need a fixed port use `--callback-port`.

## R5 — Stop signals (Ctrl+C, Q, Esc)

**Decision**: Use a linked `CancellationTokenSource` owned by the command. Two stop sources, both behind small interfaces so they can be tested:
1. **Ctrl+C** — handle `Console.CancelKeyPress`, set `e.Cancel = true` (keeps the process alive so the clean stop can run), and cancel.
2. **Q / Esc** — a polling loop every 50 ms: `console.Input.IsKeyAvailable()`, then `console.Input.ReadKey(intercept: true)`. It runs only when `console.Profile.Capabilities.Interactive` is `true`.

**Rationale**:
- **FACT** — CreativeCoders.Cli 6.7.3 passes no `CancellationToken` and has no Ctrl+C handling (`ICliCommand<TOptions>.ExecuteAsync(TOptions)`). The command must do it itself.
- **FACT** — Spectre.Console 0.55.2 `IAnsiConsoleInput` has `IsKeyAvailable()`, `ReadKey(bool)` and `ReadKeyAsync(bool, CancellationToken)`. `Profile.Capabilities.Interactive` exists. (Checked with `dotnet-inspect`.)
- **FACT** — .NET 10: `Console.CancelKeyPress`, and `ConsoleCancelEventArgs.Cancel` is settable.
- Polling `IsKeyAvailable()` was chosen over `ReadKeyAsync` for two reasons. A pending blocking read cannot be cancelled cleanly on every platform. And **FACT** — in `Spectre.Console.Testing`, `ReadKeyAsync` throws when no input is queued, which would make tests fragile.
- **UNVERIFIED** — Spectre reports `Interactive = false` when stdin is redirected. If it doesn't, an extra `Console.IsInputRedirected` check goes into the key-source implementation.

**Alternatives considered**: *`PosixSignalRegistration` (SIGINT)*. **FACT** — it exists in .NET 10. But `Console.CancelKeyPress` already works on all platforms, so it is not needed.

## R6 — Channel names

**Decision**:
- Load names once at startup with the existing JSON-RPC client: `IHomeMaticJsonRpcClientBuilder.ForUrl(url).WithCredentials(cred).Build()`, then `AutoLogout()` and `ListAllDetailsAsync()`.
- Extend `DeviceDetails` with `Channels` (`ChannelDetails`: `Id`, `Name`, `Address`).
- Credentials come from `ICcuConnectionsStore.GetCredentials(connection)`, which prompts when none are stored.
- If loading fails, print a warning and continue without names (spec edge case).

**Rationale**:
- **FACT** — Today `DeviceDetails` has only device-level fields (`JsonRpc/Models/DeviceDetails.cs`). Nothing in the library returns channel names.
- **FACT** — `IHomeMaticJsonRpcClient` has `LoginAsync`, `LogoutAsync`, `ListAllDetailsAsync` and `AutoLogout`. The API maps `Device.listAllDetail` (`JsonRpc/Api/IHomeMaticJsonRpcApi.cs:17`).
- **UNVERIFIED** — the CCU's `Device.listAllDetail` response holds a `channels` array per device, with `name` and `address`. Fallback if not: show the device name for every channel. That still meets FR-013 well enough, and needs no further library change.

**Alternatives considered**:
- *A HM-Script/ReGa query.* No support in the library; a new transport would be needed.
- *JSON-RPC `Channel.getName` per address.* One call per channel is too slow at startup.

## R7 — Event flow, ordering and isolation from errors

**Decision**:
- The handler builds a `CcuEventRecord` and writes it to an unbounded `System.Threading.Channels` channel with one reader. A single consumer filters, resolves the name and prints.
- The handler never throws: it only does an O(1) `TryWrite`.

**Rationale**:
- **FACT** — If any call in a `system.multicall` batch throws, the package answers the whole HTTP request with 500 (`XmlRpcServer.cs:98-103`). The CCU would then retry or drop the whole batch.
- An unbounded channel loses nothing (FR-006, SC-004) and keeps the HTTP response fast (SC-001).
- With requests handled one at a time (R1) and one consumer, events are printed in arrival order.

## R8 — Encoding

**Decision**: Set `XmlRpcServer.Encoding = Encoding.Latin1` on the event server.

**Rationale**:
- **FACT** — `XmlRpcServer.Encoding` is settable and defaults to UTF-8 (6.7.3 `XmlRpcServer.cs:39,177`).
- **FACT** — The client side already uses Latin1.
- CCU interface processes send ISO-8859-1. String values with umlauts would otherwise come out garbled.

**Finding during implementation (2026-10-06)**:
- **FACT** — `XmlRpcServer.Encoding` sets only the encoding of the *response* (and Base64 values). Incoming requests are parsed by `XmlReader`, which uses the XML declaration or BOM and defaults to UTF-8 (6.7.3 `ModelReaderBase.ReadXmlDocAsync`).
- So a request with `<?xml ... encoding="ISO-8859-1"?>` is decoded correctly. A Latin-1 body *without* a declaration that contains non-ASCII characters would fail with HTTP 500.
- **UNVERIFIED** — whether the CCU sends the declaration. Check in T046 with a string datapoint that contains umlauts. If it fails, the fix belongs in the request reading of CreativeCoders.Net.XmlRpc (upstream) or in a pre-decoding step inside `HttpListenerServer`.

## R9 — Command surface

**Decision**: `hmc ccu events <Name> [-a|--address <list>] [-k|--value-key <list>] [--callback-host <host>] [--callback-port <port>]`. Lists are comma-separated. Full contract: [contracts/cli-command.md](./contracts/cli-command.md).

**Rationale**:
- **FACT** — Groups and commands are registered by attribute (`[assembly: CliCommandGroup(["ccu"], ...)]`, `[CliCommand(["ccu","backup"])]`) and found by assembly scan (`Program.cs:33`).
- **FACT** — The positional name follows `BackupCcuOptions` (`[OptionValue(0, IsRequired = true)]`).
- **FACT** — `IEnumerable<T>` option properties are split on `OptionBaseAttribute.Separator`, default `','` (SysConsole.Cli.Parsing 6.7.3, `EnumerableValueConverter`). Repeating an option (`-a X -a Y`) is **UNVERIFIED** and not promised in the contract.
- "events" was chosen over "monitor" because it names the subject, like `device list` does.

## R10 — Testing approach

**Decision**:
- xUnit + FakeItEasy + AwesomeAssertions, as the existing test projects use (**FACT**).
- Command tests use `TestConsole` from `Spectre.Console.Testing` (same version as Spectre.Console). Earlier tests created a real `AnsiConsole` with `AnsiSupport.No`, but on GitHub Actions Spectre's default `GitHubEnricher` switches ANSI back on (`GITHUB_ACTIONS=true`), which broke exact output assertions in CI. `TestConsole` turns the default enrichers off.
- The monitor tests keep their thread-safe `LockedTextWriter` as `Profile.Out` of the `TestConsole`, because they read the output while the command writes it.
- Key input is still tested with `A.Fake<IAnsiConsoleInput>()`. `TestConsoleInput` is not thread-safe and cannot simulate input errors.
- `HttpListenerServer` and `CcuXmlRpcEventServer` get loopback integration tests in `CreativeCoders.HomeMatic.XmlRpc.Tests`: a real listener on a free port, plus an `HttpClient` POST of an XML-RPC `system.multicall` with two `event` calls. **FACT** — there are no server tests yet.
- End-to-end checks against a real CCU are manual ([quickstart.md](./quickstart.md)).
