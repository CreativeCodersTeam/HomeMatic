using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions
{
    public class GeneralException : CcuXmlRpcException
    {
        public GeneralException(string message, Exception faultException) : base(message, faultException)
        {
        }
    }
}