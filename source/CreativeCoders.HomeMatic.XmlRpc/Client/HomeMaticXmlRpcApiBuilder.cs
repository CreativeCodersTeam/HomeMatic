using System.Net.Http;
using CreativeCoders.Core;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.Net.Http;
using CreativeCoders.Net.XmlRpc.Proxy;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client
{
    [PublicAPI]
    public class HomeMaticXmlRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
    {
        private readonly IProxyBuilder<IHomeMaticXmlRpcApi> _proxyBuilder;
        
        private readonly IClassFactory<IHttpClient> _httpClientFactory;

        private string _url;

        public HomeMaticXmlRpcApiBuilder(IProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder, IClassFactory<IHttpClient> httpClientFactory)
        {
            _proxyBuilder = proxyBuilder;
            _httpClientFactory = httpClientFactory;
        }

        public static IHomeMaticXmlRpcApiBuilder Create()
        {
            var httpClient = new HttpClient();
            
            return new HomeMaticXmlRpcApiBuilder(
                new ProxyBuilder<IHomeMaticXmlRpcApi>(), 
                new DelegateClassFactory<IHttpClient>(() => new HttpClientEx(httpClient)));
        }
        
        public IHomeMaticXmlRpcApiBuilder ForUrl(string url)
        {
            _url = url;
            return this;
        }

        public IHomeMaticXmlRpcApi Build()
        {
            return new XmlRpcProxyBuilder<IHomeMaticXmlRpcApi>(_proxyBuilder, _httpClientFactory)
                .ForUrl(_url)
                .Build();
        }
    }
}