namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> that returns a single <see cref="HttpClient"/> backed by
/// the given <see cref="HttpMessageHandler"/>.
/// </summary>
internal sealed class SingleHandlerHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient(handler, disposeHandler: false);
    }
}
