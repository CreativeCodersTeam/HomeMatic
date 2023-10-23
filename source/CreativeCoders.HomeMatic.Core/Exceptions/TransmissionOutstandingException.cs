using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

public class TransmissionOutstandingException : CcuXmlRpcException
{
    public TransmissionOutstandingException(string message, Exception faultException) : base(message, faultException)
    {
    }
}