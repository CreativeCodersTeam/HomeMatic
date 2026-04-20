using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the CCU does not recognize the requested parameter or the provided value is invalid.
/// </summary>
public class UnknownParameterOrValueException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownParameterOrValueException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public UnknownParameterOrValueException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
