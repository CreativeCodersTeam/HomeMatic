using AutoMapper;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Api.Parameters
{
    public static class ParameterInfoCreator
    {
        private static readonly IMapper Mapper =
            new Mapper(new MapperConfiguration(x => x.CreateMap<ParameterDescription, CcuParameterInfo>()));

        public static CcuParameterInfo Create(ParameterDescription parameterDescription)
        {
            return Mapper.Map<CcuParameterInfo>(parameterDescription);
        }
    }
}