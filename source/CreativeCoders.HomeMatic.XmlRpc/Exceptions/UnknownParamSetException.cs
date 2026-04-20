using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the CCU does not recognize the requested parameter set.
/// </summary>
public class UnknownParamSetException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownParamSetException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public UnknownParamSetException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
