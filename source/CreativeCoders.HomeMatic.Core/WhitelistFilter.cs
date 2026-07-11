using System;
using System.Collections.Generic;
using System.Linq;
using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Provides the shared whitelist semantics used by ParamSet and parameter-value filters.
/// </summary>
[PublicAPI]
public static class WhitelistFilter
{
    /// <summary>
    /// Determines whether a key is allowed based on the given whitelist.
    /// </summary>
    /// <param name="whitelist">The whitelist to check against, or <see langword="null"/> for no filtering.</param>
    /// <param name="key">The key to check. The comparison is case-insensitive.</param>
    /// <returns><see langword="true"/> if the key is contained in the whitelist or the whitelist is
    /// <see langword="null"/> or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsAllowed(ICollection<string>? whitelist, string key)
    {
        Ensure.NotNull(key);

        if (whitelist is null || whitelist.Count == 0)
        {
            return true;
        }

        return whitelist.Contains(key, StringComparer.OrdinalIgnoreCase);
    }
}
