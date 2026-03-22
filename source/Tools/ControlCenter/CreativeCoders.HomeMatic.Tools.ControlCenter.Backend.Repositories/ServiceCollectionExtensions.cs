using CreativeCoders.Core.IO;
using CreativeCoders.Data.NoSql.LiteDb;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories;

public static class ServiceCollectionExtensions
{
    public static void AddHmccRepositories(this IServiceCollection services)
    {
        services.AddLiteDbDocumentRepositories(FileSys.Path.Combine(FileSys.Path.GetTempPath(), "hmcc-backend.db"))
            .AddRepository<CcuModel, string>(indexBuilder =>
                indexBuilder
                    .AddIndex(x => x.Name, true)
                    .AddIndex(x => x.Url, true));
        
        services.TryAddScoped<ICcuRepository, ObjectCcuRepository>();
    }
}
