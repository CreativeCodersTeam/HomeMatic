using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// The exception that is thrown when a previous transmission to the target device is still outstanding.
/// </summary>
public class TransmissionOutstandingException : CcuXmlRpcException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransmissionOutstandingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    public TransmissionOutstandingException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
