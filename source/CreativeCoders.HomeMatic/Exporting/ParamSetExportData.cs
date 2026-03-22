namespace CreativeCoders.HomeMatic.Exporting;

public class ParamSetExportData
{
    public required string ParamSetKey { get; init; }

    public required IEnumerable<ParamValueExportData> Values { get; init; }
}
