using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

[PublicAPI]
public abstract class CcuXmlRpcException : HomeMaticException
{
    protected CcuXmlRpcException(string message, Exception faultException) : base(message, faultException)
    {
    }
}