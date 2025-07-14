namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface IParamSetValueWithDescription
{
    IParamSetValue ParamSetValue { get; }

    string Description { get; }
}
