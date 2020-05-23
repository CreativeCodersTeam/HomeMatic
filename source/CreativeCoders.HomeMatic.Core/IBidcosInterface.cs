using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core
{
    [PublicAPI]
    public interface IBidcosInterface
    {
        string Address { get; set; }
        
        string Description { get; set; }
        
        bool IsConnected { get; set; }
        
        bool IsDefault { get; set; }
    }
}