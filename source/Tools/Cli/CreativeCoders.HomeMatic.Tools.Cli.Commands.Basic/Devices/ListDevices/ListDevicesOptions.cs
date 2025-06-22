using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ListDevices;

[UsedImplicitly]
public class ListDevicesOptions
{
    [OptionValue(0)] public string FilterPattern { get; set; } = string.Empty;
}
