namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public interface IObjectRepository<T, TKey>
    where T : class, IObjectKey<TKey>
{
    Task AddAsync(T item);
    
    Task DeleteAsync(TKey id);
    
    Task UpdateAsync(T item);
    
    Task<IEnumerable<T>> GetAllAsync();
}