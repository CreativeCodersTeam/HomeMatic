using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public class CcuModel : IObjectKey<string>
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string? Url { get; set; }
}
