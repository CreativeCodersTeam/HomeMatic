# Data Model: CCU Event Monitor CLI Command

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Research**: [research.md](./research.md)

All entities live in memory for the length of one command run. Nothing is persisted.

## CLI-side entities (`CreativeCoders.HomeMatic.Tools.Cli.Commands`, `Ccu/Events/`)

### MonitorCcuEventsOptions

Command options. Exact syntax: [contracts/cli-command.md](./contracts/cli-command.md).

| Field | Type | Rules |
|---|---|---|
| `Name` | `string` | Required, positional (index 0). Must match a stored connection name, ignoring case (FR-002). |
| `Addresses` | `IEnumerable<string>` | Optional. Comma-separated. Each entry is trimmed. Empty entries are ignored (FR-012). |
| `ValueKeys` | `IEnumerable<string>` | Optional. Comma-separated. Trimmed, compared ignoring case (FR-012). |
| `CallbackHost` | `string?` | Optional. Host name or IP the CCU uses to call back. Automatic when empty (FR-011). |
| `CallbackPort` | `int` | Optional. `0` = pick a free port automatically. Otherwise 1–65535 (FR-011). |

### CcuEventFilter

Immutable. Built from the options and applied to every received event (FR-012).

| Field | Type | Rules |
|---|---|---|
| `Addresses` | `IReadOnlyList<string>` | Upper-cased and trimmed. Empty list = no address restriction. |
| `ValueKeys` | `IReadOnlyList<string>` | Upper-cased and trimmed. Empty list = no value-key restriction. |

**Matching**: `Matches(address, valueKey)` =
`(Addresses empty OR any a: address == a OR (a has no ':' AND address starts with a + ":"))`
`AND (ValueKeys empty OR any k: valueKey equals k, ignoring case)`.
A device-level event address (no `:`) matches a filter entry only when the two are equal.

### CcuEventRecord

One received event, ready to print (FR-005).

| Field | Type | Source |
|---|---|---|
| `ReceivedAt` | `DateTimeOffset` | Local clock when the callback arrived. The CCU sends no timestamp. |
| `Interface` | `CcuDeviceKind` | Mapped back from the `interfaceId` given in `init` (see `EventSubscription`). |
| `Address` | `string` | `event` callback parameter, e.g. `000A1B2C3D4E5F:1`. |
| `ValueKey` | `string` | `event` callback parameter, e.g. `STATE`. |
| `Value` | `object?` | `event` callback parameter: bool, int, double, string or null. |
| `Name` | `string?` | From `ChannelNameDirectory`. `null` means unknown. |

### EventSubscription

The registration of the callback URL at one CCU interface (FR-003, FR-008).

| Field | Type | Rules |
|---|---|---|
| `Interface` | `CcuDeviceKind` | One of `HomeMatic`, `HomeMaticIp`, `HomeMaticWired`. |
| `InterfaceId` | `string` | `hmc-<8 hex session id>-<kind>`. Unique per run. Echoed back by the CCU in every callback. |
| `Api` | `IHomeMaticXmlRpcApi` | Client for `http://<ccu-host>:<CcuRpcPorts>`. |
| `State` | enum | See state transitions below. |

**State transitions**

```text
Pending ──init(url, id) ok──▶ Subscribed ──init(url, "") ok──▶ Unsubscribed
   │                              │
   └──init fails──▶ Failed        └──init(url, "") fails/timeout──▶ UnsubscribeFailed (warning only, FR-009)
```

### CallbackEndpoint

| Field | Type | Rules |
|---|---|---|
| `Host` | `string` | `CallbackHost`, or the local IP address the OS routes to the CCU host (research R4). |
| `Port` | `int` | `CallbackPort`, or a free TCP port picked by the OS (research R4). |
| `ListenPrefix` | `string` | `http://+:{Port}/`. The listener binds to all local addresses. |
| `CallbackUrl` | `string` | `http://{Host}:{Port}/`. Passed to `init` and shown at startup (FR-004). |

### ChannelNameDirectory

Address → name lookup, filled once at startup (FR-013).

| Field | Type | Rules |
|---|---|---|
| `Names` | `IReadOnlyDictionary<string,string>` | Keys are channel and device addresses, ignoring case. |
| `IsAvailable` | `bool` | `false` if loading failed. The output then shows that names are unavailable. |

**Lookup**: the channel address first, then the device address (the part before `:`), otherwise `null`, which prints as unknown.

### MonitorSession (state machine of the command run)

```text
Starting ──all subscriptions failed / CCU unreachable / port in use──▶ Failed (exit -1, FR-010)
   │  (stop requested during start ⇒ Stopping)
   ▼
Listening ──Ctrl+C / Q / Esc──▶ Stopping ──(bounded, FR-009)──▶ Stopped (exit 0)
```

## Library changes

### `ICcuEventHandler` (`CreativeCoders.HomeMatic.XmlRpc.Server`) — breaking change

Every callback gets the `interfaceId` the CCU sends, as the first parameter. Details: [contracts/library-api.md](./contracts/library-api.md).

### `DeviceDetails.Channels` (`CreativeCoders.HomeMatic.JsonRpc.Models`) — additive

| Field | Type | Notes |
|---|---|---|
| `Channels` | `ChannelDetails[]?` | New. Filled by `Device.listAllDetail`. |

`ChannelDetails`: `Id` (`string?`), `Name` (`string?`), `Address` (`string?`). Further JSON fields are ignored.
