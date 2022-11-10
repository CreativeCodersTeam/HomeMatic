using System;
using CreativeCoders.Core;
using CreativeCoders.Net.XmlRpc.Proxy;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

[PublicAPI]
public class HomeMaticXmlRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
{
    private readonly IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> _proxyBuilder;
        
    private string _url;

    public HomeMaticXmlRpcApiBuilder(IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder)
    {
        Ensure.IsNotNull(proxyBuilder, nameof(proxyBuilder));
            
        _proxyBuilder = proxyBuilder;
    }

    private HomeMaticXmlRpcApiBuilder(IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder, string url) : this(proxyBuilder)
    {
        _url = Ensure.IsNotNullOrWhitespace(url, nameof(url));
    }

    public IHomeMaticXmlRpcApiBuilder ForUrl(string url)
    {
        Ensure.IsNotNullOrWhitespace(url, nameof(url));

        return new HomeMaticXmlRpcApiBuilder(_proxyBuilder, url);
    }

    public IHomeMaticXmlRpcApi Build()
    {
        if (string.IsNullOrWhiteSpace(_url))
        {
            throw new InvalidOperationException("No url specified");
        }
            
        return _proxyBuilder
            .ForUrl(_url)
            .Build();
    }
}