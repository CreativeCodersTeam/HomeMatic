using CreativeCoders.Mvvm;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Explorer.ViewModels
{
    [UsedImplicitly]
    public class MainViewModel : ViewModelBase
    {
        private string _title;

        public MainViewModel(ExplorerTreeViewModel explorerTree, MainRibbonViewModel mainRibbon, MainContentViewModel mainContent)
        {
            ExplorerTree = explorerTree;
            MainRibbon = mainRibbon;
            MainContent = mainContent;
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        
        public ExplorerTreeViewModel ExplorerTree { get; }
        
        public MainRibbonViewModel MainRibbon { get; }
        
        public MainContentViewModel MainContent { get; }
    }
}