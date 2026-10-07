# Contract: Library API changes

**Research**: [../research.md](../research.md) (R1, R2, R6, R8)

## 1. `ICcuEventHandler` — breaking (`CreativeCoders.HomeMatic.XmlRpc.Server`)

Every callback gets the `interfaceId` the CCU sends back. The id is the one the client gave in `init`.

```csharp
public interface ICcuEventHandler
{
    Task Event(string interfaceId, string address, string valueKey, object value);
    Task NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions);
    Task DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions);
    Task UpdateDevice(string interfaceId, string address, int hint);
}
```

- **FACT** — current signatures (`Server/ICcuEventHandler.cs:23,33,41,52`): `Event(string address, string valueKey, object value)`, `NewDevices(DeviceDescription[])`, `DeleteDevices(DeviceDescription[])`, `UpdateDevice(string address, int hint)`. Only `interfaceId` is added in front; return types and the other parameters stay the same.
- `CcuXmlRpcEventServer` passes the `interfaceId` it already receives.
- Handlers must not throw. A throwing handler makes the whole `system.multicall` batch fail with HTTP 500.

## 2. `HttpListenerServer` — new (`CreativeCoders.HomeMatic.XmlRpc.Server.Http`)

```csharp
public sealed class HttpListenerServer : HttpServerBase<HttpListenerContext>, IDisposable
{
    // StartAsync: adds all Urls as prefixes, then starts the listener.
    //   Bind errors (port in use, access denied) surface as HttpListenerException from StartAsync.
    // StopAsync: cancels the accept loop and closes the listener; never throws when already stopped.
    // Requests are processed sequentially; a failing request is answered with HTTP 500 and does not end the loop.
}
```

## 3. Event server factory — new

`XmlRpcServer` cannot be resolved from DI: its `bool` constructor parameter cannot be resolved, and nothing registers an `IHttpServer`. A factory does the wiring:

```csharp
public interface ICcuXmlRpcEventServerFactory
{
    /// listenUrl: HttpListener prefix, e.g. "http://+:53817/"
    ICcuXmlRpcEventServer Create(string listenUrl);
}
```

- The implementation creates `new XmlRpcServer(new HttpListenerServer(), disposeHttpServer: true)` with `Encoding = Encoding.Latin1`, wraps it in `CcuXmlRpcEventServer`, and sets `ServerUrl = listenUrl`.
- Registered in `AddHomeMaticXmlRpc()`.
- The returned server must release the listener when stopped or disposed. `ICcuXmlRpcEventServer` gains `IAsyncDisposable`, or the factory returns a disposable wrapper; this is decided in tasks.

## 4. `DeviceDetails.Channels` — additive (`CreativeCoders.HomeMatic.JsonRpc.Models`)

```csharp
public class DeviceDetails
{
    // existing members unchanged
    public ChannelDetails[]? Channels { get; set; }
}

public class ChannelDetails
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}
```

Filled from the `channels` array of `Device.listAllDetail` (R6, UNVERIFIED shape). If the array is missing, `Channels` stays `null`.
