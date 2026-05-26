using System.Collections.ObjectModel;

namespace AudioInOut.Actions.ViewModel
{
    interface IOptionViewModel
    {
        ObservableCollection<Option> All { get; }
        Option Selected { get; set; }
    }
}