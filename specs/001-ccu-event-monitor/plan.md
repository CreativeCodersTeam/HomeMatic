# Implementation Plan: CCU Event Monitor CLI Command

**Branch**: `feature/cliccueventmonitor` | **Date**: 2026-10-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-ccu-event-monitor/spec.md`

## Summary

Add `hmc ccu events <Name>`:
- It subscribes the local machine as the XML-RPC callback receiver for the HomeMatic, HomeMatic IP and HomeMatic Wired interfaces of one stored CCU.
- It prints every event as one line: time, interface, address, channel name, value key and value. Address and value-key filters are optional.
- It stops cleanly on Ctrl+C, Q or Esc: it unsubscribes every interface within a bounded time and closes the listener.

The technical approach reuses the library's unused `CcuXmlRpcEventServer` and the package `XmlRpcServer`. Gaps found during research are closed with four library changes:
- A robust HttpListener-based `IHttpServer` replaces the package's `SimpleHttpServer`, which hides bind errors and may crash on stop (R1).
- `ICcuEventHandler` passes on the `interfaceId` (R2).
- A factory wires up the event server (R1).
- `DeviceDetails` gets channel names (R6).

Details: [research.md](./research.md).

## Technical Context

**Language/Version**: C# (latest for the SDK), `net10.0` (**FACT** — `source/Directory.Build.props`), `Nullable=enable`, `ImplicitUsings=enable`

**Primary Dependencies**:
- CreativeCoders.Cli.Core / Cli.Hosting 6.7.3 (commands and groups registered by attribute)
- CreativeCoders.Net.XmlRpc 6.7.3 (`XmlRpcServer`, `HttpServerBase<T>`)
- Spectre.Console 0.55.2 (`IAnsiConsole`, `IAnsiConsoleInput`)
- CreativeCoders.Core (`Ensure`)
- System.Threading.Channels and System.Net.HttpListener (BCL)
- No new NuGet packages. All versions are central (`Directory.Packages.props`).

**Storage**: N/A. Uses the existing connection store (`hmc-connections.json`) and the OS credential manager, read only.

**Testing**:
- xUnit 2.9.3, FakeItEasy 9.0.1, AwesomeAssertions 9.4.0, coverlet (**FACT** — test csproj files).
- Command tests use a `StringWriter`-backed `AnsiConsole`.
- Loopback integration tests for the HTTP/XML-RPC server.

**Target Platform**:
- Cross-platform .NET global tool (`hmc`). Primary platforms are macOS and Linux.
- Windows also works, but listening on `http://+:port/` needs admin rights or a URL ACL (R4, UNVERIFIED on Windows).

**Project Type**: Library (`CreativeCoders.HomeMatic.*` NuGet packages) plus CLI tool.

**Performance Goals**:
- Event shown within 1 s of receipt (SC-001).
- Stop within 3 s when the CCU is reachable and 10 s when it is not (SC-002).
- No event loss (SC-004).

**Constraints**:
- No `CancellationToken` from the CLI framework, so the command manages cancellation itself (R5).
- The CCU XML-RPC API has no `CancellationToken`; time limits use `Task.WaitAsync(TimeSpan)` (R3).
- Callback handlers must never throw (R7).

**Scale/Scope**:
- One CCU per run. Typical home installation: up to a few hundred channels, with event bursts of up to around 100/s (for example after a CCU restart).
- About 12 new production files, about 5 modified, about 8 new test files.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` is still the unfilled template (**FACT**), so there are no ratified gates. The project instructions (`CLAUDE.md`, C# guidelines) serve as the de facto gates:

| Gate (source) | Pre-research | Post-design | Notes |
|---|---|---|---|
| `Ensure.NotNull` guards on public and ctor parameters (C# guidelines) | PASS | PASS | All new public types use primary ctors with guarded fields. |
| Primary constructors by default | PASS | PASS | `HttpListenerServer` may need a classic ctor only if the base needs setup. |
| `.ConfigureAwait(false)` in library code, not in tests | PASS | PASS | Applies to XmlRpc/JsonRpc/Cli.Base/Cli.Commands. |
| No `.Result` / `.Wait()` / `.GetAwaiter().GetResult()` | PASS | PASS | Time limits use `Task.WaitAsync` (R3). The `CancelKeyPress` handler only cancels a CTS. |
| `IAnsiConsole` via DI, coloured output | PASS | PASS | Colours match existing commands ([contracts/cli-command.md](./contracts/cli-command.md)). |
| XML docs on all public members | PASS | PASS | Needed for every new and changed public API, including the `ICcuEventHandler` change. |
| Tests for every change (`dotnet-tester`) | PASS | PASS | Test plan in research R10. |
| Simplicity / no speculative features (CLAUDE.md §2) | PASS | PASS with justification | Own HTTP server and breaking interface change: see Complexity Tracking. |
| No new packages without need | PASS | PASS | `Spectre.Console.Testing` (test project only) is needed for CI-stable console output: see research R10. Key tests still fake `IAnsiConsoleInput`. |

## Project Structure

### Documentation (this feature)

```text
specs/001-ccu-event-monitor/
├── spec.md
├── plan.md              # This file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── cli-command.md   # hmc ccu events — options, output, exit codes
│   └── library-api.md   # ICcuEventHandler, HttpListenerServer, factory, DeviceDetails.Channels
├── checklists/requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks) — not created yet
```

### Source Code (repository root)

```text
source/CreativeCoders.HomeMatic.XmlRpc/
├── XmlRpcServiceCollectionExtensions.cs        # MODIFY: register ICcuXmlRpcEventServerFactory
└── Server/
    ├── ICcuEventHandler.cs                     # MODIFY (breaking): interfaceId first parameter
    ├── CcuXmlRpcEventServer.cs                 # MODIFY: pass interfaceId through
    ├── ICcuXmlRpcEventServerFactory.cs         # NEW
    ├── CcuXmlRpcEventServerFactory.cs          # NEW: XmlRpcServer(HttpListenerServer) + Latin1
    └── Http/
        └── HttpListenerServer.cs               # NEW: robust IHttpServer (R1)

