using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the target device could not be reached by the CCU.
/// </summary>
public class DeviceOutOfReachException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceOutOfReachException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public DeviceOutOfReachException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
