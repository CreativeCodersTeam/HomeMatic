using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class OperationNotSupportedException : CcuXmlRpcException
{
    public OperationNotSupportedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}