using System;
using CreativeCoders.Core;
using CreativeCoders.Net.XmlRpc.Proxy;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

[PublicAPI]
public class HomeMaticXmlRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
{
    private readonly IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> _proxyBuilder;
        
    private Uri? _url;

    public HomeMaticXmlRpcApiBuilder(IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder)
    {
        Ensure.IsNotNull(proxyBuilder, nameof(proxyBuilder));
            
        _proxyBuilder = proxyBuilder;
    }
        
    public IHomeMaticXmlRpcApiBuilder ForUrl(Uri url)
    {
        _url = Ensure.NotNull(url);

        return this;
    }

    public IHomeMaticXmlRpcApi Build()
    {
        if (_url == null)
        {
            throw new InvalidOperationException("No url specified");
        }
            
        return _proxyBuilder
            .ForUrl(_url)
            .Build();
    }
}