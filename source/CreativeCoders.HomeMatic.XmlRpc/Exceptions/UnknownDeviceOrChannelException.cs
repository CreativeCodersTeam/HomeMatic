using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class UnknownDeviceOrChannelException : CcuXmlRpcException
{
    public UnknownDeviceOrChannelException(string message, Exception faultException) : base(message, faultException)
    {
    }
}