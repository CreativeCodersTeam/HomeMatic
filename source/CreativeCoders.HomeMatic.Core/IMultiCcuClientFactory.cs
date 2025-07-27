namespace CreativeCoders.HomeMatic.Core;

public interface IMultiCcuClientFactory
{
    IMultiCcuClientFactory AddCcu(string ccuName, string host, string userName, string password,
        params CcuDeviceKind[] deviceKinds);

    IMultiCcuClient Build();
}
