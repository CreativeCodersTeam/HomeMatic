using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Represents one event received from a CCU interface, ready to be printed.
/// </summary>
/// <param name="ReceivedAt">The local time at which the callback arrived. The CCU sends no timestamp.</param>
/// <param name="Interface">The CCU interface that sent the event.</param>
/// <param name="Address">The address of the device or channel that raised the event (e.g. <c>000A1B2C3D4E5F:1</c>).</param>
/// <param name="ValueKey">The name of the changed value (e.g. <c>STATE</c>).</param>
/// <param name="Value">The new value; a <see cref="bool"/>, <see cref="int"/>, <see cref="double"/>, <see cref="string"/> or <see langword="null"/>.</param>
public sealed record CcuEventRecord(
    DateTimeOffset ReceivedAt,
    CcuDeviceKind Interface,
    string Address,
    string ValueKey,
    object? Value);
