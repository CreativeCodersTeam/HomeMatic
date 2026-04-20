using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class DeviceOutOfReachException : CcuXmlRpcException
{
    public DeviceOutOfReachException(string message, Exception faultException) : base(message, faultException)
    {
    }
}