using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core
{
    [PublicAPI]
    public interface ICcu
    {
        string Address { get; }
    }
}