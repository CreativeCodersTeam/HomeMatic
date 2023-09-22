namespace CreativeCoders.HomeMatic.Client.Core;

public class HomeMaticCcu
{
    public string Name { get; set; }
    
    public Uri Url { get; set; }

    public HomeMaticSystem Systems { get; set; }
}