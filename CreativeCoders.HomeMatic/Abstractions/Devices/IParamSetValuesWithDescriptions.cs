namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public class ParamSetValuesWithDescriptions
{
    public required string ParamSetKey { get; init; }

    public required IEnumerable<ParamSetValueWithDescription> ParamSetValues { get; init; }
}
