using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

public class ParamSetValuesWithDescriptions
{
    public required string ParamSetKey { get; init; }

    public required IEnumerable<ParamSetValueWithDescription> ParamSetValues { get; init; }
}
