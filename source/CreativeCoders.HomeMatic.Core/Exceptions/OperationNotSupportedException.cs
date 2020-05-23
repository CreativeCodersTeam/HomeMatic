using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions
{
    public class OperationNotSupportedException : CcuXmlRpcException
    {
        public OperationNotSupportedException(string message, Exception faultException) : base(message, faultException)
        {
        }
    }
}