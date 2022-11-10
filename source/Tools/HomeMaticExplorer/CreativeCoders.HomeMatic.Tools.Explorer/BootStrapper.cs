using System.Globalization;
using CreativeCoders.HomeMatic.Tools.Explorer.Localizations;
using CreativeCoders.Mvvm.Skeletor;
using CreativeCoders.Mvvm.Skeletor.Infrastructure;
using CreativeCoders.Mvvm.Skeletor.Infrastructure.Default;
using CreativeCoders.HomeMatic.Tools.Explorer.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tools.Explorer;

public class BootStrapper : BootStrapperBase<MainViewModel>
{
    protected void Configure(IViewAttributeInitializer viewAttributeInitializer)
    {
        Resource.Culture = CultureInfo.GetCultureInfo("en-US");
        Resource.Culture = CultureInfo.CurrentCulture;
            
        viewAttributeInitializer.InitFromAllAssemblies();
        ConfigureShell(x => x.Title = Resource.HomeMaticExplorer);
    }
        
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.SetupDefaultInfrastructure();
        services.AddTransient<ExplorerTreeViewModel>();
    }
}