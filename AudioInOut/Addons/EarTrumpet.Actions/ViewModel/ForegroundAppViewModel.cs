using AudioInOut.UI.ViewModels;
using AudioInOut.Actions.DataModel.Serialization;

namespace AudioInOut.Actions.ViewModel
{
    class ForegroundAppViewModel : SettingsAppItemViewModel
    {
        public ForegroundAppViewModel()
        {
            Id = AppRef.ForegroundAppId;
            DisplayName = Properties.Resources.ForegroundAppText;
        }
    }
}
