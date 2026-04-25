using System.Collections.Concurrent;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Default thread-safe implementation of <see cref="T:CreativeCoders.HomeMatic.Core.ICcuRoutingTable">ICcuRoutingTable</see> backed by a
/// <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2">ConcurrentDictionary{TKey,TValue}</see>.
/// </summary>
public class CcuRoutingTable : ICcuRoutingTable
{
    private readonly ConcurrentDictionary<string, ICcuClient> _routes = new();

    /// <inheritdoc />
    public bool TryGetClient(string address, out ICcuClient? client)
    {
        Ensure.IsNotNullOrWhitespace(address);

        return _routes.TryGetValue(address, out client);
    }

    /// <inheritdoc />
    public void Register(string address, ICcuClient client)
    {
        Ensure.IsNotNullOrWhitespace(address);
        Ensure.NotNull(client);

        _routes[address] = client;
    }

    /// <inheritdoc />
    public void Register(IEnumerable<KeyValuePair<string, ICcuClient>> entries)
    {
        foreach (var entry in Ensure.NotNull(entries))
        {
            Register(entry.Key, entry.Value);
        }
    }

    /// <inheritdoc />
    public void Invalidate(string address)
    {
        Ensure.IsNotNullOrWhitespace(address);

        _routes.TryRemove(address, out _);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _routes.Clear();
    }
}
