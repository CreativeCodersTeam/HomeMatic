using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Optional configuration for building an <see cref="Core.Devices.ICompleteCcuDevice"/> snapshot.
/// </summary>
[PublicAPI]
public class CompleteCcuDeviceBuildOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the communication links of each channel are fetched
    /// from the CCU and stored in the snapshot.
    /// </summary>
    /// <value><see langword="true"/> to include links; otherwise, <see langword="false"/>. Default is <see langword="false"/>.</value>
    public bool IncludeLinks { get; set; }

    /// <summary>
    /// Gets or sets the flags forwarded to <c>getLinks</c> when <see cref="IncludeLinks"/> is enabled.
    /// </summary>
    /// <value>The <see cref="GetLinksFlags"/> value. Default is <see cref="GetLinksFlags.None"/>.</value>
    public GetLinksFlags LinksFlags { get; set; } = GetLinksFlags.None;

    /// <summary>
    /// Whitelist of ParamSet keys to fetch from the CCU (e.g. "MASTER", "VALUES").
    /// If empty or null, all ParamSets are fetched.
    /// </summary>
    /// <value>The collection of allowed ParamSet keys, or <see langword="null"/> to fetch all.</value>
    public ICollection<string>? ParamSetWhitelist { get; set; }

    /// <summary>
    /// Determines whether a ParamSet key is allowed based on the <see cref="ParamSetWhitelist"/>.
    /// </summary>
    /// <param name="paramSetKey">The ParamSet key to check.</param>
    /// <returns><c>true</c> if the key is allowed or no whitelist is configured; otherwise <c>false</c>.</returns>
    public bool IsParamSetAllowed(string paramSetKey)
    {
        return WhitelistFilter.IsAllowed(ParamSetWhitelist, paramSetKey);
    }
}
