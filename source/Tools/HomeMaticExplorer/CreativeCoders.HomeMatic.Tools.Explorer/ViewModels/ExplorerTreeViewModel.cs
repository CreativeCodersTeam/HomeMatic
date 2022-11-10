using CreativeCoders.Core.Collections;
using CreativeCoders.Mvvm;
using CreativeCoders.HomeMatic.Tools.Explorer.ViewModels.Explorer;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Explorer.ViewModels;

[UsedImplicitly]
public class ExplorerTreeViewModel : ViewModelBase
{
    public ExplorerTreeViewModel()
    {
        Devices = new ExtendedObservableCollection<DeviceViewModel>();
    }
        
    public ExtendedObservableCollection<DeviceViewModel> Devices { get; }
}