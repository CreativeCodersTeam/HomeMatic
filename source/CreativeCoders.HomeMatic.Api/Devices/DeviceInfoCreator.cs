using AutoMapper;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Api.Devices;

public static class DeviceInfoCreator
{
    private static readonly IMapper Mapper =
        new Mapper(new MapperConfiguration(x => x.CreateMap<DeviceDescription, CcuDeviceInfo>()));
        
    public static CcuDeviceInfo Create(DeviceDescription deviceDescription)
    {
        return Mapper.Map<CcuDeviceInfo>(deviceDescription);
    }
}