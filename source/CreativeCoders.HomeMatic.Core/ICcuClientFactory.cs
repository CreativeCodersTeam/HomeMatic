using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core;

public interface ICcuClientFactory
{
    ICcuClient CreateClient(string ccuName, IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName,
        string password);
}
