using System;
using System.Collections.ObjectModel;

namespace AudioInOut.UI.ViewModels
{
    public interface IFocusedViewModel
    {
        event Action RequestClose;
        string DisplayName { get; }
        ObservableCollection<ToolbarItemViewModel> Toolbar { get; }
        void Closing();
    }
}