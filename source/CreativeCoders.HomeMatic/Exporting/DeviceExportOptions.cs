namespace CreativeCoders.HomeMatic.Exporting;

public class DeviceExportOptions
{
    /// <summary>
    /// Whitelist of ParamSet keys to include in the export (e.g. "MASTER", "VALUES").
    /// If empty or null, all ParamSets are exported.
    /// </summary>
    public ICollection<string>? ParamSetWhitelist { get; set; }

    /// <summary>
    /// Whether to write indented JSON output.
    /// </summary>
    public bool WriteIndented { get; set; } = true;

    public bool IsParamSetAllowed(string paramSetKey)
    {
        if (ParamSetWhitelist == null || ParamSetWhitelist.Count == 0)
        {
            return true;
        }

        return ParamSetWhitelist.Contains(paramSetKey, StringComparer.OrdinalIgnoreCase);
    }
}
