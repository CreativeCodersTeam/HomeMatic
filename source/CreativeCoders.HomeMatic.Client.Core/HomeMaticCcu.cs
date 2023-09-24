using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class HomeMaticCcu
{
    public HomeMaticCcu(string? name, Uri url)
    {
        Url = Ensure.NotNull(url);
        Name = name ?? url.ToString();
    }
    
    public string? Name { get; }

    public Uri? Url { get; }

    public HomeMaticSystem Systems { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
}