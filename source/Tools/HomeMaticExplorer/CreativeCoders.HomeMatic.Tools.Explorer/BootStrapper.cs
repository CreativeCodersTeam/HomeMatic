using System.Globalization;
using CreativeCoders.Di.Building;
using CreativeCoders.Di.SimpleInjector;
using CreativeCoders.HomeMatic.Tools.Explorer.Localizations;
using CreativeCoders.Mvvm.Skeletor;
using CreativeCoders.Mvvm.Skeletor.Infrastructure;
using CreativeCoders.Mvvm.Skeletor.Infrastructure.Default;
using CreativeCoders.HomeMatic.Tools.Explorer.ViewModels;
using SimpleInjector;

namespace CreativeCoders.HomeMatic.Tools.Explorer
{
    public class BootStrapper : BootStrapperBase<MainViewModel>
    {
        protected override IDiContainerBuilder CreateDiContainerBuilder()
        {
            return new SimpleInjectorDiContainerBuilder(new Container());
        }
        
        protected void Configure(IViewAttributeInitializer viewAttributeInitializer)
        {
            Resource.Culture = CultureInfo.GetCultureInfo("en-US");
            Resource.Culture = CultureInfo.CurrentCulture;
            
            viewAttributeInitializer.InitFromAllAssemblies();
            ConfigureShell(x => x.Title = Resource.HomeMaticExplorer);
        }
        
        protected override void ConfigureDiContainer(IDiContainerBuilder containerBuilder)
        {
            containerBuilder.SetupDefaultInfrastructure();
            containerBuilder.AddTransient<ExplorerTreeViewModel>();
        }
    }
}