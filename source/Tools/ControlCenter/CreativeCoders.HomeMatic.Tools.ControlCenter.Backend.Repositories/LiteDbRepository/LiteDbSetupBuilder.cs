using CreativeCoders.Core;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public class LiteDbSetupBuilder : ILiteDbSetupBuilder
{
    private readonly IServiceCollection _services;

    public LiteDbSetupBuilder(IServiceCollection services)
    {
        _services = Ensure.NotNull(services);
    }

    public ILiteDbSetupBuilder AddCollection<T, TKey>(string? name = null) where T : class, IObjectKey<TKey>
    {
        _services.TryAddScoped<ILiteCollection<T>>(sp =>
        {
            var liteDatabase = sp.GetRequiredService<ILiteDatabase>();
            
            return liteDatabase.GetCollection<T>(name ?? typeof(T).Name);
        });
        
        _services.TryAddScoped<IObjectRepository<T, TKey>, LiteDbObjectRepository<T, TKey>>();
        
        return this;
    }
}