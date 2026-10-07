# Feature Specification: CCU Event Monitor CLI Command

**Feature Branch**: `feature/cliccueventmonitor`

**Created**: 2026-10-06

**Status**: Draft

**Input**: User description: "Ein neues Cli Command das für eine CCU die Events aboniert und auf der Console ausgibt bis mit einem Keyboard Shortcut abgebrochen wird" (A new CLI command that subscribes to the events of a CCU and prints them on the console until it is cancelled with a keyboard shortcut.)

## Clarifications

### Session 2026-10-06

- Q: Which keyboard shortcut stops the monitor? → A: Both Ctrl+C and a dedicated key (Q or Esc).
- Q: Which event filters are in scope for the first version? → A: Filter by device/channel address and by value key.
- Q: How is the device identified in each event line? → A: Address plus the device/channel name configured on the CCU.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Watch live events of a CCU (Priority: P1)

A HomeMatic user wants to see, in real time, what happens on their CCU: a switch is toggled, a window contact opens, a thermostat reports a new temperature. They start the monitor command for one of their stored CCU connections. The command subscribes to the CCU's events and prints every incoming event as one readable line on the console, until the user presses the cancel shortcut.

**Why this priority**: This is the whole feature. Without it there is nothing to show. It already helps on its own for debugging devices, checking that a device talks to the CCU, and finding the address and value key of a datapoint.

**Independent Test**: Start the command against a stored CCU, operate a physical or virtual device, and check that a line with the device/channel address, the datapoint name and the new value appears on the console.

**Acceptance Scenarios**:

1. **Given** a stored CCU connection that is reachable, **When** the user starts the monitor command for it, **Then** the command reports that it is listening (including which CCU and which interfaces are subscribed) and keeps running.
2. **Given** the monitor is running, **When** a device on the CCU changes a datapoint value, **Then** one line is printed with the local receive time, the interface, the channel address, the channel name as configured on the CCU, the datapoint (value key) and the value.
3. **Given** the monitor is running and no events arrive, **When** time passes, **Then** the command keeps waiting and does not exit by itself.

---

### User Story 2 - Stop the monitor cleanly with a keyboard shortcut (Priority: P1)

The user has seen enough and presses the cancel shortcut. The command unsubscribes from the CCU, frees all local resources and ends with a short confirmation and a success exit status.

**Why this priority**: The user named it explicitly. Without a clean stop the CCU keeps trying to deliver events to an address that no longer exists, which causes delays and error entries on the CCU.

**Independent Test**: Start the monitor, press the shortcut, and check that the command ends within a few seconds, prints a confirmation, returns a success exit code, and that the CCU no longer holds the subscription.

**Acceptance Scenarios**:

1. **Given** the monitor is running, **When** the user presses the cancel shortcut, **Then** the command unsubscribes from every subscribed interface, prints a confirmation and exits with a success status.
2. **Given** the monitor is running, **When** the user presses the cancel shortcut and the CCU is no longer reachable, **Then** the command still exits. It warns that the unsubscribe failed instead of hanging.
3. **Given** the monitor has been stopped, **When** the user starts it again right away, **Then** it subscribes successfully and receives events again.

---

### User Story 3 - Narrow the output to relevant events (Priority: P2)

A CCU with many devices produces a lot of events. The user wants to see only the events of one device or channel, or of one datapoint such as only `STATE` changes.

**Why this priority**: It makes the monitor usable on large installations. The core value of US1 and US2 does not depend on it.

**Independent Test**: Start the monitor with a filter for one device address. Operate that device and another device, and check that only the first device's events are printed.

**Acceptance Scenarios**:

1. **Given** the monitor is started with an address filter, **When** events arrive from matching and non-matching channels, **Then** only events of matching channels are printed.
2. **Given** the monitor is started with a value-key filter, **When** events arrive with different value keys, **Then** only events with a matching value key are printed.
3. **Given** the monitor is started with both an address filter and a value-key filter, **When** events arrive, **Then** only events that match both filters are printed.

---

### Edge Cases

