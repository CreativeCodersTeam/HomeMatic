namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Specifies the kind of HomeMatic device addressed on a CCU.
/// </summary>
public enum CcuDeviceKind
{
    /// <summary>
    /// A classic BidCoS-RF (HomeMatic) device.
    /// </summary>
    HomeMatic,

    /// <summary>
    /// A HomeMatic IP device.
    /// </summary>
    HomeMaticIp,

    /// <summary>
    /// A HomeMatic Wired (RS485) device.
    /// </summary>
    HomeMaticWired,

    /// <summary>
    /// A device that is coupled across multiple interfaces.
    /// </summary>
    Coupled
}
