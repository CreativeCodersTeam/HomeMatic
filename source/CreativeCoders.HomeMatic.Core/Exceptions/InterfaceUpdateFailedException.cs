using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

public class InterfaceUpdateFailedException : CcuXmlRpcException
{
    public InterfaceUpdateFailedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}