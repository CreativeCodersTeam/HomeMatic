using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public static class ServiceCollectionExtensions
{
    public static ILiteDbSetupBuilder AddLiteDbObjectRepository(this IServiceCollection services, string dbConnectionString)
    {
        services.TryAddSingleton<ILiteDatabase>(_ => new LiteDatabase(dbConnectionString));

        return new LiteDbSetupBuilder(services);
    }
}