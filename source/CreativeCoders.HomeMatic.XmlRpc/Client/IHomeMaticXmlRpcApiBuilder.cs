using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

/// <summary>
/// Defines a builder for creating instances of <see cref="IHomeMaticXmlRpcApi"/>.
/// </summary>
[PublicAPI]
public interface IHomeMaticXmlRpcApiBuilder
{
    /// <summary>
    /// Configures the builder to use the specified URL as the XML-RPC endpoint.
    /// </summary>
    /// <param name="url">The full URL of the HomeMatic CCU XML-RPC endpoint.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IHomeMaticXmlRpcApiBuilder ForUrl(Uri url);

    /// <summary>
    /// Configures the builder using an <see cref="XmlRpcApiAddress"/> that combines a base URL with a device system.
    /// </summary>
    /// <param name="apiAddress">The API address that provides the full URL including the port for the target device system.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IHomeMaticXmlRpcApiBuilder ForUrl(XmlRpcApiAddress apiAddress);

    /// <summary>
    /// Builds and returns a configured <see cref="IHomeMaticXmlRpcApi"/> instance.
    /// </summary>
    /// <returns>A ready-to-use <see cref="IHomeMaticXmlRpcApi"/> proxy.</returns>
    /// <exception cref="System.InvalidOperationException">No URL has been configured via <see cref="ForUrl(Uri)"/> or <see cref="ForUrl(XmlRpcApiAddress)"/>.</exception>
    IHomeMaticXmlRpcApi Build();
}