using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuClientFactory
{
    ICcuClient CreateClient(IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName, string password);
}
