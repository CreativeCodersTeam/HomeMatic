using CreativeCoders.SysConsole.Cli.Parsing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ShowDeviceDetails;

public class ShowDeviceDetailsOptions
{
    [OptionValue(0, IsRequired = true)]
    public string Address { get; set; }
}
