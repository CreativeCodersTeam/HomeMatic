using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class InterfaceUpdateFailedException : CcuXmlRpcException
{
    public InterfaceUpdateFailedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}