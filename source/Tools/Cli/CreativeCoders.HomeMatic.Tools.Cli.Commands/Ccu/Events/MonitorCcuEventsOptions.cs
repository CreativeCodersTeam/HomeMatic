using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Represents the command line options of the <c>ccu events</c> command.
/// </summary>
[PublicAPI]
public class MonitorCcuEventsOptions
{
    /// <summary>
    /// Gets or sets the name of the stored CCU connection whose events are monitored.
    /// </summary>
    /// <value>The connection name. It must match a stored connection name, ignoring case.</value>
    [OptionValue(0, IsRequired = true, HelpText = "Name of the configured CCU connection")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device or channel addresses whose events are shown.
    /// </summary>
    /// <value>
    /// The addresses, given comma-separated on the command line, or <see langword="null"/> to show the events of
    /// all addresses. A device address matches all its channels.
    /// </value>
    [OptionParameter('a', "address", HelpText = "Comma-separated device or channel addresses to show; a device address matches all its channels")]
    public IEnumerable<string>? Addresses { get; set; }

    /// <summary>
    /// Gets or sets the value keys whose events are shown.
    /// </summary>
    /// <value>
    /// The value keys, given comma-separated on the command line, or <see langword="null"/> to show the events of
    /// all value keys. Value keys are compared ignoring case.
    /// </value>
    [OptionParameter('k', "value-key", HelpText = "Comma-separated value keys to show, case-insensitive, e.g. STATE,LEVEL")]
    public IEnumerable<string>? ValueKeys { get; set; }

    /// <summary>
    /// Gets or sets the host name or IP address the CCU uses to reach this machine.
    /// </summary>
    /// <value>
    /// The callback host, or <see langword="null"/>, empty or white space to use the local address the operating
    /// system routes to the CCU.
    /// </value>
    [OptionParameter("callback-host", HelpText = "Host/IP the CCU uses to reach this machine (default: auto)")]
    public string? CallbackHost { get; set; }

    /// <summary>
    /// Gets or sets the local TCP port of the callback endpoint.
    /// </summary>
    /// <value><c>0</c> to pick a free port automatically; otherwise a port between 1 and 65535.</value>
    [OptionParameter("callback-port", HelpText = "Local callback port (default 0 = automatic)")]
    public int CallbackPort { get; set; }
}
