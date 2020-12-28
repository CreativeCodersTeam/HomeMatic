using System.Threading.Tasks;
using System.Windows.Input;
using CreativeCoders.Core;
using CreativeCoders.Mvvm;
using CreativeCoders.Mvvm.Commands;
using CreativeCoders.Mvvm.Skeletor.Infrastructure;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Explorer.ViewModels
{
    [UsedImplicitly]
    public class MainRibbonViewModel : ViewModelBase
    {
        private readonly IWindowManager _windowManager;
        
        private readonly IClassFactory _classFactory;

        public MainRibbonViewModel(IWindowManager windowManager, IClassFactory classFactory)
        {
            _windowManager = windowManager;
            _classFactory = classFactory;

            AddCcuCommand = new AsyncSimpleRelayCommand(OnExecuteAddCcu);
        }

        private Task OnExecuteAddCcu()
        {
            var addCcuViewModel = _classFactory.Create<AddCcuViewModel>();

            if (_windowManager.ShowDialog(addCcuViewModel) == true)
            {
                
            }

            return Task.CompletedTask;
        }

        public ICommand AddCcuCommand { get; }
    }
}