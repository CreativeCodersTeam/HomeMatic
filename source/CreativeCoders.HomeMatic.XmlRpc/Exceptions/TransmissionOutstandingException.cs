using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class TransmissionOutstandingException : CcuXmlRpcException
{
    public TransmissionOutstandingException(string message, Exception faultException) : base(message, faultException)
    {
    }
}