- **Unknown CCU name**: the command fails before subscribing, with an error that names the unknown connection, and exits with an error status.
- **CCU not reachable or wrong credentials at start**: the command fails with a clear error and an error exit status. It never stays in a "listening" state without a subscription.
- **Only some interfaces can be subscribed** (for example the CCU has no wired interface): the command warns for each failed interface and goes on with the rest. It fails only if no interface could be subscribed.
- **The CCU cannot reach the local machine** (firewall, wrong network, NAT): the startup message shows the callback address it gave to the CCU, so the user can diagnose why no events arrive.
- **Local callback port already in use**: the command fails with an error that names the port and tells the user how to choose another one.
- **The user presses the shortcut during startup** (before the subscription finished): the command cancels, removes any subscriptions already made, and exits.
- **Process ends abnormally** (terminal closed, killed): no clean unsubscribe is possible. This is accepted. The next start must still work.
- **Lots of events in a short time**: every event is printed in arrival order and none is dropped.
- **Device names cannot be loaded at startup**: the command warns and goes on, showing addresses only.
- **Event from a device that is unknown at startup** (for example, paired while the monitor runs): the line shows the address, and the name is marked as unknown.
- **Filter that matches nothing**: the command runs normally and prints no events. The startup message shows the active filters so the user can spot a typo.
- **Values of different types** (boolean, number, text, empty): each is shown in a readable, unambiguous form. An empty value is visibly marked as empty.
- **Input is not an interactive terminal** (input redirected): the dedicated stop keys are not available. The command still runs and can be stopped with Ctrl+C.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The CLI MUST offer a new command in the existing `ccu` command group that monitors the events of one CCU.
- **FR-002**: The command MUST select the CCU by the name of a stored connection, the same way as the existing `ccu backup` command, and MUST reuse the stored credentials (including the prompt when none are stored).
- **FR-003**: The command MUST subscribe to the events of all device interfaces the CCU offers and the library supports (HomeMatic, HomeMatic IP, HomeMatic Wired).
- **FR-004**: After subscribing, the command MUST print a startup message that names the CCU, the subscribed interfaces, the local callback address given to the CCU, the active filters (if any), and the shortcuts that stop the monitor.
- **FR-005**: For every received event the command MUST print one line with the local receive time, the interface, the channel address, the channel name, the value key and the value.
- **FR-006**: Events MUST be printed in the order they were received, and none may be lost while the monitor runs.
- **FR-007**: The command MUST keep running until the user presses the cancel shortcut. Users MUST be able to stop it with Ctrl+C and with the dedicated keys Q and Esc. All of them MUST trigger the same clean stop (FR-008).
- **FR-008**: On cancel, the command MUST unsubscribe every interface it subscribed, release the local callback endpoint, print a confirmation and exit with a success status.
- **FR-009**: Stopping MUST finish within a bounded time even when the CCU does not respond. Failed unsubscribes MUST be reported as warnings, not as a failure exit status.
- **FR-010**: If no interface can be subscribed, or the CCU cannot be contacted at start, the command MUST print a clear error and exit with an error status.
- **FR-011**: Users MUST be able to override the local callback address and port. When they don't, the command MUST pick working defaults by itself.
- **FR-012**: Users MUST be able to filter the printed events by address and by value key:
  - An address filter given as a device address (no channel part) matches all channels of that device. One given as a channel address matches only that channel.
  - A value-key filter matches the value key exactly, ignoring case.
  - Each filter can be given more than once. Values of the same filter combine with OR, and different filters combine with AND.
  - Without filters, all events are printed.
- **FR-013**: The event line MUST show the name of the device as configured on the CCU next to the address, resolved from the device part of the address (before the first `:`) and followed by the channel as `(Channel x)`. Names MUST be loaded once at startup. If loading fails, or the address is unknown, the line MUST still be printed: it shows the address, and the name is marked as unavailable or unknown (see Edge Cases).
- **FR-014**: Errors, warnings and success messages MUST use the same colours and wording style as the existing CLI commands.

### Key Entities

- **CCU connection**: a stored CCU entry (name, URL, credentials in the OS credential store) that the user picks by name.
- **Subscription**: the registration of the local callback endpoint at one CCU interface. It exists from startup until cancel.
- **Event**: one value change reported by the CCU. It holds the interface, channel address, value key and value, plus the local time it was received.
- **Event filter**: optional address and value-key criteria that limit which events are printed (FR-012).
- **Channel name**: the label of an event address: the device name configured on the CCU plus the channel number, e.g. `Living room light (Channel 1)`. Device names are loaded at startup.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An event shows up on the console within 1 second after the CCU sends it.
- **SC-002**: After the cancel shortcut, the command ends within 3 seconds when the CCU is reachable and within 10 seconds when it is not.
- **SC-003**: After a clean stop, the CCU sends no more events to the former callback address (no lingering subscription).
- **SC-004**: In a 10-minute session on a typical home installation, 100% of the events the CCU sends appear on the console, in the order received.
- **SC-005**: A user with a stored CCU connection can start the monitor with a single command and no extra setup on a typical home network.
- **SC-006**: When startup fails, the user can tell the cause (unknown CCU, unreachable CCU, port in use, no interface available) from the error message alone.

## Assumptions

- The CCU and the machine running the CLI are on the same network, and the CCU can open connections to the machine. The command does not set up port forwarding or firewall rules.
- One command run monitors exactly one CCU. Monitoring several CCUs at the same time is out of scope.
- Interfaces not supported by the library (for example virtual devices / CUxD or BinRPC) are out of scope.
- Events are shown only on the console. Writing them to a file or in a machine-readable format (JSON/YAML) is out of scope for the first version. Users can redirect console output.
- The receive time is the local time of the machine running the CLI. The CCU does not provide event timestamps.
- Device and channel names loaded at startup are not refreshed while the monitor runs. Renames during a session show only after a restart.
- Device-management callbacks from the CCU (new, deleted or updated devices) are accepted so the CCU works normally, but they are not a goal of this feature. They may be shown as informational lines.
- The existing event-receiving building blocks of the library may need small extensions (for example passing on the interface of an event). That is an implementation matter for planning.
- No project constitution has been ratified yet (`.specify/memory/constitution.md` is still the template), so no extra governance limits apply.
