using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when the CCU cannot execute an operation because the available radio duty cycle is exhausted.
/// </summary>
public class NotEnoughDutyCycleException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotEnoughDutyCycleException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public NotEnoughDutyCycleException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
