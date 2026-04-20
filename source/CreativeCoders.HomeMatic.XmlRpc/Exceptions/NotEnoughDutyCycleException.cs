using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class NotEnoughDutyCycleException : CcuXmlRpcException
{
    public NotEnoughDutyCycleException(string message, Exception faultException) : base(message, faultException)
    {
    }
}