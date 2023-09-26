using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class HomeMaticCcuConnectionInfo
{
    public HomeMaticCcuConnectionInfo(string? name, Uri url)
    {
        Url = Ensure.NotNull(url);
        Name = name ?? url.ToString();
    }
    
    public string? Name { get; }

    public Uri? Url { get; }

    public HomeMaticDeviceSystem Systems { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
}