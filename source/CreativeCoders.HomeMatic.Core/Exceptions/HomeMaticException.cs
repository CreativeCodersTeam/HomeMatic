using System;

namespace CreativeCoders.HomeMatic.Core.Exceptions
{
    public abstract class HomeMaticException : Exception
    {
        protected HomeMaticException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}