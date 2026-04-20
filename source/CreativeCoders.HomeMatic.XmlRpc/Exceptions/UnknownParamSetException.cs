using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class UnknownParamSetException : CcuXmlRpcException
{
    public UnknownParamSetException(string message, Exception faultException) : base(message, faultException)
    {
    }
}