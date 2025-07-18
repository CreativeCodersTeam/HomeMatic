namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public class ParamSetValueWithDescription
{
    public required ParamSetValue ParamSetValue { get; init; }

    public required CcuParameterDescription Description { get; init; }
}
