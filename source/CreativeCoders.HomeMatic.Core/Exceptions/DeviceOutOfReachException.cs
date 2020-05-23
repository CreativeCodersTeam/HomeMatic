using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions
{
    public class DeviceOutOfReachException : CcuXmlRpcException
    {
        public DeviceOutOfReachException(string message, Exception faultException) : base(message, faultException)
        {
        }
    }
}