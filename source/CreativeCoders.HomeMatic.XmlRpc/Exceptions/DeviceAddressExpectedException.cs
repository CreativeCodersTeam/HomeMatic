using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

public class DeviceAddressExpectedException : CcuXmlRpcException
{
    public DeviceAddressExpectedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}