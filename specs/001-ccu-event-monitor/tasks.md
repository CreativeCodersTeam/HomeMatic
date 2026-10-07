---

description: "Task list for the CCU event monitor CLI command"
---

# Tasks: CCU Event Monitor CLI Command

**Input**: Design documents from `specs/001-ccu-event-monitor/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/cli-command.md](./contracts/cli-command.md), [contracts/library-api.md](./contracts/library-api.md), [quickstart.md](./quickstart.md)

**Tests**: Included. The project rules (`CLAUDE.md` → "Always include test cases for code changes") and plan research R10 require them.
- Write every test task with the `dotnet-tester` skill: xUnit + FakeItEasy + AwesomeAssertions, Arrange/Act/Assert, no `.ConfigureAwait(false)` in tests.
- Write each story's tests first. They must fail before the implementation exists.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: The user story the task belongs to (US1, US2, US3)

## Conventions for every production task

These rules apply to every production task (sources: `CLAUDE.md` and the C# guidelines):
- File-scoped namespaces, matching the folder.
- Primary constructors with `Ensure.NotNull(...)` guards in field initializers.
- `.ConfigureAwait(false)` in all library and CLI production code.
- Never use `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`.
- `is null` / `is not null` instead of `== null` / `!= null`.
- XML docs on every public member, following the `dotnet-xmldocs` skill.
- `[UsedImplicitly]` on types created only through DI or reflection.
- Escape every CCU-provided string with `Markup.Escape` before printing it as Spectre markup.
- Colours: errors `[bold italic red3]`, success `[bold lime]`, warnings `[yellow]`.

## Path Conventions

- Library: `source/CreativeCoders.HomeMatic.XmlRpc/`, `source/CreativeCoders.HomeMatic.JsonRpc/`
- CLI: `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/`, `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/`
- Tests: `tests/<ProjectName>.Tests/`, mirroring the source folders

**Deviation from plan.md (decided here)**: the command host builds commands only from DI services. **FACT**: `CreativeCoders.Core` `TypeExtensions.CreateInstance` resolves every constructor parameter from the `IServiceProvider` (v6.7.3, `Reflection/TypeExtensions.cs:96`). So every abstraction the command injects must be registered.
- `ICallbackEndpointResolver` and `IConsoleCancelKeySource` therefore live in `Cli.Base` and are registered in `CliBaseServiceCollectionExtensions.cs`.
- Pure helpers (filter, formatter, subscriptions, channel handler, key watcher) are created inside the command with `new`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Get a known-good baseline before any change.

- [X] T001 Run `dotnet build HomeMatic.sln` and `dotnet test HomeMatic.sln` from the repository root. Record any build warnings or failing tests that exist before this feature, so later failures can be attributed correctly. Change no files.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Library changes that every user story needs: a reliable callback HTTP endpoint, events that carry the interface id, and a factory that builds the event server. Contract: [contracts/library-api.md](./contracts/library-api.md) §1–3. Research: R1, R2, R7, R8.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Tests for the foundation ⚠️

- [X] T002 [P] Write loopback tests for the new HTTP server in `tests/CreativeCoders.HomeMatic.XmlRpc.Tests/Server/Http/HttpListenerServerTests.cs`. Each test picks a free port (`TcpListener` on `IPAddress.Loopback`, port 0, read the port, stop), uses the prefix `http://127.0.0.1:<port>/` and a fake `IHttpRequestHandler`. Cases:
  - (a) `StartAsync` then an `HttpClient` POST reaches the handler, which writes a body; the client receives it with status 200.
  - (b) A second `HttpListenerServer` on the same port throws `HttpListenerException` from `StartAsync` (bind errors must come out of `StartAsync`).
  - (c) If the handler throws, the client gets HTTP 500 and the next request still works.
  - (d) `StopAsync` returns without throwing, a second `StopAsync` also does not throw, and `Dispose` after stop does not throw.
  - (e) Two requests sent one after another reach the handler in send order.
- [X] T003 [P] Write loopback integration tests in `tests/CreativeCoders.HomeMatic.XmlRpc.Tests/Server/CcuXmlRpcEventServerTests.cs`. Build the server with `new CcuXmlRpcEventServerFactory(...).Create("http://127.0.0.1:<freePort>/")`, register a fake `ICcuEventHandler`, `StartAsync`, then POST raw XML-RPC bodies (Content-Type `text/xml`, Latin1-encoded). Cases:
  - (a) `system.multicall` with two `event` calls (`interfaceId="hmc-1234abcd-HomeMaticIp"`, addresses `000A1B2C3D4E5F:1` / `000A1B2C3D4E5F:2`, value key `STATE`, boolean values) calls `Event(interfaceId, address, valueKey, value)` twice, in order, with the given interface id.
  - (b) A single `event` with the string value `"Küche"` sent in Latin1 arrives as `"Küche"` (R8).
  - (c) `newDevices`, `deleteDevices` and `updateDevice` reach the handler with the interface id first.
  - (d) `listDevices` returns an empty array.
  - (e) `DisposeAsync` releases the port: a new server can bind the same port afterwards.

### Implementation for the foundation

