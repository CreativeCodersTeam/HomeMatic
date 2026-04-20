using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the requested operation is not supported by the CCU or the target device.
/// </summary>
public class OperationNotSupportedException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationNotSupportedException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public OperationNotSupportedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
