namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> that returns a single <see cref="HttpClient"/> backed by
/// the given <see cref="HttpMessageHandler"/>.
/// </summary>
internal sealed class SingleHandlerHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public SingleHandlerHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler, disposeHandler: false);
    }
}
