using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.ShowDetails;

[PublicAPI]
public class ShowDeviceDetailsOptions : CliCommandOptionsBase
{
    [OptionValue(0, IsRequired = true)]
    public string Address { get; set; } = string.Empty;

    [OptionParameter('p', "param-sets",
        HelpText = "Comma-separated list of ParamSet keys to show (e.g. MASTER,VALUES). If omitted, all ParamSets are shown")]
    public string ParamSets { get; set; } = string.Empty;
}
