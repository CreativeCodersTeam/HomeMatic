using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

/// <summary>
/// Test <see cref="HttpMessageHandler"/> that returns a queued response per request and records
/// the requests it receives.
/// </summary>
internal sealed class QueueingHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new();

    public List<RecordedRequest> Requests { get; } = [];

    public void EnqueueJsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    public void EnqueueBinaryResponse(byte[] payload, string fileName, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ =>
        {
            var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return new HttpResponseMessage(statusCode) { Content = content };
        });
    }

    public void EnqueueResponse(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responders.Enqueue(responder);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, body));

        if (_responders.Count == 0)
        {
            throw new InvalidOperationException("No more responses queued.");
        }

        return _responders.Dequeue()(request);
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri Uri, string Body);
