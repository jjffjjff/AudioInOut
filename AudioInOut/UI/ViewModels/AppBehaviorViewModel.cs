using AudioInOut.UI.Helpers;
using Microsoft.Win32;
using System.Collections.Generic;
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

        public bool StartWithWindows
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                    return key?.GetValue(RunKeyName) != null;
            }
            set
            {
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

        public List<string> FloatingMixerLayoutOptions { get; } = new List<string> { "Horizontal", "Vertical" };

        public string FloatingMixerDefaultLayout
        {
            get => _settings.FloatingMixerDefaultLayout;
            set
            {
                _settings.FloatingMixerDefaultLayout = value;
                RaisePropertyChanged(nameof(FloatingMixerDefaultLayout));
            }
        }

        public List<string> FloatingMixerSourcesOptions { get; } = new List<string> { "All", "Only active" };

        public string FloatingMixerDefaultSources
        {
            get => _settings.FloatingMixerDefaultSources;
            set
            {
                _settings.FloatingMixerDefaultSources = value;
                RaisePropertyChanged(nameof(FloatingMixerDefaultSources));
            }
        }

        public AppBehaviorViewModel(AppSettings settings)
        {
            _settings = settings;
        }
    }
}
