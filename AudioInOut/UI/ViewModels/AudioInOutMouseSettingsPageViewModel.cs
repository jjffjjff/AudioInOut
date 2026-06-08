

namespace AudioInOut.UI.ViewModels
{
    public class AudioInOutMouseSettingsPageViewModel : SettingsPageViewModel
    {
        public bool UseScrollWheelInTray
        {
            get => _settings.UseScrollWheelInTray;
            set => _settings.UseScrollWheelInTray = value;
        }

        public bool UseGlobalMouseWheelHook
        {
            get => _settings.UseGlobalMouseWheelHook;
            set => _settings.UseGlobalMouseWheelHook = value;
        }

        private readonly AppSettings _settings;

        public AudioInOutMouseSettingsPageViewModel(AppSettings settings) : base(null)
        {
            _settings = settings;
            Title = Properties.Resources.MouseSettingsPageText;
            Glyph = "\xE962";
        }
    }
}