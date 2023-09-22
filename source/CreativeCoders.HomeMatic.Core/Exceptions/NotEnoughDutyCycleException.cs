using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

public class NotEnoughDutyCycleException : CcuXmlRpcException
{
    public NotEnoughDutyCycleException(string message, Exception faultException) : base(message, faultException)
    {
    }
}