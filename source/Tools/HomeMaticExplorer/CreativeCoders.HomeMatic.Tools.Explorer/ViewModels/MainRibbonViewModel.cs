﻿using System.Threading.Tasks;
using System.Windows.Input;
using CreativeCoders.Core;
using CreativeCoders.Di;
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

        private async Task OnExecuteAddCcu()
        {
            var addCcuViewModel = _classFactory.Create<AddCcuViewModel>();

            if (_windowManager.ShowDialog(addCcuViewModel) == true)
            {
                
            }
        }

        public ICommand AddCcuCommand { get; }
    }
}