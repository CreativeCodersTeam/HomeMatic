using System.Collections.Frozen;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Provides an immutable lookup from device and channel addresses to the device names configured on the CCU.
/// </summary>
public sealed class ChannelNameDirectory
{
    private readonly FrozenDictionary<string, string> _names;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelNameDirectory"/> class with the specified names.
    /// </summary>
    /// <param name="names">
    /// The names keyed by device address. Keys are compared ignoring case; if keys differ only in case, the last
    /// one wins.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="names"/> is <see langword="null"/>.</exception>
    public ChannelNameDirectory(IReadOnlyDictionary<string, string> names)
        : this(ToCaseInsensitive(Ensure.NotNull(names)), true)
    {
    }

    private ChannelNameDirectory(FrozenDictionary<string, string> names, bool isAvailable)
    {
        _names = names;
        IsAvailable = isAvailable;
    }

    /// <summary>
    /// Loads the device names from the CCU with the JSON-RPC method <c>Device.listAllDetail</c>.
    /// </summary>
    /// <param name="client">The JSON-RPC client used to query the CCU. The client is logged out afterwards.</param>
    /// <returns>
    /// A task whose result contains the loaded directory and <see langword="null"/> as error, or
    /// <see cref="Unavailable"/> and the error message if the names could not be loaded.
    /// </returns>
    /// <remarks>
    /// Channel names are ignored. Devices whose address or name is <see langword="null"/> or white space are
    /// skipped. If an address occurs more than once, the last name wins. A failed logout is ignored and does not
    /// discard the loaded names. This method does not throw for CCU or transport errors.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async Task<(ChannelNameDirectory Directory, string? Error)> LoadAsync(
        IHomeMaticJsonRpcClient client)
    {
        Ensure.NotNull(client);

        try
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var logout = client.AutoLogout();

            try
            {
                var devices = await client.ListAllDetailsAsync().ConfigureAwait(false);

                foreach (var device in devices)
                {
                    AddName(names, device.Address, device.Name);
                }
            }
            finally
            {
                await TryLogoutAsync(logout).ConfigureAwait(false);
            }

            return (new ChannelNameDirectory(names), null);
        }
        catch (Exception ex)
        {
            return (Unavailable, ex.Message);
        }
    }

    /// <summary>
    /// Loads the device names from the CCU like <see cref="LoadAsync(IHomeMaticJsonRpcClient)"/>, but
    /// waits at most the specified time and stops waiting when cancellation is requested.
    /// </summary>
    /// <param name="client">The JSON-RPC client used to query the CCU. The client is logged out afterwards.</param>
    /// <param name="timeout">The maximum time to wait for the names.</param>
    /// <param name="cancellationToken">A token that stops waiting for the names.</param>
    /// <returns>
    /// A task whose result contains the loaded directory and <see langword="null"/> as error, or
    /// <see cref="Unavailable"/> and the error message if the names could not be loaded. If the names are not
    /// loaded within <paramref name="timeout"/>, the error is <c>timeout</c>.
    /// </returns>
    /// <remarks>
    /// A load that is no longer waited for keeps running in the background and logs out when it completes. This
    /// method does not throw for CCU or transport errors.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="timeout"/> is negative and not <see cref="Timeout.InfiniteTimeSpan"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> is canceled before the names are loaded.
    /// </exception>
    public static async Task<(ChannelNameDirectory Directory, string? Error)> LoadAsync(
        IHomeMaticJsonRpcClient client,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(client);

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await LoadAsync(client).WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            return (Unavailable, "timeout");
        }
    }

    /// <summary>
    /// Returns the device name for the specified device or channel address.
    /// </summary>
    /// <param name="address">The device or channel address (e.g. <c>000A1B2C3D4E5F:1</c>).</param>
    /// <returns>
    /// The name of the device address, which is the part of <paramref name="address"/> before the first <c>:</c>
    /// or the whole address if it contains no <c>:</c>; otherwise <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
    public string? Lookup(string address)
    {
        Ensure.NotNull(address);

        var separatorIndex = address.IndexOf(':');

        var deviceAddress = separatorIndex >= 0 ? address[..separatorIndex] : address;

        return _names.TryGetValue(deviceAddress, out var name)
            ? name
            : null;
    }

    private static FrozenDictionary<string, string> ToCaseInsensitive(IReadOnlyDictionary<string, string> names)
    {
        var caseInsensitiveNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (address, name) in names)
        {
            caseInsensitiveNames[address] = name;
        }

        return caseInsensitiveNames.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task TryLogoutAsync(IAsyncDisposable logout)
    {
        try
        {
            await logout.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // A failed logout must neither discard names that were loaded nor hide the error of a failed load.
        }
    }

    private static void AddName(Dictionary<string, string> names, string? address, string? name)
    {
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        names[address] = name;
    }

    /// <summary>
    /// Gets a directory that contains no names and reports that names could not be loaded.
    /// </summary>
    /// <value>An empty directory whose <see cref="IsAvailable"/> is <see langword="false"/>.</value>
    public static ChannelNameDirectory Unavailable { get; } = new(FrozenDictionary<string, string>.Empty, false);

    /// <summary>
    /// Gets a value indicating whether the names could be loaded from the CCU.
    /// </summary>
    /// <value><see langword="true"/> if the names are available; otherwise, <see langword="false"/>.</value>
    public bool IsAvailable { get; }
}
