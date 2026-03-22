namespace CreativeCoders.HomeMatic.Core.Devices;

public class ParamSetValueWithDescription
{
    public required ParamSetValue ParamSetValue { get; init; }

    public required CcuParameterDescription Description { get; init; }
}
