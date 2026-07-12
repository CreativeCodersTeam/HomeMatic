using System;
using System.Collections.Generic;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Provides the shared filter semantics that decide whether a ParamSet may be used.
/// </summary>
/// <remarks>
/// The filter combines two independent rules: skipping the <see cref="ParamSetKey.Service"/> ParamSet and an
/// optional whitelist. Skipping takes precedence, so the SERVICE ParamSet is rejected even when the whitelist
/// explicitly contains it.
/// </remarks>
[PublicAPI]
public static class ParamSetFilter
{
    /// <summary>
    /// Determines whether a ParamSet may be used.
    /// </summary>
    /// <param name="whitelist">The allowed ParamSet keys, or <see langword="null"/> for no whitelist filtering.</param>
    /// <param name="skipServiceParamSet"><see langword="true"/> to reject the <see cref="ParamSetKey.Service"/>
    /// ParamSet regardless of the whitelist; otherwise, <see langword="false"/>.</param>
    /// <param name="paramSetKey">The ParamSet key to check. The comparison is case-insensitive.</param>
    /// <returns><see langword="true"/> if the ParamSet may be used; otherwise, <see langword="false"/>.
    /// The SERVICE ParamSet is never allowed while <paramref name="skipServiceParamSet"/> is
    /// <see langword="true"/>, even when <paramref name="whitelist"/> contains it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="paramSetKey"/> is <see langword="null"/>.</exception>
    public static bool IsParamSetAllowed(ICollection<string>? whitelist, bool skipServiceParamSet,
        string paramSetKey)
    {
        Ensure.NotNull(paramSetKey);

        if (skipServiceParamSet
            && string.Equals(paramSetKey, ParamSetKey.Service, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return WhitelistFilter.IsAllowed(whitelist, paramSetKey);
    }
}
