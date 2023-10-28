namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public interface ILiteDbSetupBuilder
{
    ILiteDbSetupBuilder AddCollection<T, TKey>(string? name = null) where T : class, IObjectKey<TKey>;
}