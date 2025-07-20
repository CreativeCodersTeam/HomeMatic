using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ShowDeviceDetails;

[PublicAPI]
public class ShowDeviceDetailsOptions
{
    [OptionValue(0, IsRequired = true)]
    public string Address { get; set; } = string.Empty;
}
