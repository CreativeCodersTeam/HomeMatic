namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface IParamSetValuesWithDescriptions
{
    string ParamSetKey { get; }

    IEnumerable<IParamSetValueWithDescription> ParamSetValues { get; }
}
