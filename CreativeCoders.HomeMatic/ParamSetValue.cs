using CreativeCoders.HomeMatic.Abstractions;

namespace CreativeCoders.HomeMatic;

public class ParamSetValue : IParamSetValue
{
    public required string Name { get; init; }

    public required object Value { get; init; }
}