source/CreativeCoders.HomeMatic.JsonRpc/Models/
├── DeviceDetails.cs                            # MODIFY: + Channels
└── ChannelDetails.cs                           # NEW

source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Base/
└── Commanding/                                 # NEW stop-signal abstractions (testable)
    ├── IConsoleCancelKeySource.cs / ConsoleCancelKeySource.cs   # Console.CancelKeyPress wrapper
    └── (registration in existing CliBaseServiceCollectionExtensions.cs — MODIFY)

source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Commands/Ccu/Events/
├── MonitorCcuEventsCommand.cs                  # [CliCommand(["ccu","events"])]: orchestration
├── MonitorCcuEventsOptions.cs
├── CcuEventFilter.cs                           # pure matching logic (FR-012)
├── CcuEventRecord.cs
├── CcuEventLineFormatter.cs                    # line + value formatting (FR-005)
├── ChannelNameDirectory.cs                     # lookup + loader via JSON-RPC (FR-013)
├── CcuEventSubscriptions.cs                    # init / init("") per interface with timeouts (FR-003, FR-008/9)
├── CallbackEndpointResolver.cs                 # local IP + free port (FR-011)
├── CcuEventChannelHandler.cs                   # ICcuEventHandler → Channel<CcuEventRecord> (R7)
└── StopKeyWatcher.cs                           # Q/Esc polling via IAnsiConsole.Input (R5)

tests/CreativeCoders.HomeMatic.XmlRpc.Tests/Server/
├── HttpListenerServerTests.cs                  # loopback: start/stop, port in use, 500 on failure
└── CcuXmlRpcEventServerTests.cs                # loopback: multicall of events → handler with interfaceId

tests/CreativeCoders.HomeMatic.JsonRpc.Tests/Models/
└── DeviceDetailsDeserializationTests.cs        # channels array → Channels

tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests/Ccu/Events/
├── MonitorCcuEventsCommandTests.cs             # unknown name, no interface, stop flow, exit codes
├── CcuEventFilterTests.cs
├── CcuEventLineFormatterTests.cs
├── ChannelNameDirectoryTests.cs
├── CcuEventSubscriptionsTests.cs               # partial failure, unsubscribe timeout
└── StopKeyWatcherTests.cs                      # fake IAnsiConsoleInput: Q, Esc, other keys, non-interactive
```

**Structure Decision**: Follow the existing layout. Library changes go into the existing XmlRpc and JsonRpc projects, next to the code they extend. The command goes into `Cli.Commands/Ccu/Events/`, like `Ccu/Backup/`. The reusable Ctrl+C abstraction goes into `Cli.Base`. Tests mirror the source paths in the existing test projects. No new projects.

## Key Design Flow

```text
ExecuteAsync(options)
 ├─ resolve connection by name (ICcuConnectionsStore)            → -1 if unknown
 ├─ CTS ← Ctrl+C (IConsoleCancelKeySource) + Q/Esc (StopKeyWatcher)
 ├─ load ChannelNameDirectory (JSON-RPC, credentials)            → warning on failure
 ├─ resolve CallbackEndpoint, factory.Create(listenPrefix)
 ├─ server.RegisterEventHandler(CcuEventChannelHandler) ; StartAsync  → -1 port in use / denied
 ├─ subscriptions.SubscribeAsync(all kinds)                      → -1 if none succeeded
 ├─ print startup block
 ├─ consume Channel until CTS cancelled: filter → name → format → console
 └─ finally: subscriptions.UnsubscribeAsync (bounded) ; server.StopAsync ; dispose → 0
```

## Risks and open verifications

| ID | Risk / unverified assumption | Mitigation |
|---|---|---|
| R1 | Diagnosis of `SimpleHttpServer` crash-on-stop (UNVERIFIED at runtime) | Avoided anyway by our own server; suggest an upstream fix in CreativeCoders.Core |
| R4 | Windows needs admin rights or a URL ACL for `http://+:port/` (UNVERIFIED) | Specific error message with the `netsh` hint; documented in quickstart |
| R5 | Spectre `Interactive` is `false` when stdin is redirected (UNVERIFIED) | Add a `Console.IsInputRedirected` check if needed |
| R6 | `Device.listAllDetail` returns `channels[].name` (UNVERIFIED) | Fall back to the device name; quickstart #2 confirms against a real CCU |
| R2 | Breaking `ICcuEventHandler` change for outside package users | No users in this repo (FACT); mention in release notes |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Own `HttpListenerServer` instead of the package's `SimpleHttpServer` | Bind errors must come out of `StartAsync` (spec edge case "port in use"), and stopping must not crash the process (FR-008) | The package server adds prefixes after start on a background thread and has no exception handling (FACT, R1); fixing it upstream needs a Core release first |
| Breaking change to `ICcuEventHandler` | Events must show their interface (FR-005), and several interfaces share one callback URL | A server per interface needs 3 ports/firewall rules; a parallel interface duplicates the contract (R2) |
