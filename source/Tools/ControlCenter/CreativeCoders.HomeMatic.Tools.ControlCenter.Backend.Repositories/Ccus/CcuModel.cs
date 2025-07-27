using CreativeCoders.Data.NoSql;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public class CcuModel : IDocumentKey<string>
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Uri? Url { get; set; }
}
