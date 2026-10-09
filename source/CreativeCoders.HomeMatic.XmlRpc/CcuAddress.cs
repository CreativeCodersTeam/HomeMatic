using System;
using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Provides helpers for CCU device and channel addresses such as <c>000A1B2C3D4E5F:1</c>.
/// </summary>
/// <remarks>
/// A channel address consists of the device address, the <see cref="ChannelSeparator"/> and the channel part. An
/// address without <see cref="ChannelSeparator"/> is a device address.
/// </remarks>
[PublicAPI]
public static class CcuAddress
{
    /// <summary>
    /// The character that separates the device address from the channel part.
    /// </summary>
    public const char ChannelSeparator = ':';

    /// <summary>
    /// Returns the device address of the specified device or channel address.
    /// </summary>
    /// <param name="address">The device or channel address (e.g. <c>000A1B2C3D4E5F:1</c>).</param>
    /// <returns>
    /// The part of <paramref name="address"/> before the first <see cref="ChannelSeparator"/>, or
    /// <paramref name="address"/> itself if it contains no <see cref="ChannelSeparator"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
    public static string GetDeviceAddress(string address)
    {
        Ensure.NotNull(address);

        var separatorIndex = address.IndexOf(ChannelSeparator);

        return separatorIndex < 0 ? address : address[..separatorIndex];
    }

    /// <summary>
    /// Returns the channel part of the specified device or channel address.
    /// </summary>
    /// <param name="address">The device or channel address (e.g. <c>000A1B2C3D4E5F:1</c>).</param>
    /// <returns>
    /// The part of <paramref name="address"/> after the first <see cref="ChannelSeparator"/>, or
    /// <see langword="null"/> if <paramref name="address"/> contains no <see cref="ChannelSeparator"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
    public static string? GetChannel(string address)
    {
        Ensure.NotNull(address);

        var separatorIndex = address.IndexOf(ChannelSeparator);

        return separatorIndex < 0 ? null : address[(separatorIndex + 1)..];
    }
}
