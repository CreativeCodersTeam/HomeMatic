using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// Serves as the base class for exceptions that wrap XML-RPC faults returned by the CCU.
/// </summary>
[PublicAPI]
public abstract class CcuXmlRpcException : HomeMaticException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CcuXmlRpcException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="faultException">The original XML-RPC fault exception.</param>
    protected CcuXmlRpcException(string message, Exception faultException) : base(message, faultException)
    {
    }
}
