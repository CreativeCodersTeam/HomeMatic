# Contract: `hmc ccu events`

**Spec**: [../spec.md](../spec.md) | **Data model**: [../data-model.md](../data-model.md)

## Synopsis

```text
hmc ccu events <Name> [-a|--address <addr>[,<addr>...]] [-k|--value-key <key>[,<key>...]]
                      [--callback-host <host>] [--callback-port <port>] [-v|--verbose <level>]
```

| Argument / option | Required | Description | Req. |
|---|---|---|---|
| `<Name>` | yes | Name of the stored CCU connection (`hmc connection list`). Case-insensitive. | FR-002 |
| `-a`, `--address` | no | Comma-separated device or channel addresses. A device address matches all its channels. | FR-012 |
| `-k`, `--value-key` | no | Comma-separated value keys (e.g. `STATE,LEVEL`). Case-insensitive. | FR-012 |
| `--callback-host` | no | Host/IP the CCU uses to reach this machine. Default: the local address routed to the CCU. | FR-011 |
| `--callback-port` | no | TCP port of the local callback endpoint. Default `0` = a free port picked automatically. | FR-011 |
| `-v`, `--verbose` | no | Existing shared option (`CliCommandOptionsBase`). | — |

Filter combination: values in one option are combined with OR. `--address` and `--value-key` are combined with AND.

## Console output

Colours follow the existing commands: errors `[bold italic red3]`, success `[bold lime]`, warnings `[yellow]`. All CCU-provided text is escaped before it is printed as markup.

### Startup (FR-004)

```text
Listening for events of CCU 'home' (http://ccu.local/)
  Interfaces : BidCos-RF, HmIP-RF
  Callback   : http://192.168.1.20:53817/
  Filter     : address 000A1B2C3D4E5F | value key STATE
  Press Ctrl+C, Q or Esc to stop.
```

- The `Filter` line is shown only when a filter is set.
- One warning line for each interface that could not be subscribed: `Interface BidCos-Wired not available: <reason>`. `<reason>` is `timeout`, `no XML-RPC service on port <port>` (the response is not XML, e.g. an HTML page), `connection failed on port <port> (<short reason>)` (e.g. connection refused, host unreachable), or the error message otherwise. Unsubscribe warnings use the same reasons.
- If names could not be loaded: `Device names unavailable: <reason>. Showing addresses only.`

### Event line (FR-005, FR-013)

```text
<HH:mm:ss.fff>  <interface>  <address>  <name>  <VALUE_KEY> = <value>
21:15:03.412  HmIP-RF    000A1B2C3D4E5F:1  Living room light (Channel 1)  STATE = true
```

| Column | Format |
|---|---|
| time | Local receive time, `HH:mm:ss.fff` |
| interface | `BidCos-RF`, `HmIP-RF` or `BidCos-Wired` |
| name | Device name of the address part before the first `:`, followed by `(Channel x)` with the part after it (e.g. `Living room light (Channel 1)`). A device address without `:` shows only the device name. `<unknown>` when the device address is not in the directory, `<n/a>` when names could not be loaded. |
| value | `true`/`false` · numbers in invariant culture · strings in double quotes · `<empty>` for null or `""` |

Device-management callbacks (`newDevices`, `deleteDevices`, `updateDevice`) are acknowledged but not printed.

### Stop (FR-008, FR-009)

```text
Stopping ...
Event monitor stopped.
```

- Before the success line: one warning for each interface where unsubscribing failed or timed out, e.g. `Unsubscribe from HmIP-RF failed: timeout`.

## Exit codes

| Code | When |
|---|---|
| `0` | Stopped by Ctrl+C, Q or Esc (also when unsubscribing failed, FR-009) |
| `-1` | Unknown connection name, CCU not reachable, no interface subscribed, callback port in use or access denied (FR-010) |
| `1` | Unexpected `HomeMaticException`, `FaultException` or `HttpRequestException` caught by the global handler in `Program.cs` (existing behaviour) |

## Error messages (SC-006)

| Cause | Message |
|---|---|
| Unknown name | `CCU connection '<name>' not found` |
| No interface | `No CCU interface could be subscribed. Is the CCU reachable at <url>?` |
| Port in use | `Callback port <port> is already in use. Use --callback-port to choose another port.` |
| Access denied (Windows) | `Not allowed to listen on port <port>. Run as administrator or add a URL ACL: netsh http add urlacl url=http://+:<port>/ user=<user>` |
| Invalid port | `--callback-port must be between 0 and 65535` |
