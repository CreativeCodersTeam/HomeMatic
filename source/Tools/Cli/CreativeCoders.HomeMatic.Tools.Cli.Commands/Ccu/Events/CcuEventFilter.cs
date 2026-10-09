using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Represents an immutable filter that limits the printed events by address and by value key.
/// </summary>
/// <remarks>
/// Entries of one filter are combined with OR, and the address filter and the value-key filter are combined with
/// AND. An address entry without a channel part (no <c>:</c>) matches the device and all its channels; an entry
/// with a channel part matches only that channel. Addresses and value keys are compared ignoring case.
/// </remarks>
public sealed class CcuEventFilter
{
    private CcuEventFilter(IReadOnlyList<string> addresses, IReadOnlyList<string> valueKeys)
    {
        Addresses = addresses;
        ValueKeys = valueKeys;
    }

    /// <summary>
    /// Creates a filter from the specified address and value-key entries.
    /// </summary>
    /// <param name="addresses">
    /// The device or channel addresses to show, or <see langword="null"/> for no address restriction.
    /// </param>
    /// <param name="valueKeys">The value keys to show, or <see langword="null"/> for no value-key restriction.</param>
    /// <returns>
    /// A filter whose entries are trimmed. Entries that are <see langword="null"/>, empty or white space are
    /// ignored.
    /// </returns>
    public static CcuEventFilter Create(IEnumerable<string>? addresses, IEnumerable<string>? valueKeys)
    {
        return new CcuEventFilter(Normalize(addresses), Normalize(valueKeys));
    }

    /// <summary>
    /// Determines whether an event with the specified address and value key passes the filter.
    /// </summary>
    /// <param name="address">The device or channel address of the event (e.g. <c>000A1B2C3D4E5F:1</c>).</param>
    /// <param name="valueKey">The value key of the event (e.g. <c>STATE</c>).</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="address"/> matches any address entry or there is none, and
    /// <paramref name="valueKey"/> matches any value-key entry or there is none; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="address"/> or <paramref name="valueKey"/> is <see langword="null"/>.
    /// </exception>
    public bool Matches(string address, string valueKey)
    {
        Ensure.NotNull(address);
        Ensure.NotNull(valueKey);

        var deviceAddress = CcuAddress.GetDeviceAddress(address);

        return (Addresses.Count == 0 || Addresses.Any(x => MatchesAddress(address, deviceAddress, x)))
               && (ValueKeys.Count == 0 ||
                   ValueKeys.Any(x => string.Equals(valueKey, x, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool MatchesAddress(string address, string deviceAddress, string filterAddress)
    {
        var comparedAddress = filterAddress.Contains(CcuAddress.ChannelSeparator)
            ? address
            : deviceAddress;

        return string.Equals(comparedAddress, filterAddress, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] Normalize(IEnumerable<string>? entries)
    {
        return (entries ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();
    }

    /// <summary>
    /// Gets the device and channel addresses to show.
    /// </summary>
    /// <value>The trimmed addresses as entered. An empty list means no address restriction.</value>
    public IReadOnlyList<string> Addresses { get; }

    /// <summary>
    /// Gets the value keys to show.
    /// </summary>
    /// <value>The trimmed value keys as entered. An empty list means no value-key restriction.</value>
    public IReadOnlyList<string> ValueKeys { get; }

    /// <summary>
    /// Gets a value indicating whether the filter has no restriction.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if there are neither address nor value-key entries, so every event passes;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsEmpty => Addresses.Count == 0 && ValueKeys.Count == 0;
}
