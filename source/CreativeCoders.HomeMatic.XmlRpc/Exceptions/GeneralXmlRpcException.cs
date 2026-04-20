using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class GeneralException : CcuXmlRpcException
{
    public GeneralException(string message, Exception faultException) : base(message, faultException)
    {
    }
}