using System;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Combines a CCU base URL with a HomeMatic device system to produce the final XML-RPC API endpoint URL.
/// </summary>
/// <remarks>
/// The HomeMatic CCU exposes separate XML-RPC servers on different ports for each device system.
/// BidCoS-RF (wireless) uses port 2001, HM-IP uses port 2010 and BidCoS-Wired uses port 2000.
/// This class derives the correct port from the <see cref="DeviceSystems"/> value via
/// <see cref="ToApiUrl"/>.
/// </remarks>
public class XmlRpcApiAddress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcApiAddress"/> class.
    /// </summary>
    /// <param name="baseUrl">The base URL of the HomeMatic CCU (host and scheme, without port).</param>
    /// <param name="deviceSystems">One of the enumeration values that specifies the target HomeMatic device system.</param>
    public XmlRpcApiAddress(Uri baseUrl, HomeMaticDeviceSystems deviceSystems)
    {
        BaseUrl = Ensure.NotNull(baseUrl);
        DeviceSystems = deviceSystems;
    }

    /// <summary>
    /// Builds the final XML-RPC API URL by combining <see cref="BaseUrl"/> with the port derived from <see cref="DeviceSystems"/>.
    /// </summary>
    /// <returns>The complete URL to the XML-RPC endpoint for the configured device system.</returns>
    public Uri ToApiUrl()
    {
        var uriBuilder = new UriBuilder(BaseUrl);
        uriBuilder.Port = DeviceSystems.ToPort();

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Gets the base URL of the HomeMatic CCU.
    /// </summary>
    /// <value>The base URL without port (e.g. <c>http://192.168.1.100/</c>).</value>
    public Uri BaseUrl { get; }

    /// <summary>
    /// Gets the HomeMatic device system that determines the target XML-RPC port.
    /// </summary>
    /// <value>One of the <see cref="HomeMaticDeviceSystems"/> values.</value>
    public HomeMaticDeviceSystems DeviceSystems { get; }
}
