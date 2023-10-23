using CreativeCoders.SysConsole.Cli.Parsing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ShowDeviceDetails;

public class ShowDeviceDetailsOptions
{
    [OptionValue(0)]
    public string Address { get; set; }
}
