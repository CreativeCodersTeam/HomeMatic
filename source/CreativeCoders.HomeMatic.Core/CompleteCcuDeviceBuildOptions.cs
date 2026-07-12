using System;
using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Parameters;
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
    /// Gets or sets a value indicating whether the <see cref="ParamSetKey.Service"/> ParamSet is skipped
    /// instead of being fetched from the CCU.
    /// </summary>
    /// <value><see langword="true"/> to skip the SERVICE ParamSet; otherwise, <see langword="false"/>.
    /// Default is <see langword="false"/>.</value>
    /// <remarks>
    /// Skipping takes precedence over <see cref="ParamSetWhitelist"/>: the SERVICE ParamSet is skipped even
    /// when the whitelist explicitly contains it. Reading the SERVICE ParamSet frequently fails for
    /// battery-powered devices that are not reachable, so skipping it avoids both the request and the resulting
    /// read error.
    /// </remarks>
    public bool SkipServiceParamSet { get; set; }

    /// <summary>
    /// Determines whether a ParamSet may be fetched from the CCU.
    /// </summary>
    /// <param name="paramSetKey">The ParamSet key to check. The comparison is case-insensitive.</param>
    /// <returns><see langword="true"/> if the ParamSet may be fetched; otherwise, <see langword="false"/>.
    /// The SERVICE ParamSet is never allowed while <see cref="SkipServiceParamSet"/> is <see langword="true"/>,
    /// even when the <see cref="ParamSetWhitelist"/> contains it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="paramSetKey"/> is <see langword="null"/>.</exception>
    public bool IsParamSetAllowed(string paramSetKey)
    {
        return ParamSetFilter.IsParamSetAllowed(ParamSetWhitelist, SkipServiceParamSet, paramSetKey);
    }
}
