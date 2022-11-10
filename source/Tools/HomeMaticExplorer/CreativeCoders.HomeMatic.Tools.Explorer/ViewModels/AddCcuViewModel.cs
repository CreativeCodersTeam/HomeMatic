using CreativeCoders.Core.Collections;
using CreativeCoders.Mvvm;
using CreativeCoders.Mvvm.Commands;
using CreativeCoders.Mvvm.Skeletor.Infrastructure;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Explorer.ViewModels;

[UsedImplicitly]
public class AddCcuViewModel : ViewModelBase
{
    private string _address;

    public AddCcuViewModel(IWindowManager windowManager)
    {
        OkCommand = new SimpleRelayCommand(() => windowManager.CloseDialog(this, true),
            () => !string.IsNullOrWhiteSpace(Address));
        CancelCommand = new SimpleRelayCommand(() => windowManager.CloseDialog(this, false));

        MruAddresses = new ExtendedObservableCollection<string>();
    }

    public string Address
    {
        get => _address;
        set => Set(ref _address, value);
    }

    public SimpleRelayCommand OkCommand { get; }

    public SimpleRelayCommand CancelCommand { get; }

    public ExtendedObservableCollection<string> MruAddresses { get; }
}