using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ShowDeviceDetails;

[PublicAPI]
public class ShowDeviceDetailsOptions : CliCommandOptionsBase
{
    [OptionValue(0, IsRequired = true)]
    public string Address { get; set; } = string.Empty;
}
