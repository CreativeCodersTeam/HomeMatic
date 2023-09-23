using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

[PublicAPI]
public interface IHomeMaticXmlRpcApiBuilder
{
    IHomeMaticXmlRpcApiBuilder ForUrl(Uri url);

    IHomeMaticXmlRpcApi Build();
}