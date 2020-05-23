namespace CreativeCoders.HomeMatic.Api
{
//    public class CcuBuilder : ICcuBuilder
//    {
//        private readonly ICcuXmlRpcApiBuilder _ccuXmlRpcApiBuilder;
//        
//        private string _ccuAddress;
//
//        public CcuBuilder(ICcuXmlRpcApiBuilder ccuXmlRpcApiBuilder)
//        {
//            Ensure.IsNotNull(ccuXmlRpcApiBuilder, nameof(ccuXmlRpcApiBuilder));
//            
//            _ccuXmlRpcApiBuilder = ccuXmlRpcApiBuilder;
//        }
//
//        public ICcuBuilder ForAddress(string ccuAddress)
//        {
//            Ensure.IsNotNullOrWhitespace(ccuAddress, nameof(ccuAddress));
//            
//            _ccuAddress = ccuAddress;
//            return this;
//        }
//
//        public ICcu Build()
//        {
//            return new Ccu(_ccuXmlRpcApiBuilder, _ccuAddress);
//        }
//    }
}