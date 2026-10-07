using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.Net.Servers.Http;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Http;

/// <summary>
/// Provides an <see cref="IHttpServer"/> based on <see cref="HttpListener"/> that processes requests one at a time.
/// </summary>
/// <remarks>
/// <para>
/// All entries of <see cref="HttpServerBase{THttpContext}.Urls"/> are registered as <see cref="HttpListener"/>
/// prefixes before the listener starts, so bind errors such as a port that is already in use surface as
/// <see cref="HttpListenerException"/> from <see cref="StartAsync"/>.
/// </para>
/// <para>
/// Requests are processed sequentially in arrival order. A request whose processing fails is answered with
/// HTTP 500 and does not end the accept loop. A request that exceeds <see cref="RequestTimeout"/>, or is still in
/// progress when the server stops, is aborted by dropping its connection.
/// </para>
/// <para>
/// A stopped server cannot be started again; create a new instance instead.
/// </para>
/// </remarks>
[PublicAPI]
public sealed class HttpListenerServer : HttpServerBase<HttpListenerContext>, IDisposable
{
    private readonly HttpListener _httpListener = new();

    private CancellationTokenSource? _stopTokenSource;

    private Task? _acceptLoopTask;

    private bool _isDisposed;

    /// <summary>
    /// Gets or sets the maximum time the server spends on a single request.
    /// </summary>
    /// <value>
    /// The maximum processing time per request. The default is 30 seconds.
    /// </value>
    /// <remarks>
    /// A request that is not completed in time, for example because the client sent the headers but never the body,
    /// is answered with HTTP 408 where the response has not been started yet, its connection is dropped, and the
    /// server continues with the next request. The handler of an aborted request is not awaited, so it may still be
    /// running while the next request is processed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is less than or equal to <see cref="TimeSpan.Zero"/>, or greater than <see cref="int.MaxValue"/>
    /// milliseconds.
    /// </exception>
    public TimeSpan RequestTimeout
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, TimeSpan.FromMilliseconds(int.MaxValue));
            field = value;
        }
    } = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    /// <exception cref="HttpListenerException">The listener cannot bind to one of the URL prefixes.</exception>
    /// <exception cref="InvalidOperationException">The server has already been started, even if it has been stopped since.</exception>
    /// <exception cref="ObjectDisposedException">The server has been disposed.</exception>
    public override Task StartAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_acceptLoopTask is not null)
        {
            throw new InvalidOperationException("The HTTP server has already been started.");
        }

        foreach (var url in Urls)
        {
            _httpListener.Prefixes.Add(url);
        }

        _httpListener.Start();

        var stopTokenSource = new CancellationTokenSource();
        _stopTokenSource = stopTokenSource;
        _acceptLoopTask = Task.Run(() => RunAcceptLoopAsync(stopTokenSource.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Stops accepting requests, closes the listener and waits until the accept loop has ended.
    /// Calling this method on a server that has not been started or has already been stopped has no effect.
    /// </remarks>
    public override async Task StopAsync()
    {
        var stopTokenSource = Interlocked.Exchange(ref _stopTokenSource, null);
        if (stopTokenSource is null)
        {
            return;
        }

        await stopTokenSource.CancelAsync().ConfigureAwait(false);

        _httpListener.Close();

        if (_acceptLoopTask is not null)
        {
            await _acceptLoopTask.ConfigureAwait(false);
        }

        stopTokenSource.Dispose();
    }

    /// <summary>
    /// Closes the underlying <see cref="HttpListener"/> and releases all resources used by the server.
    /// </summary>
    /// <remarks>
    /// This method does not wait for the accept loop to end; call <see cref="StopAsync"/> first for a graceful stop.
    /// Calling this method more than once has no effect.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, true))
        {
            return;
        }

        Interlocked.Exchange(ref _stopTokenSource, null)?.Cancel();

        _httpListener.Close();
    }

    /// <inheritdoc />
    protected override async Task FlushAndCloseOutputStreamAsync(HttpListenerContext httpContext)
    {
        await httpContext.Response.OutputStream.FlushAsync(CancellationToken.None).ConfigureAwait(false);

        httpContext.Response.Close();
    }

    /// <inheritdoc />
    protected override IHttpRequest GetRequest(HttpListenerContext httpContext)
    {
        return new HttpListenerRequestAdapter(httpContext.Request);
    }

    /// <inheritdoc />
    protected override IHttpResponse GetResponse(HttpListenerContext httpContext)
    {
        return new HttpListenerResponseAdapter(httpContext.Response);
    }

    private async Task RunAcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext httpContext;

            try
            {
                httpContext = await _httpListener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || !_httpListener.IsListening)
            {
                return;
            }
            catch (HttpListenerException)
            {
                // A single failed accept (e.g. a client that aborted the connection) must not end the loop.
                continue;
            }

            await ProcessRequestAsync(httpContext, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext httpContext, CancellationToken cancellationToken)
    {
        Task? handleRequestTask = null;

        try
        {
            handleRequestTask = HandleRequestAsync(httpContext);

            await handleRequestTask.WaitAsync(RequestTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (handleRequestTask is { IsCompleted: false })
        {
            // Timed out or stopping: drop the connection so the stalled request cannot block further requests.
            AbortResponse(httpContext.Response, ex is TimeoutException);
            ObserveFault(handleRequestTask);
        }
        catch (Exception)
        {
            SendInternalServerError(httpContext.Response);
        }
    }

    private static void AbortResponse(HttpListenerResponse response, bool timedOut)
    {
        try
        {
            if (timedOut)
            {
                // The managed HttpListener sends the pending headers when aborting, so they must not claim success.
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            }
        }
        catch (InvalidOperationException)
        {
            // Headers have already been sent; only the connection can be dropped.
        }

        response.Abort();
    }

    private static void ObserveFault(Task task)
    {
        task.ContinueWith(t => _ = t.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static void SendInternalServerError(HttpListenerResponse response)
    {
        try
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Close();
        }
        catch (Exception)
        {
            // Headers may already have been sent or the connection may be gone; drop the connection instead.
            response.Abort();
        }
    }

    private sealed class HttpListenerRequestAdapter(HttpListenerRequest request) : IHttpRequest
    {
        private readonly HttpListenerRequest _request = Ensure.NotNull(request);

        public IHttpRequestBody Body => new StreamRequestBody(_request.InputStream);

        public string? ContentType => _request.ContentType;

        public string HttpMethod => _request.HttpMethod;
    }

    private sealed class HttpListenerResponseAdapter(HttpListenerResponse response) : IHttpResponse
    {
        private readonly HttpListenerResponse _response = Ensure.NotNull(response);

        private StreamResponseBody? _body;

        public string? ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public long? ContentLength
        {
            get => _response.ContentLength64;
            set => _response.ContentLength64 =
                value ?? throw new NotSupportedException("Clearing the content length is not supported.");
        }

        public int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public IHttpResponseBody Body => _body ??= new StreamResponseBody(_response.OutputStream);
    }
}
