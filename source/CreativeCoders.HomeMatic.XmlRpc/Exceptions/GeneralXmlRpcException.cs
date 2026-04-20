using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the CCU reports a general XML-RPC fault that does not match a more specific type.
/// </summary>
public class GeneralException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public GeneralException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
