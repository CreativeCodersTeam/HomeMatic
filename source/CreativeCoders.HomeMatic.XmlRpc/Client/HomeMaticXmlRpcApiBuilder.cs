using CreativeCoders.Net.XmlRpc.Proxy;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client
{
    [PublicAPI]
    public class HomeMaticXmlRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
    {
        private readonly IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> _proxyBuilder;
        
        private string _url;

        public HomeMaticXmlRpcApiBuilder(IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder)
        {
            _proxyBuilder = proxyBuilder;
        }

        public IHomeMaticXmlRpcApiBuilder ForUrl(string url)
        {
            _url = url;
            return this;
        }

        public IHomeMaticXmlRpcApi Build()
        {
            return _proxyBuilder
                .ForUrl(_url)
                .Build();
        }
    }
}