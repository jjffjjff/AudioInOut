using AudioInOut.UI.ViewModels;
using AudioInOut.Actions.DataModel.Serialization;

namespace AudioInOut.Actions.ViewModel
{
    class EveryAppViewModel : SettingsAppItemViewModel
    {
        public EveryAppViewModel()
        {
            DisplayName = Properties.Resources.EveryAppText;
            Id = AppRef.EveryAppId;
        }
    }
}
