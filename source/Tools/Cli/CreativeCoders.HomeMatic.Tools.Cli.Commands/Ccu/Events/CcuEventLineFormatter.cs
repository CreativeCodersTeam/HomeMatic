using System.Globalization;
using System.Text;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Formats received CCU events as console lines.
/// </summary>
public static class CcuEventLineFormatter
{
    private const string Separator = "  ";

    private const string EmptyValue = "<empty>";

    private const string UnknownName = "<unknown>";

    private const string NamesNotAvailable = "<n/a>";

    private static readonly int InterfaceLabelWidth =
        new[] { CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp, CcuDeviceKind.HomeMaticWired }
            .Max(x => InterfaceLabel(x).Length);

    /// <summary>
    /// Formats an event as one line in the form
    /// <c>&lt;HH:mm:ss.fff&gt;  &lt;interface&gt;  &lt;address&gt;  &lt;name&gt;  &lt;VALUE_KEY&gt; = &lt;value&gt;</c>.
    /// </summary>
    /// <param name="record">The event to format.</param>
    /// <param name="names">The directory used to look up the name of the event address.</param>
    /// <returns>The formatted line as plain text, which must not be printed as Spectre.Console markup.</returns>
    /// <remarks>
    /// The interface label is padded to the width of the longest label so that the columns line up.
    /// Booleans are printed as <c>true</c>/<c>false</c>, numbers in the invariant culture, strings in double
    /// quotes, and <see langword="null"/> or empty strings as <c>&lt;empty&gt;</c>. The name is the device name of
    /// the part of the address before the first <c>:</c>, followed by <c>(Channel &lt;x&gt;)</c> with the part
    /// after it, for example <c>Living room light (Channel 1)</c> for <c>000A1B2C3D4E5F:1</c>. An address without
    /// <c>:</c> shows only the device name. The name is <c>&lt;unknown&gt;</c> if the device address is not in
    /// <paramref name="names"/>, and <c>&lt;n/a&gt;</c> if <paramref name="names"/> is not available. Control characters such as CR, LF, TAB or ESC are replaced by
    /// their hexadecimal form (for example <c>\x0A</c>), so that every event stays on one line and cannot inject
    /// terminal sequences.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="record"/> or <paramref name="names"/> is <see langword="null"/>.</exception>
    public static string Format(CcuEventRecord record, DeviceNameDirectory names)
    {
        Ensure.NotNull(record);
        Ensure.NotNull(names);

        var name = names.IsAvailable
            ? FormatName(record.Address, names)
            : NamesNotAvailable;

        var line = string.Join(Separator,
            record.ReceivedAt.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
            InterfaceLabel(record.Interface).PadRight(InterfaceLabelWidth),
            record.Address,
            name,
            $"{record.ValueKey} = {FormatValue(record.Value)}");

        return EscapeControlCharacters(line);
    }

    /// <summary>
    /// Returns the CCU interface name of the specified device kind.
    /// </summary>
    /// <param name="kind">The device kind.</param>
    /// <returns>
    /// <c>BidCos-RF</c>, <c>HmIP-RF</c> or <c>BidCos-Wired</c> for the supported kinds; otherwise the name of
    /// <paramref name="kind"/>.
    /// </returns>
    public static string InterfaceLabel(CcuDeviceKind kind)
    {
        return kind switch
        {
            CcuDeviceKind.HomeMatic => "BidCos-RF",
            CcuDeviceKind.HomeMaticIp => "HmIP-RF",
            CcuDeviceKind.HomeMaticWired => "BidCos-Wired",
            _ => kind.ToString()
        };
    }

    private static string FormatName(string address, DeviceNameDirectory names)
    {
        var deviceName = names.Lookup(address);

        if (deviceName is null)
        {
            return UnknownName;
        }

        var channel = CcuAddress.GetChannel(address);

        return channel is null
            ? deviceName
            : $"{deviceName} (Channel {channel})";
    }

    private static string EscapeControlCharacters(string text)
    {
        if (!text.Any(char.IsControl))
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsControl(c))
            {
                builder.Append(CultureInfo.InvariantCulture, $"\\x{(int)c:X2}");
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => EmptyValue,
            bool boolValue => boolValue ? "true" : "false",
            string { Length: 0 } => EmptyValue,
            string stringValue => $"\"{stringValue}\"",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? EmptyValue
        };
    }
}
