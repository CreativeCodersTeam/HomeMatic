using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Links;

/// <summary>
/// Specifies the option flags for the <c>getLinks</c> XML-RPC method.
/// </summary>
/// <remarks>
/// Values can be combined with bitwise OR. See section 4.2.10 of the HomeMatic XML-RPC
/// specification for details.
/// </remarks>
[PublicAPI]
[Flags]
public enum GetLinksFlags
{
    /// <summary>
    /// No optional fields are requested. The default behaviour.
    /// </summary>
    None = 0,

    /// <summary>
    /// Returns the links of all channels in the same group when the address denotes a grouped channel
    /// (<c>GL_FLAG_GROUP</c>).
    /// </summary>
    Group = 1,

    /// <summary>
    /// Includes the <c>SENDER_PARAMSET</c> field in the returned <see cref="Link"/> structures
    /// (<c>GL_FLAG_SENDER_PARAMSET</c>).
    /// </summary>
    SenderParamSet = 2,

    /// <summary>
    /// Includes the <c>RECEIVER_PARAMSET</c> field in the returned <see cref="Link"/> structures
    /// (<c>GL_FLAG_RECEIVER_PARAMSET</c>).
    /// </summary>
    ReceiverParamSet = 4
}
