using CreativeCoders.Core;
using LiteDB;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public class LiteDbObjectRepository<T, TKey> : IObjectRepository<T, TKey>
    where T : class, IObjectKey<TKey>
{
    private readonly ILiteCollection<T> _liteCollection;

    public LiteDbObjectRepository(ILiteCollection<T> liteCollection)
    {
        _liteCollection = Ensure.NotNull(liteCollection);
    }
    
    public Task AddAsync(T item)
    {
        _liteCollection.Insert(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TKey id)
    {
        _liteCollection.Delete(new BsonValue(id));
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T item)
    {
        _liteCollection.Update(new BsonValue(item.Id), item);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_liteCollection.FindAll());
    }
}