- [X] T004 [P] Create `HttpListenerServer` in `source/CreativeCoders.HomeMatic.XmlRpc/Server/Http/HttpListenerServer.cs` as `public sealed class HttpListenerServer : HttpServerBase<HttpListenerContext>, IDisposable` (base types: `CreativeCoders.Net.Servers.Http`, package CreativeCoders.Net.XmlRpc 6.7.3).
  - `StartAsync`: add every entry of `Urls` to `HttpListener.Prefixes` **before** `HttpListener.Start()`, so `HttpListenerException` propagates out of `StartAsync`. Then start one background accept loop task.
  - Accept loop: `while (!token.IsCancellationRequested) { var ctx = await listener.GetContextAsync()...; await ProcessRequestAsync(ctx); }`. Handle requests **one at a time** to keep arrival order (FR-006). On `HttpListenerException` or `ObjectDisposedException` after stop was requested, leave the loop quietly.
  - `ProcessRequestAsync`: call the base `HandleRequestAsync(ctx)`. On any exception, set status 500 and close the response. Never let an exception escape the loop.
  - Implement the abstract members: `GetRequest`/`GetResponse` wrap `HttpListenerRequest`/`HttpListenerResponse` with `StreamRequestBody(request.InputStream)` / `StreamResponseBody(response.OutputStream)` (both public in the package; the package's own wrappers are internal, so write small private nested wrapper classes). `FlushAndCloseOutputStreamAsync` flushes and closes `ctx.Response.OutputStream`.
  - `StopAsync`: idempotent. Cancel the token, `Stop()`/`Close()` the listener, and await the loop task.
  - `Dispose`: idempotent, closes the listener.
  - Makes T002 pass.
- [X] T005 Change `ICcuEventHandler` in `source/CreativeCoders.HomeMatic.XmlRpc/Server/ICcuEventHandler.cs` (breaking, R2). The current signatures are `Task Event(string address, string valueKey, object value)`, `Task NewDevices(DeviceDescription[] deviceDescriptions)`, `Task DeleteDevices(DeviceDescription[] deviceDescriptions)`, `Task UpdateDevice(string address, int hint)`. Insert `string interfaceId` as the **first** parameter of each and leave everything else unchanged. Update the XML docs: the interface id is the value the client passed to `init`, and implementations must not throw, because a throwing handler fails the whole `system.multicall` batch with HTTP 500.
- [X] T006 Update `source/CreativeCoders.HomeMatic.XmlRpc/Server/CcuXmlRpcEventServer.cs`:
  - Each private `[XmlRpcMethod]` callback (`event`, `newDevices`, `deleteDevices`, `updateDevice`) passes on the `interfaceId` it already receives (e.g. `x.Event(interfaceId, address, valueKey, value)`).
  - Make `ICcuXmlRpcEventServer` (`Server/ICcuXmlRpcEventServer.cs`) extend `IAsyncDisposable`. `CcuXmlRpcEventServer.DisposeAsync` calls `StopAsync` if the server was started, then disposes `_xmlRpcServer`. **FACT**: `IXmlRpcServer : IDisposable`, and the `XmlRpcServer` built by the factory with `disposeHttpServer: true` also disposes the `HttpListenerServer`.
  - Make `DisposeAsync` safe to call twice. Depends on T005.
- [X] T007 Create `ICcuXmlRpcEventServerFactory` in `source/CreativeCoders.HomeMatic.XmlRpc/Server/ICcuXmlRpcEventServerFactory.cs`, with `ICcuXmlRpcEventServer Create(string listenUrl)`. The XML doc states that `listenUrl` is an HttpListener prefix like `http://+:53817/` and must end with `/`.
- [X] T008 Create `CcuXmlRpcEventServerFactory` in `source/CreativeCoders.HomeMatic.XmlRpc/Server/CcuXmlRpcEventServerFactory.cs`, with a primary ctor `(ILoggerFactory loggerFactory)`.
  - `Create` validates `listenUrl` with `Ensure.IsNotNullOrWhitespace`.
  - It builds `var xmlRpcServer = new XmlRpcServer(new HttpListenerServer(), true) { Encoding = Encoding.Latin1 };` (**FACT**: `Encoding` is settable, 6.7.3 `XmlRpcServer.cs:177`).
  - It returns `new CcuXmlRpcEventServer(xmlRpcServer, loggerFactory.CreateLogger<CcuXmlRpcEventServer>()) { ServerUrl = listenUrl }`.
  - Depends on T004, T006, T007. Makes T003 pass.
- [X] T009 Register `services.TryAddTransient<ICcuXmlRpcEventServerFactory, CcuXmlRpcEventServerFactory>()` inside `AddHomeMaticXmlRpc()` in `source/CreativeCoders.HomeMatic.XmlRpc/XmlRpcServiceCollectionExtensions.cs`. Add a test in `tests/CreativeCoders.HomeMatic.XmlRpc.Tests/XmlRpcServiceCollectionExtensionsTests.cs`: after `AddLogging().AddHomeMaticXmlRpc()`, `ICcuXmlRpcEventServerFactory` resolves. Depends on T008.
- [X] T010 Build the solution (`dotnet build HomeMatic.sln`) and fix every compile error the breaking `ICcuEventHandler` change causes (expected: none; **FACT**: no implementers in the repo). Run `dotnet test tests/CreativeCoders.HomeMatic.XmlRpc.Tests`; T002, T003 and T009 must be green.

**Checkpoint**: The library can receive CCU callbacks with the interface id on a reliable listener. User stories can start.

---

## Phase 3: User Story 1 - Watch live events of a CCU (Priority: P1) 🎯 MVP

**Goal**: `hmc ccu events <Name>` subscribes all supported interfaces of a stored CCU and prints one line per event, with time, interface, address, channel name, value key and value (FR-001–FR-006, FR-010, FR-011, FR-013, FR-014).

**Independent Test**: Quickstart scenarios #1–#3 and #10–#11. Start the command against a stored CCU, operate a device, and see a line with address, name, `STATE` and value. Without US2, the process ends by the default Ctrl+C termination, without unsubscribing.

### Tests for User Story 1 ⚠️

- [X] T011 [P] [US1] Write deserialization tests in `tests/CreativeCoders.HomeMatic.JsonRpc.Tests/Models/DeviceDetailsDeserializationTests.cs`. Use the same `System.Text.Json` options the JSON-RPC client uses (find them in `source/CreativeCoders.HomeMatic.JsonRpc/`). Cases:
  - (a) A `Device.listAllDetail` device object with `"channels":[{"id":"1235","name":"Living room light:1","address":"000A1B2C3D4E5F:1"}]` fills `Channels[0].Name` and `.Address`.
  - (b) A device without `channels` gives `Channels == null`.
  - (c) Unknown channel fields (e.g. `"isReady":"true"`, `"category":"CATEGORY_SENDER"`) are ignored.
- [X] T012 [P] [US1] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/CcuEventLineFormatterTests.cs` for `CcuEventLineFormatter.Format(CcuEventRecord record, ChannelNameDirectory names)`. It returns the escaped markup line `<HH:mm:ss.fff>  <interface>  <address>  <name>  <VALUE_KEY> = <value>`. Cases:
  - Time `21:15:03.412`.
  - Interface labels `HomeMatic`→`BidCos-RF`, `HomeMaticIp`→`HmIP-RF`, `HomeMaticWired`→`BidCos-Wired`.
  - Values: `true`/`false` lower-case; `int` and `double` in invariant culture (`21.5`, not `21,5`, also with culture `de-DE`); strings in double quotes; `null` and `""` → `<empty>`.
  - Name: the channel name; `<unknown>` when the address is not in the directory; `<n/a>` when `names.IsAvailable == false`.
  - Markup characters in a name (`[Test]`) are escaped.
- [X] T013 [P] [US1] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/ChannelNameDirectoryTests.cs`.
  - `ChannelNameDirectory.Lookup(address)`: channel address first, then the device address (the part before `:`), otherwise `null`; case-insensitive.
  - `ChannelNameDirectory.Unavailable.IsAvailable == false`.
  - `ChannelNameDirectory.LoadAsync(IHomeMaticJsonRpcClient client)` with a faked client:
    - Builds entries for devices (`Address`→`Name`) and channels (`Channels[].Address`→`Channels[].Name`).
    - Skips entries whose address or name is null or whitespace.
    - Returns `Unavailable` when `ListAllDetailsAsync` throws (the caller prints the warning).
- [X] T014 [P] [US1] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Base.Tests/Events/CallbackEndpointResolverTests.cs` for `CallbackEndpointResolver.Resolve(Uri ccuUrl, string? callbackHost, int callbackPort)`. Cases:
  - Explicit host and port are used as they are.
  - `callbackPort == 0` gives a port in 1–65535 that can be bound.
  - `callbackHost` null or empty with `ccuUrl = http://127.0.0.1/` gives the host `127.0.0.1`.
  - `ListenPrefix == $"http://+:{Port}/"` and `CallbackUrl == $"http://{Host}:{Port}/"`.
  - `callbackPort` < 0 or > 65535 throws `ArgumentOutOfRangeException`.
- [X] T015 [P] [US1] Write subscription tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/CcuEventSubscriptionsTests.cs` with a faked `IHomeMaticXmlRpcApiBuilder` / `IHomeMaticXmlRpcApi`. Cases:
  - (a) `SubscribeAsync(callbackUrl)` calls `InitAsync(callbackUrl, "hmc-<session>-<kind>")` once for each of `HomeMatic`, `HomeMaticIp`, `HomeMaticWired`, with the CCU URL and the port from `XmlRpcApiAddress(connection.Url, kind)`.
  - (b) One kind throws → result has 2 subscribed and 1 failed with its error message.
  - (c) All throw → `Subscribed` is empty.
  - (d) A kind whose `InitAsync` never completes fails after the subscribe timeout. Inject the timeout as a ctor parameter so the test can use 100 ms; production uses 5 s.
  - (e) `TryGetKind(interfaceId)` maps an issued id back to its `CcuDeviceKind` and returns `false` for foreign ids.
- [X] T016 [P] [US1] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/CcuEventChannelHandlerTests.cs` for `CcuEventChannelHandler`. Cases:
  - `Event(interfaceId, address, valueKey, value)` with a known id writes one `CcuEventRecord` (with `Interface`, `Address`, `ValueKey`, `Value`, and `ReceivedAt` from an injected `TimeProvider`) to the channel.
  - An unknown interface id writes nothing and does not throw.
  - `NewDevices`, `DeleteDevices` and `UpdateDevice` complete without writing anything.
  - No method throws, even after the channel writer is completed.
- [X] T017 [P] [US1] Write command tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/MonitorCcuEventsCommandTests.cs`. Follow the existing pattern: a `StringWriter`-backed `AnsiConsole` with Ansi off, no colours, not interactive (see `tests/.../Ccu/Backup/BackupCcuCommandTests.cs:130-139`), plus faked `ICcuConnectionsStore`, `IHomeMaticXmlRpcApiBuilder`, `ICcuXmlRpcEventServerFactory`, `IHomeMaticJsonRpcClientBuilder`, `ICallbackEndpointResolver` and `IConsoleCancelKeySource`. Cases:
  - (a) Unknown name → output contains `CCU connection 'x' not found`, result `-1`, no `InitAsync` call.
  - (b) All `InitAsync` throw → output contains `No CCU interface could be subscribed`, result `-1`, and the event server was disposed.
  - (c) `StartAsync` throws `HttpListenerException` with error code 98 (Linux), 48 (macOS) or 10048 (Windows), all "address in use" → `Callback port <port> is already in use. Use --callback-port to choose another port.`, result `-1`.
  - (d) A JSON-RPC failure prints `Device names unavailable` and continues.

### Implementation for User Story 1

- [X] T018 [P] [US1] Create `ChannelDetails` in `source/CreativeCoders.HomeMatic.JsonRpc/Models/ChannelDetails.cs`, with `string? Id`, `string? Name`, `string? Address`. Add `public ChannelDetails[]? Channels { get; set; }` to `source/CreativeCoders.HomeMatic.JsonRpc/Models/DeviceDetails.cs` and leave the other members unchanged. Use the same attributes (`[UsedImplicitly]`, `[PublicAPI]`) as `DeviceDetails`. Makes T011 pass.
- [X] T019 [P] [US1] Create `MonitorCcuEventsOptions` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/MonitorCcuEventsOptions.cs`, modelled on `Ccu/Backup/BackupCcuOptions.cs` (same base class, if any).
  - `Name`: `[OptionValue(0, IsRequired = true, HelpText = "Name of the configured CCU connection")]`, "Required, positional (index 0). Must match a stored connection name, ignoring case".
  - `CallbackHost`: `[OptionParameter("callback-host", HelpText = "Host/IP the CCU uses to reach this machine (default: auto)")] string? CallbackHost`.
  - `CallbackPort`: `[OptionParameter("callback-port", HelpText = "Local callback port (default 0 = automatic)")] int CallbackPort`, "`0` = pick a free port automatically. Otherwise 1–65535".
- [X] T020 [P] [US1] Create `CcuEventRecord` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventRecord.cs`, as `public sealed record CcuEventRecord(DateTimeOffset ReceivedAt, CcuDeviceKind Interface, string Address, string ValueKey, object? Value)` (data-model.md: "Local clock when the callback arrived"). The name is resolved at print time.
- [X] T021 [P] [US1] Create `ChannelNameDirectory` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/ChannelNameDirectory.cs`.
  - Members: an immutable dictionary with `StringComparer.OrdinalIgnoreCase`, `bool IsAvailable`, `static ChannelNameDirectory Unavailable`, `string? Lookup(string address)` ("the channel address first, then the device address (the part before `:`), otherwise `null`").
  - `static async Task<(ChannelNameDirectory Directory, string? Error)> LoadAsync(IHomeMaticJsonRpcClient client)`: `await using (client.AutoLogout())`, `ListAllDetailsAsync()`, devices plus `Channels`. Catch exceptions → `(Unavailable, ex.Message)`.
  - Makes T013 pass. Depends on T018.
- [X] T022 [US1] Create `CcuEventLineFormatter` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventLineFormatter.cs`. It is a static class with `Format(CcuEventRecord, ChannelNameDirectory)` and `InterfaceLabel(CcuDeviceKind)`, following [contracts/cli-command.md](./contracts/cli-command.md) "Event line". Makes T012 pass. Depends on T020, T021.
- [X] T023 [P] [US1] Create the callback endpoint parts in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/Events/`:
  - `CallbackEndpoint.cs`: `public sealed record CallbackEndpoint(string Host, int Port)` with the computed properties `ListenPrefix => $"http://+:{Port}/"` and `CallbackUrl => $"http://{Host}:{Port}/"`.
  - `ICallbackEndpointResolver.cs`: `CallbackEndpoint Resolve(Uri ccuUrl, string? callbackHost, int callbackPort)`.
  - `CallbackEndpointResolver.cs`:
    - Host: if `callbackHost` is empty, the local address of a UDP `Socket` connected to `ccuUrl.Host`:`ccuUrl.Port` (no packet sent), read from `LocalEndPoint`.
    - Port: if `callbackPort == 0`, take the port of a `TcpListener(IPAddress.Any, 0)` that is started and stopped right away.
    - Out-of-range ports throw `ArgumentOutOfRangeException`.
  - Register `services.TryAddSingleton<ICallbackEndpointResolver, CallbackEndpointResolver>()` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/CliBaseServiceCollectionExtensions.cs`.
  - Makes T014 pass.
- [X] T024 [P] [US1] Create `CcuEventSubscriptions` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventSubscriptions.cs`.
  - Ctor `(IHomeMaticXmlRpcApiBuilder apiBuilder, Uri ccuUrl, TimeSpan subscribeTimeout, TimeSpan unsubscribeTimeout)`. It creates an 8-hex-char session id once (from `Guid.NewGuid()`).
  - Interface ids follow `hmc-<8 hex session id>-<kind>`.
  - `SubscribeAsync(string callbackUrl)` runs `InitAsync(callbackUrl, id).WaitAsync(subscribeTimeout)` for `HomeMatic`, `HomeMaticIp` and `HomeMaticWired` **in parallel** (`Task.WhenAll` over per-kind try/catch).
  - It returns `SubscriptionResult(IReadOnlyList<CcuDeviceKind> Subscribed, IReadOnlyList<(CcuDeviceKind Kind, string Error)> Failed)`.
  - `bool TryGetKind(string interfaceId, out CcuDeviceKind kind)`.
  - Track the state of each subscription: `Pending → Subscribed | Failed`.
  - Makes T015 pass.
- [X] T025 [P] [US1] Create `CcuEventChannelHandler` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventChannelHandler.cs`, implementing `ICcuEventHandler`. Ctor `(CcuEventSubscriptions subscriptions, ChannelWriter<CcuEventRecord> writer, TimeProvider timeProvider)`.
  - `Event` maps the interface id with `TryGetKind` and calls `writer.TryWrite(new CcuEventRecord(timeProvider.GetLocalNow(), kind, address, valueKey, value))`.
  - The other callbacks return `Task.CompletedTask`.
  - Wrap every body in try/catch so it never throws (R7).
  - Makes T016 pass. Depends on T020, T024.
- [X] T026 [US1] Create `MonitorCcuEventsCommand` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/MonitorCcuEventsCommand.cs`.
  - Declaration: `[CliCommand([CcuCommandGroup.Name, "events"], Description = "Subscribe to the events of a CCU and print them until Ctrl+C, Q or Esc is pressed")]`, implementing `ICliCommand<MonitorCcuEventsOptions>`.
  - Primary ctor `(IAnsiConsole console, ICcuConnectionsStore connectionsStore, IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder, IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder, ICcuXmlRpcEventServerFactory eventServerFactory, ICallbackEndpointResolver endpointResolver)`, with every parameter guarded. Look up the connection exactly like `Ccu/Backup/BackupCcuCommand.cs:43-55`.
  - Flow, in the order of plan.md "Key Design Flow":
    1. Resolve the connection, or print `CCU connection '<name>' not found` and return `-1`.
    2. Get the credentials and load the `ChannelNameDirectory`. On failure print `[yellow]Device names unavailable: <reason>. Showing addresses only.[/]`.
    3. Resolve the endpoint.
    4. Create an unbounded `Channel<CcuEventRecord>` (`SingleReader = true`).
    5. Create the event server from the factory, register `CcuEventChannelHandler` and call `StartAsync`. Map an `HttpListenerException` "address in use" (codes 98/48/10048) to the port-in-use message, and "access denied" (code 5 on Windows) to the URL-ACL message from the contract. Both return `-1`.
    6. Run `SubscribeAsync`. Print one `[yellow]Interface <label> not available: <error>[/]` per failed kind. If none succeeded, print `No CCU interface could be subscribed. Is the CCU reachable at <url>?` and return `-1`.
    7. Print the startup block from the contract.
    8. Read the channel with `await foreach (... ReadAllAsync(token))` and print each formatted line with `console.MarkupLine`.
  - Always `await using` the event server so it is disposed on every path.
  - For US1 the read loop takes a `CancellationToken` parameter that US2 wires up. Until then pass `CancellationToken.None`.
  - Makes T017 pass. Depends on T019–T025, T009.
- [ ] T027 [US1] Run `dotnet test tests/CreativeCoders.HomeMatic.JsonRpc.Tests tests/CreativeCoders.HomeMatic.Tools.Cli.Base.Tests tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests`. All US1 tests must be green. Then run quickstart scenarios #1, #2, #3, #10 and #11 against a real CCU, and note in this task whether `Device.listAllDetail` returned channel names (R6 UNVERIFIED → verified or fallback).

**Checkpoint**: The MVP works. Events of a real CCU appear on the console.

---

## Phase 4: User Story 2 - Stop the monitor cleanly with a keyboard shortcut (Priority: P1)

**Goal**: Ctrl+C, Q or Esc stops the monitor. It unsubscribes every subscribed interface (`init(url, "")`) within a bounded time, stops the listener, prints `Event monitor stopped.` and returns `0` (FR-007, FR-008, FR-009).

**Independent Test**: Quickstart scenarios #4, #5, #6, #12 and #13.

### Tests for User Story 2 ⚠️

- [X] T028 [P] [US2] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/StopKeyWatcherTests.cs` for `StopKeyWatcher.RunAsync(CancellationTokenSource stopSource, CancellationToken token)`, using `A.Fake<IAnsiConsole>()` with a faked `IAnsiConsoleInput` and `Profile.Capabilities.Interactive`. Cases:
  - (a) `IsKeyAvailable()` true and `ReadKey(true)` returns `Q` → `stopSource` is cancelled.
  - (b) Same for `ConsoleKey.Escape`.
  - (c) Lower-case `q` (KeyChar `'q'`, Key `Q`) → cancelled.
  - (d) Other keys (e.g. `A`, `Enter`) → not cancelled; the watcher keeps polling.
  - (e) `Interactive == false` → `ReadKey` and `IsKeyAvailable` are never called.
  - (f) Cancelling `token` ends `RunAsync` without throwing.
  - Inject the poll interval (production 50 ms) so tests run fast.
- [X] T029 [P] [US2] Add unsubscribe tests to `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/CcuEventSubscriptionsTests.cs`. Cases:
  - (a) `UnsubscribeAsync()` calls `InitAsync(callbackUrl, "")` only for kinds that are `Subscribed`, in parallel.
  - (b) A throwing unsubscribe gives a warning entry `(kind, error)` and does not throw.
  - (c) A hanging unsubscribe fails after the unsubscribe timeout (100 ms in the test, 2 s in production).
  - (d) A second `UnsubscribeAsync()` does nothing.
- [X] T030 [P] [US2] Add stop-flow tests to `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/MonitorCcuEventsCommandTests.cs`. The faked `IConsoleCancelKeySource` raises its event after the startup block. Cases:
  - (a) Output ends with `Stopping ...` and `Event monitor stopped.`, result `0`, `InitAsync(url, "")` called for each subscribed kind, the event server disposed.
  - (b) Unsubscribe throws → `[yellow]` warning `Unsubscribe from <label> failed: <error>`, still result `0`.
  - (c) The cancel event raised **during** `SubscribeAsync` (fake `InitAsync` that raises it) → no read loop, every kind already subscribed is unsubscribed, result `0`.
  - (d) Events written before the stop are printed before `Stopping ...`.

### Implementation for User Story 2

- [X] T031 [P] [US2] Create `IConsoleCancelKeySource` and `ConsoleCancelKeySource` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/Commanding/`.
  - `IConsoleCancelKeySource`: `IDisposable Register(Action onCancel)`.
  - `ConsoleCancelKeySource` subscribes to `Console.CancelKeyPress`. Its handler sets `e.Cancel = true` (keeps the process alive for the clean stop) and calls `onCancel`.
  - Disposing the registration unsubscribes the handler.
  - Register `services.TryAddSingleton<IConsoleCancelKeySource, ConsoleCancelKeySource>()` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/CliBaseServiceCollectionExtensions.cs`.
  - Thin wrapper around a static BCL event, so it gets no unit test. It is covered by quickstart #4.
- [X] T032 [P] [US2] Create `StopKeyWatcher` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/StopKeyWatcher.cs`. Ctor `(IAnsiConsole console, TimeSpan pollInterval)`.
  - `RunAsync` returns immediately when `!console.Profile.Capabilities.Interactive`.
  - Otherwise it loops: `if (console.Input.IsKeyAvailable()) { var key = console.Input.ReadKey(true); if (key?.Key is ConsoleKey.Q or ConsoleKey.Escape) stopSource.Cancel(); } await Task.Delay(pollInterval, token)`. It catches `OperationCanceledException` on token cancellation.
  - Use `IsKeyAvailable` polling, not `ReadKeyAsync` (R5).
  - Makes T028 pass.
- [X] T033 [US2] Add `UnsubscribeAsync()` to `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventSubscriptions.cs`.
  - For every `Subscribed` entry, in parallel, run `InitAsync(callbackUrl, string.Empty).WaitAsync(unsubscribeTimeout)` (unsubscribe per `docs/HomeMatic-XmlRpc.md` §4.2.1: "Zum Abmelden von der Ereignisbehandlung wird interface_id leer gelassen").
  - Transitions: `Subscribed → Unsubscribed | UnsubscribeFailed`.
  - Return the failures as `IReadOnlyList<(CcuDeviceKind Kind, string Error)>`; never throw. It is idempotent.
  - Remember `callbackUrl` from `SubscribeAsync`.
  - Makes T029 pass.
- [X] T034 [US2] Wire up stopping in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/MonitorCcuEventsCommand.cs`.
  - Add `IConsoleCancelKeySource cancelKeySource` to the primary ctor, guarded.
  - At the start of `ExecuteAsync`, create `using var stopSource = new CancellationTokenSource()` and `using var registration = cancelKeySource.Register(stopSource.Cancel)`. Start `new StopKeyWatcher(console, TimeSpan.FromMilliseconds(50)).RunAsync(stopSource, stopSource.Token)`.
  - Pass `stopSource.Token` to the read loop instead of `CancellationToken.None`. Check it after `SubscribeAsync`, so a stop during startup skips the loop.
  - In a `finally` that always runs once subscribing has started:
    1. Print `Stopping ...`.
    2. Run `UnsubscribeAsync()` and print `[yellow]Unsubscribe from <label> failed: <error>[/]` for each failure.
    3. Complete the channel writer and drain the events still queued (print them; see T030 d).
    4. Dispose the event server.
    5. Await the key watcher.
    6. Print `[bold lime]Event monitor stopped.[/]`.
    7. Return `0`.
  - Treat the `OperationCanceledException` from the read loop as a normal stop.
  - Makes T030 pass. Depends on T026, T031–T033.
- [ ] T035 [US2] Run `dotnet test tests/CreativeCoders.HomeMatic.Tools.Cli.Base.Tests tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests`. All green. Run quickstart #4, #5, #6, #12 and #13 against a real CCU. For #12 (stdin redirected), confirm R5: Spectre reports `Interactive == false`. If it does not, give `StopKeyWatcher` an injected `Func<bool> isInputRedirected` (production: `() => Console.IsInputRedirected`), skip key polling when it returns `true`, and add a matching case to T028 in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/StopKeyWatcherTests.cs`.

**Checkpoint**: US1 + US2 are complete. The P1 scope is done and the monitor leaves no subscription behind.

---

## Phase 5: User Story 3 - Narrow the output to relevant events (Priority: P2)

**Goal**: The `-a|--address` and `-k|--value-key` filters, comma-separated. OR within one filter, AND between the two. A device address matches all its channels (FR-012).

**Independent Test**: Quickstart scenarios #7, #8 and #9.

### Tests for User Story 3 ⚠️

- [X] T036 [P] [US3] Write tests in `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/CcuEventFilterTests.cs` for `CcuEventFilter.Create(IEnumerable<string>? addresses, IEnumerable<string>? valueKeys)` and `Matches(string address, string valueKey)`. The data-model rule to implement exactly: "`(Addresses empty OR any a: address == a OR (a has no ':' AND address starts with a + ":"))` `AND (ValueKeys empty OR any k: valueKey equals k, ignoring case)`. A device-level event address (no `:`) matches a filter entry only when the two are equal." Cases:
  - No filters → everything matches.
  - Device filter `000A1B2C3D4E5F` matches `000A1B2C3D4E5F:1` and `000A1B2C3D4E5F:12`, but not `000A1B2C3D4E5F0:1`.
  - Channel filter `000A1B2C3D4E5F:1` does not match `000A1B2C3D4E5F:12`.
  - Address comparison ignores case (entries are upper-cased and trimmed).
  - Entries ` STATE ` and `level` match `STATE` and `LEVEL`.
  - Empty entries (`"A,,B"` → `["A","","B"]`) are ignored.
  - Address and value-key filter together → AND.
  - Several addresses → OR.
- [X] T037 [P] [US3] Add filter tests to `tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/MonitorCcuEventsCommandTests.cs`. Cases:
  - (a) With `Addresses = ["000A1B2C3D4E5F"]` and events from two devices, only the matching device's lines are printed.
  - (b) The startup block contains `Filter     : address 000A1B2C3D4E5F | value key STATE` when both are set.
  - (c) There is no `Filter` line when none is set.

### Implementation for User Story 3

- [X] T038 [P] [US3] Create `CcuEventFilter` in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/CcuEventFilter.cs`. It is an immutable `sealed class` with `IReadOnlyList<string> Addresses`, `IReadOnlyList<string> ValueKeys` ("Upper-cased and trimmed. Empty list = no … restriction"), `static Create(...)`, `bool Matches(string address, string valueKey)` and `bool IsEmpty`. Makes T036 pass.
- [X] T039 [P] [US3] Add to `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/MonitorCcuEventsOptions.cs`:
  - `[OptionParameter('a', "address", HelpText = "Comma-separated device or channel addresses to show")] IEnumerable<string>? Addresses`
  - `[OptionParameter('k', "value-key", HelpText = "Comma-separated value keys to show, e.g. STATE,LEVEL")] IEnumerable<string>? ValueKeys`
  - **FACT**: `IEnumerable<T>` is split on `OptionBaseAttribute.Separator`, default `','`. Data model: "Optional. Comma-separated. Each entry is trimmed. Empty entries are ignored".
- [X] T040 [US3] Apply the filter in `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/MonitorCcuEventsCommand.cs`.
  - Build `CcuEventFilter.Create(options.Addresses, options.ValueKeys)` before subscribing.
  - Skip records where `!filter.Matches(record.Address, record.ValueKey)` in the read loop and in the drain on stop.
  - When `!filter.IsEmpty`, add the `Filter` line to the startup block in the format of [contracts/cli-command.md](./contracts/cli-command.md).
  - Makes T037 pass. Depends on T038, T039, T034.
- [ ] T041 [US3] Run `dotnet test tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests` (green), then quickstart #7, #8 and #9 against a real CCU.

**Checkpoint**: All user stories work on their own.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T042 [P] Review every public member added or changed by T004–T009, T018 and T023–T040 with the `dotnet-xmldocs` skill. The files are under `source/CreativeCoders.HomeMatic.XmlRpc/Server/`, `source/CreativeCoders.HomeMatic.JsonRpc/Models/`, `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/` and `source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/`. Fix missing or incomplete `<summary>`, `<param>`, `<returns>` and `<exception>` tags, including the documented breaking change on `ICcuEventHandler`.
- [X] T043 [P] Check `hmc ccu events --help` (and `hmc ccu --help`): the command, every option and the help texts from T019/T039 appear. Fix the help texts in `MonitorCcuEventsOptions.cs` if they are unclear.
- [X] T044 Run `dotnet build HomeMatic.sln` with no new warnings compared to the T001 baseline, and `dotnet test HomeMatic.sln` all green.
- [X] T045 Run a structured code review of the whole change with the `dotnet-reviewer` skill. Scope: the working-tree diff against `main` across `HomeMatic.sln`. Fix the confirmed findings in the affected files under `source/` and `tests/`, then re-run T044.
- [ ] T046 Run the full manual validation `specs/001-ccu-event-monitor/quickstart.md` (#1–#13) against a real CCU. Tick the results in a short note at the end of `specs/001-ccu-event-monitor/quickstart.md`, including the outcome of the R6 and R5 checks. If Windows is available, also check the R4 URL-ACL message.
- [X] T047 [P] Write a follow-up note in `specs/001-ccu-event-monitor/research.md` under R1 for the upstream fix of `SimpleHttpServer` in CreativeCoders.Core (prefixes before `Start`, exception-safe accept loop, no `async void`), so `HttpListenerServer` can be dropped later. Do not change the Core repository.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: none.
- **Foundational (Phase 2)**: after T001. Blocks every user story (the event server factory and `interfaceId` are used by T025/T026).
- **US1 (Phase 3)**: after Phase 2.
- **US2 (Phase 4)**: after US1 (T034 extends the command from T026; T033 extends T024).
- **US3 (Phase 5)**: T036–T039 can start right after Phase 2. T040 needs T034, because both edit the command's read loop and startup block.
- **Polish (Phase 6)**: after every story you want to ship.

### Task-level dependencies

```text
T001 → T002,T003,T004,T005,T007
T005 → T006 → T008 ; T004 → T008 ; T007 → T008 → T009 → T010
T010 → US1
T018 → T021 → T022 ; T020 → T022, T025 ; T024 → T025
T019,T021–T025,T009 → T026 → T027
T026 → T034 ; T031,T032,T033 → T034 → T035
T024 → T033
T038,T039,T034 → T040 → T041
T041 (and/or T035) → T042–T047
```

### Within Each User Story

- Write the tests first and confirm they fail, then implement.
- Records and options, then helpers (directory, formatter, subscriptions, handler), then command orchestration.
- Run the story's tests and quickstart scenarios before you move to the next priority.

### Parallel Opportunities

- Phase 2: T002, T003, T004, T005 and T007 touch different files.
- US1 tests: T011–T017 all [P]. US1 implementation: T018, T019, T020, T021 (after T018), T023 and T024 [P]; T025 after T020+T024.
- US2: T028, T029 and T030 [P]; T031 and T032 [P].
- US3: T036–T039 can run in parallel with US2 (different files).
- Polish: T042, T043 and T047 [P].

---

## Parallel Example: User Story 1

```bash
# All US1 tests at once (different files):
Task: "T011 DeviceDetailsDeserializationTests in tests/CreativeCoders.HomeMatic.JsonRpc.Tests/Models/"
Task: "T012 CcuEventLineFormatterTests in tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/"
Task: "T013 ChannelNameDirectoryTests in tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/"
Task: "T014 CallbackEndpointResolverTests in tests/CreativeCoders.HomeMatic.Tools.Cli.Base.Tests/Events/"
Task: "T015 CcuEventSubscriptionsTests in tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/"

# Independent US1 building blocks at once:
Task: "T018 ChannelDetails + DeviceDetails.Channels"
Task: "T019 MonitorCcuEventsOptions"
Task: "T020 CcuEventRecord"
Task: "T023 CallbackEndpoint + resolver in Cli.Base/Events/"
Task: "T024 CcuEventSubscriptions"
```

## Parallel Example: User Story 2 + User Story 3

```bash
Task: "T028 StopKeyWatcherTests"        # US2
Task: "T031 ConsoleCancelKeySource"     # US2
Task: "T036 CcuEventFilterTests"        # US3
Task: "T038 CcuEventFilter"             # US3
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1 (T001), then Phase 2 (T002–T010).
2. Phase 3 (T011–T027).
3. **Stop and validate**: quickstart #1–#3. Events of a real CCU are visible. Ctrl+C still ends the process the default way, without unsubscribing, which is acceptable only for the MVP demo.

### Incremental Delivery

1. Foundation, then US1: the MVP demo.
2. Add US2: the P1 scope is complete and can be released (clean stop, no lingering subscriptions).
3. Add US3: filters for large installations.
4. Polish (T042–T047), then release, with the `ICcuEventHandler` breaking change in the release notes.

### Recommended release cut

US1 and US2 together form the smallest releasable increment. The spec ranks both P1, and US1 alone leaves subscriptions on the CCU after Ctrl+C.

---

## Notes

- [P] = different files and no dependency on unfinished tasks.
- Commit only on explicit user request (`CLAUDE.md`). Stage files by name.
- Items marked UNVERIFIED in research (R1 crash-on-stop, R4 Windows ACL, R5 `Interactive` with redirected stdin, R6 channel names) are confirmed in T027, T035 and T046.
