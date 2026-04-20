using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class UnknownParameterOrValueException : CcuXmlRpcException
{
    public UnknownParameterOrValueException(string message, Exception faultException) : base(message, faultException)
    {
    }
}