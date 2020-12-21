using System;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Api.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Api
{
    public class CcuConnectionBuilder : ICcuConnectionBuilder
    {
        private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder;
        
        private string _ccuAddress;

        public CcuConnectionBuilder(IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder)
        {
            Ensure.IsNotNull(xmlRpcApiBuilder, nameof(xmlRpcApiBuilder));
            
            _xmlRpcApiBuilder = xmlRpcApiBuilder;
        }
        
        public ICcuConnectionBuilder ForAddress(string ccuAddress)
        {
            if (_ccuAddress != null)
            {
                throw new InvalidOperationException("CCU address is already set");
            }
            
            Ensure.IsNotNullOrWhitespace(ccuAddress, nameof(ccuAddress));
            
            _ccuAddress = ccuAddress;

            return this;
        }

        public ICcuConnection Build()
        {
            return new CcuConnection(
                _xmlRpcApiBuilder
                    .ForUrl(_ccuAddress)
                    .Build());
        }
    }
}