# Quickstart: Validating `hmc ccu events`

**Contract**: [contracts/cli-command.md](./contracts/cli-command.md) | **Data model**: [data-model.md](./data-model.md)

## Prerequisites

- .NET 10 SDK, and the repository builds: `dotnet build` from the repo root.
- A CCU (CCU3/RaspberryMatic) on the same network, with at least one device you can operate (switch, button, window contact).
- The CCU can open TCP connections to this machine: no firewall blocks inbound traffic on the callback port.
- A stored connection, e.g. `dotnet run --project source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Hmc -- connection add http://<ccu-host> -n home`.
- Windows only: run as administrator, or add a URL ACL for the chosen port (see research R4).

Below, `hmc` stands for `dotnet run --project source/Tools/Cli/CreativeCoders.HomeMatic.Tools.Cli.Hmc --`.

## Automated checks

```bash
dotnet test tests/CreativeCoders.HomeMatic.XmlRpc.Tests
dotnet test tests/CreativeCoders.HomeMatic.JsonRpc.Tests
dotnet test tests/CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests
```

Expected: all green, including the new loopback tests for `HttpListenerServer` and `CcuXmlRpcEventServer`, and the command, filter and formatter tests.

## Manual end-to-end scenarios

| # | Steps | Expected result | Covers |
|---|---|---|---|
| 1 | `hmc ccu events home` | Startup block with the subscribed interfaces, the callback URL and the stop hint. The command keeps running. | US1-1, FR-004 |
| 2 | Toggle a device while #1 runs | Within ~1 s, one line with time, interface, address, channel name, `STATE = true/false` | US1-2, FR-005, SC-001 |
| 3 | Wait 2 min with no device activity | Command still runs, no error | US1-3 |
| 4 | Press `Q` (then repeat #1 and press `Esc`, then repeat and press `Ctrl+C`) | `Stopping ...`, then `Event monitor stopped.` within 3 s. `echo $?` → `0` | US2-1, FR-007, FR-008, SC-002 |
| 5 | After #4, toggle the device and check the CCU's log (or start a second `hmc ccu events home`) | No connection attempts to the old callback port; the new run receives events normally | US2-3, SC-003 |
| 6 | Start, disconnect the CCU from the network, press `Ctrl+C` | Unsubscribe warning(s), then exit within 10 s with code `0` | US2-2, FR-009 |
| 7 | `hmc ccu events home -a <device address>`, operate that device and another one | Only the first device's lines | US3-1 |
| 8 | `hmc ccu events home -k STATE` | Only `STATE` lines | US3-2 |
| 9 | `hmc ccu events home -a <addr> -k STATE` | Only `STATE` lines of that device | US3-3 |
| 10 | `hmc ccu events doesnotexist` | `CCU connection 'doesnotexist' not found`, exit `-1` (shown as 255 by POSIX shells) | FR-010, SC-006 |
| 11 | Run #1 twice at the same time with `--callback-port 50000` | The second run reports that port 50000 is in use, exit `-1` | Edge case port in use |
| 12 | `echo | hmc ccu events home` (stdin redirected) | Runs; Q/Esc are not read; `Ctrl+C` stops it cleanly | Edge case non-interactive |
| 13 | Press `Ctrl+C` during the startup output | Clean exit; no subscription left (check as in #5) | Edge case stop during startup |

## Validation log

### 2026-10-07 — partial run against CCU `OG` (CCU3, `http://ccu3-og.homie/`), macOS, stdin redirected

Driven by a script with SIGINT stops, so no Q/Esc and no deliberate device operation.

| # | Result |
|---|---|
| 1 | ✅ Startup block shown. BidCos-RF and HmIP-RF subscribed, callback `http://192.168.2.71:<port>/`. BidCos-Wired warning (the CCU has no wired interface). Startup took 11.4 s on the first run and 1.3 s on the second. |
| 2 | ⚠️ Partly. 227 HmIP events in about 20 min, with channel names (e.g. `Flurbeleuchtung Steckdose:0`) and correctly formatted values (`ACTUAL_TEMPERATURE = 23`, `CARRIER_SENSE_LEVEL = 1.5`). No deliberate toggle, so latency (SC-001) was not measured. |
| 3 | ✅ Kept running for about 20 min without errors. |
| 4 | ⚠️ Only Ctrl+C (SIGINT): `Stopping ...` / `Event monitor stopped.`, exit 0, stop in 0.11 s. Q/Esc not tested. |
| 5 | ✅ A restart right after a clean stop subscribes and receives events again. The CCU-side check (no calls to the old port) was not done. |
| 10 | ✅ `CCU connection '__no_such_ccu__' not found`, exit 255 (= -1). |
| 12 | ✅ Stdin redirected: runs, and SIGINT stops it cleanly. **FACT**: Spectre reports `Interactive = false` (R5). |
| 6, 7–9, 11, 13 | Not run. |

**Research items**: R6 is verified (channel names come from `Device.listAllDetail`). R8 (umlauts) and R4 (Windows) are still open.

**Findings**:
- F1: when output is redirected, Spectre wraps lines at 80 columns, so one event spans several lines (FR-005).
- F2: the BidCos-Wired warning shows a raw XML parser message ("DTD is prohibited …"), which does not tell the user the cause (SC-006).
- **Update 2026-10-07 09:15**: F1 and F2 are fixed. A re-run against `OG` with output redirected to a file showed one line per event (e.g. `09:14:48.668  BidCos-RF     CENTRAL  <unknown>  PONG = "ccu3-og-BidCos-RF#…"`) and the warning `Interface BidCos-Wired not available: no XML-RPC service on port 2000`. Stopped with SIGINT: `Stopping ...` / `Event monitor stopped.`, exit 0.
- Test-harness note: background jobs in non-interactive bash ignore SIGINT. Scripts that send SIGINT must use `set -m`.
