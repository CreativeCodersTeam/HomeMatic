using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Links;

/// <summary>
/// Specifies the state of a HomeMatic communication link as reported in the <c>FLAGS</c>
/// field of a <see cref="Link"/> structure.
/// </summary>
/// <remarks>
/// Values can be combined with bitwise OR. See section 4.2.10 of the HomeMatic XML-RPC
/// specification.
/// </remarks>
[PublicAPI]
[Flags]
public enum LinkFlags
{
    /// <summary>
    /// The link is intact on both sides.
    /// </summary>
    None = 0,

    /// <summary>
    /// The link is broken on the sender side (<c>LINK_FLAG_SENDER_BROKEN</c>).
    /// </summary>
    SenderBroken = 1,

    /// <summary>
    /// The link is broken on the receiver side (<c>LINK_FLAG_RECEIVER_BROKEN</c>).
    /// </summary>
    ReceiverBroken = 2
}
