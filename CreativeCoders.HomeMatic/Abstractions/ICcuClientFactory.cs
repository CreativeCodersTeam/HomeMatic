using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

public interface ICcuClientFactory
{
    ICcuClient CreateClient(IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName, string password);
}
