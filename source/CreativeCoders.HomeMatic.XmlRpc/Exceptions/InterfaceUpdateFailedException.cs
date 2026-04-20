using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the CCU failed to update an interface.
/// </summary>
public class InterfaceUpdateFailedException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceUpdateFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public InterfaceUpdateFailedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
