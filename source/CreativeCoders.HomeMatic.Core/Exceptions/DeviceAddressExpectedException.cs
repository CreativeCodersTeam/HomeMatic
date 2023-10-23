using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions;

public class DeviceAddressExpectedException : CcuXmlRpcException
{
    public DeviceAddressExpectedException(string message, Exception faultException) : base(message, faultException)
    {
    }
}