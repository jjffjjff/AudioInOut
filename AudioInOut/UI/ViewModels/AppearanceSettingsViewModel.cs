using AudioInOut.UI.Helpers;
using System.Collections.Generic;

namespace AudioInOut.UI.ViewModels
{
    public class AppearanceSettingsViewModel : BindableBase
    {
        public List<string> ThemeOptions { get; } = new List<string> { "Light", "Dark", "System" };

        private string _selectedTheme;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    RaisePropertyChanged(nameof(SelectedTheme));
                    App.Settings.AppearanceTheme = value;
                    App.ApplyIcomTheme(value);
                }
            }
        }

        public AppearanceSettingsViewModel()
        {
            _selectedTheme = App.Settings.AppearanceTheme;
        }
    }
}
