using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;

namespace CreativeCoders.HomeMatic.Core.Devices;

public class CcuParameterDescription
{
    public required string? Id { get; init; }

    public required object? DefaultValue { get; init; }

    public required object? MinValue { get; init; }

    public required object? MaxValue { get; init; }

    public required string? Type { get; init; }

    public required ParameterDataType DataType { get; init; }

    public required string? Unit { get; init; }

    public required int TabOrder { get; init; }

    public required string? Control { get; init; }

    public required IEnumerable<string> ValuesList { get; init; } = [];

    public required IEnumerable<Dictionary<string, object>> SpecialValues { get; init; } = [];
}
