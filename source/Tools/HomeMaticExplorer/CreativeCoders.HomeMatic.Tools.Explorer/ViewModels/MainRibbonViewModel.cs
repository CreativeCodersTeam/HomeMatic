using System.Threading.Tasks;
using System.Windows.Input;
using CreativeCoders.Core;
using CreativeCoders.DependencyInjection;
using CreativeCoders.Mvvm;
using CreativeCoders.Mvvm.Commands;
using CreativeCoders.Mvvm.Skeletor.Infrastructure;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Explorer.ViewModels;

[UsedImplicitly]
public class MainRibbonViewModel : ViewModelBase
{
    private readonly IWindowManager _windowManager;
        
    private readonly IObjectFactory<AddCcuViewModel> _addCcuViewModelFactory;

    public MainRibbonViewModel(IWindowManager windowManager, IObjectFactory<AddCcuViewModel> addCcuViewModelFactory)
    {
        _windowManager = windowManager;
        _addCcuViewModelFactory = addCcuViewModelFactory;

        AddCcuCommand = new AsyncSimpleRelayCommand(OnExecuteAddCcu);
    }

    private Task OnExecuteAddCcu()
    {
        var addCcuViewModel = _addCcuViewModelFactory.CreateInstance();

        if (_windowManager.ShowDialog(addCcuViewModel) == true)
        {
                
        }

        return Task.CompletedTask;
    }

    public ICommand AddCcuCommand { get; }
}