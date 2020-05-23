using System.Threading.Tasks;

namespace CreativeCoders.HomeMatic.XmlRpc.Server
{
    public interface ICcuEventHandler
    {
        Task Event(string address, string valueKey, object value);

        Task NewDevices(DeviceDescription[] deviceDescriptions);

        Task DeleteDevices(DeviceDescription[] deviceDescriptions);

        Task UpdateDevice(string address, int hint);
    }
}