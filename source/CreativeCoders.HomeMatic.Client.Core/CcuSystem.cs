using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class CcuSystem
{
    public CcuSystem(string name)
    {
        Name = Ensure.NotNull(name);
    }
    
    public HomeMaticSystem HomeMaticSystem { get; init; }

    public string Name { get; }
}
