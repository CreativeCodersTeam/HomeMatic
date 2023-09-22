using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

public class UnknownParameterOrValueException : CcuXmlRpcException
{
    public UnknownParameterOrValueException(string message, Exception faultException) : base(message, faultException)
    {
    }
}