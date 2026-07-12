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

    /// <summary>
    /// Gets or sets a value indicating whether the SERVICE ParamSet is skipped.
    /// </summary>
    /// <value><see langword="true"/> to skip the SERVICE ParamSet; otherwise, <see langword="false"/>.
    /// Default is <see langword="false"/>.</value>
    /// <remarks>
    /// If set, the SERVICE ParamSet is not loaded from the CCU and is not shown. Skipping takes precedence over
    /// <see cref="ParamSets"/>, so a SERVICE entry in the ParamSet whitelist is overridden.
    /// </remarks>
    [OptionParameter("skip-service-params",
        HelpText = "Skip the SERVICE ParamSet. It is not loaded from the CCU and overrides a SERVICE entry in --param-sets")]
    public bool SkipServiceParamSet { get; set; }
}
