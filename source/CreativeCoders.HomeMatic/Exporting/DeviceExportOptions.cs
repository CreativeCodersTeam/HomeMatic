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
    /// Determines whether a ParamSet key is allowed based on the <see cref="ParamSetWhitelist"/>.
    /// </summary>
    /// <param name="paramSetKey">The ParamSet key to check.</param>
    /// <returns><c>true</c> if the key is allowed or no whitelist is configured; otherwise <c>false</c>.</returns>
    public bool IsParamSetAllowed(string paramSetKey)
    {
        if (ParamSetWhitelist is null || ParamSetWhitelist.Count == 0)
        {
            return true;
        }

        return ParamSetWhitelist.Contains(paramSetKey, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether a ParamSetValue name is allowed based on the <see cref="ParamValueNameWhitelist"/>.
    /// </summary>
    /// <param name="paramValueName">The ParamSetValue name to check.</param>
    /// <returns><c>true</c> if the name is allowed or no whitelist is configured; otherwise <c>false</c>.</returns>
    public bool IsParamValueNameAllowed(string paramValueName)
    {
        if (ParamValueNameWhitelist is null || ParamValueNameWhitelist.Count == 0)
        {
            return true;
        }

        return ParamValueNameWhitelist.Contains(paramValueName, StringComparer.OrdinalIgnoreCase);
    }
}
