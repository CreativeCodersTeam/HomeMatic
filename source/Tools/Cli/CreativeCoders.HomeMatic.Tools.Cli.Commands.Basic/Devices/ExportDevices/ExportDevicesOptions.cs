using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ExportDevices;

[UsedImplicitly]
public class ExportDevicesOptions
{
    [OptionValue(0, IsRequired = true)]
    public string? Address { get; set; }

    [OptionParameter('o', "output", HelpText = "Output file")]
    public string? OutputFileName { get; set; }
}
