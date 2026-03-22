using System;
using CreativeCoders.Core;
using CreativeCoders.Net.XmlRpc.Proxy;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

/// <summary>
/// Builds an <see cref="IHomeMaticXmlRpcApi"/> proxy for a given CCU endpoint URL.
/// </summary>
[PublicAPI]
public class HomeMaticXmlRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
{
    private readonly IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> _proxyBuilder;

    private Uri? _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticXmlRpcApiBuilder"/> class.
    /// </summary>
    /// <param name="proxyBuilder">The underlying XML-RPC proxy builder used to create the API proxy.</param>
    public HomeMaticXmlRpcApiBuilder(IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi> proxyBuilder)
    {
        Ensure.IsNotNull(proxyBuilder);

        _proxyBuilder = proxyBuilder;
    }

    /// <inheritdoc/>
    public IHomeMaticXmlRpcApiBuilder ForUrl(Uri url)
    {
        _url = Ensure.NotNull(url);

        return this;
    }

    /// <inheritdoc/>
    public IHomeMaticXmlRpcApiBuilder ForUrl(XmlRpcApiAddress apiAddress)
    {
        return ForUrl(apiAddress.ToApiUrl());
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No URL has been configured.</exception>
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
