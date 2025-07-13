using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface IMultiCcuClientFactory
{
    IMultiCcuClientFactory AddCcu(string host, string userName, string password, params CcuDeviceKind[] deviceKinds);

    IMultiCcuClient Build();
}
