using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.List;

[UsedImplicitly]
public class ListDevicesOptions
{
    [OptionValue(0)]
    public string FilterPattern { get; set; } = string.Empty;

    [OptionParameter('s', "sort")]
    public string SortField { get; set; } = string.Empty;
}
