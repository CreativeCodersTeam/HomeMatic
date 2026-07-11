using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Exporting;

[PublicAPI]
public class DeviceExportOptions
{
    /// <summary>
    /// Whitelist of ParamSet keys to include in the export (e.g. "MASTER", "VALUES").
    /// If empty or null, all ParamSets are exported.
    /// </summary>
    public ICollection<string>? ParamSetWhitelist { get; set; }

    /// <summary>
    /// Whitelist of ParamSetValue names to include in the export (e.g. "BOOST_TIME", "SET_TEMPERATURE").
    /// If empty or null, all ParamSetValues within allowed ParamSets are exported.
    /// </summary>
    public ICollection<string>? ParamValueNameWhitelist { get; set; }

    /// <summary>
    /// Whether to write indented JSON output.
    /// </summary>
    public bool WriteIndented { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the communication links of each channel are emitted
    /// to the export. Links must already be present in the snapshot — see
    /// <see cref="CompleteCcuDeviceBuildOptions.IncludeLinks"/>.
    /// </summary>
    /// <value><see langword="true"/> to emit links; otherwise, <see langword="false"/>. Default is <see langword="false"/>.</value>
    public bool IncludeLinks { get; set; }

    /// <summary>
    /// Gets or sets the flags that should be used when fetching the links during snapshot creation.
    /// This value is intended to be forwarded to <see cref="CompleteCcuDeviceBuildOptions.LinksFlags"/>
    /// by callers that build snapshots specifically for an export.
    /// </summary>
    /// <value>The <see cref="GetLinksFlags"/> value. Default is <see cref="GetLinksFlags.None"/>.</value>
    public GetLinksFlags LinksFlags { get; set; } = GetLinksFlags.None;

    /// <summary>
    /// Determines whether a ParamSet key is allowed based on the <see cref="ParamSetWhitelist"/>.
    /// </summary>
    /// <param name="paramSetKey">The ParamSet key to check.</param>
    /// <returns><c>true</c> if the key is allowed or no whitelist is configured; otherwise <c>false</c>.</returns>
    public bool IsParamSetAllowed(string paramSetKey)
    {
        return WhitelistFilter.IsAllowed(ParamSetWhitelist, paramSetKey);
    }

    /// <summary>
    /// Determines whether a ParamSetValue name is allowed based on the <see cref="ParamValueNameWhitelist"/>.
    /// </summary>
    /// <param name="paramValueName">The ParamSetValue name to check.</param>
    /// <returns><c>true</c> if the name is allowed or no whitelist is configured; otherwise <c>false</c>.</returns>
    public bool IsParamValueNameAllowed(string paramValueName)
    {
        return WhitelistFilter.IsAllowed(ParamValueNameWhitelist, paramValueName);
    }

    /// <summary>
    /// Builds a <see cref="CompleteCcuDeviceBuildOptions"/> matching this export configuration.
    /// </summary>
    /// <returns>A <see cref="CompleteCcuDeviceBuildOptions"/> that includes links iff <see cref="IncludeLinks"/> is set
    /// and forwards the <see cref="ParamSetWhitelist"/> so filtered ParamSets are not fetched at all.</returns>
    public CompleteCcuDeviceBuildOptions ToBuildOptions()
    {
        return new CompleteCcuDeviceBuildOptions
        {
            IncludeLinks = IncludeLinks,
            LinksFlags = LinksFlags,
            ParamSetWhitelist = ParamSetWhitelist
        };
    }
}
