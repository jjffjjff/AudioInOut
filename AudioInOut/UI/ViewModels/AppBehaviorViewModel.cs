using AudioInOut.UI.Helpers;
using Microsoft.Win32;
using System.Diagnostics;

namespace AudioInOut.UI.ViewModels
{
    public class AppBehaviorViewModel : BindableBase
    {
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RunKeyName = "AudioInOut";

        private readonly AppSettings _settings;

        public bool UseLogarithmicVolume
        {
            get => _settings.UseLogarithmicVolume;
            set => _settings.UseLogarithmicVolume = value;
        }

        // Registry-based startup is only valid when running unpackaged.
        // Under MSIX, Process.MainModule.FileName resolves to a version-stamped WindowsApps\ path
        // that breaks after updates. Packaged startup requires a StartupTask manifest entry instead.
        public bool IsStartupSettingEnabled => !App.HasIdentity;

        public bool StartWithWindows
        {
            get
            {
                if (App.HasIdentity) return false;
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                    return key?.GetValue(RunKeyName) != null;
            }
            set
            {
                if (App.HasIdentity) return;
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;
                    if (value)
                    {
                        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                        if (exePath != null) key.SetValue(RunKeyName, exePath);
                    }
                    else
                        key.DeleteValue(RunKeyName, throwOnMissingValue: false);
                }
            }
        }

        public bool FloatingMixerPlaceholderEnabled
        {
            get => _settings.FloatingMixerPlaceholderEnabled;
            set => _settings.FloatingMixerPlaceholderEnabled = value;
        }

        public AppBehaviorViewModel(AppSettings settings)
        {
            _settings = settings;
        }
    }
}
