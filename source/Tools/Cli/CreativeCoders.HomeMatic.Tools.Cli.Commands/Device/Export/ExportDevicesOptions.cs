using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.Export;

[UsedImplicitly]
public class ExportDevicesOptions
{
    [OptionValue(0, IsRequired = true)]
    public string Address { get; set; } = string.Empty;

    [OptionParameter('o', "output", HelpText = "Output file")]
    public string OutputFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the SERVICE ParamSet is skipped.
    /// </summary>
    /// <value><see langword="true"/> to skip the SERVICE ParamSet; otherwise, <see langword="false"/>.
    /// Default is <see langword="false"/>.</value>
    /// <remarks>
    /// If set, the SERVICE ParamSet is not loaded from the CCU and is not written to the export file.
    /// </remarks>
    [OptionParameter("skip-service-params",
        HelpText = "Skip the SERVICE ParamSet. It is not loaded from the CCU and is not written to the export file")]
    public bool SkipServiceParamSet { get; set; }
}
