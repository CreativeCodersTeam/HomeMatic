using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

[PublicAPI]
public interface IHomeMaticXmlRpcApiBuilder
{
    IHomeMaticXmlRpcApiBuilder ForUrl(string url);

    IHomeMaticXmlRpcApi Build();
}