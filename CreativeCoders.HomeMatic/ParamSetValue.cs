using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic;

public class ParamSetValue : IParamSetValue
{
    public required string Name { get; init; }

    public required object Value { get; init; }
}
