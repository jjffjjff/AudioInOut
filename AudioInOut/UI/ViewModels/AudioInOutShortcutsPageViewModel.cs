using AudioInOut.Interop.Helpers;
using System.Linq;

namespace AudioInOut.UI.ViewModels
{
    internal class AudioInOutShortcutsPageViewModel : SettingsPageViewModel
    {
        private static readonly string s_hotkeyNoneText = new HotkeyData().ToString();

        public HotkeyViewModel OpenFlyoutHotkey { get; }
        public string DefaultHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenMixerHotkey { get; }
        public string DefaultMixerHotKey => s_hotkeyNoneText;

        public HotkeyViewModel OpenSettingsHotkey { get; }
        public string DefaultSettingsHotKey => s_hotkeyNoneText;

        public HotkeyViewModel AbsoluteVolumeUpHotkey { get; }
        public string DefaultAbsoluteVolumeUpHotkey => s_hotkeyNoneText;

        public HotkeyViewModel AbsoluteVolumeDownHotkey { get; }
        public string DefaultAbsoluteVolumeDownHotkey => s_hotkeyNoneText;

        public AudioInOutShortcutsPageViewModel(AppSettings settings) : base(null)
        {
            Title = Properties.Resources.ShortcutsPageText;
            Glyph = "\xE765";

            OpenFlyoutHotkey = new HotkeyViewModel(settings.FlyoutHotkey, (newHotkey) => settings.FlyoutHotkey = newHotkey);
            OpenMixerHotkey = new HotkeyViewModel(settings.MixerHotkey, (newHotkey) => settings.MixerHotkey = newHotkey);
            OpenSettingsHotkey = new HotkeyViewModel(settings.SettingsHotkey, (newHotkey) => settings.SettingsHotkey = newHotkey);
            AbsoluteVolumeUpHotkey = new HotkeyViewModel(settings.AbsoluteVolumeUpHotkey, (newHotkey) => settings.AbsoluteVolumeUpHotkey = newHotkey);
            AbsoluteVolumeDownHotkey = new HotkeyViewModel(settings.AbsoluteVolumeDownHotkey, (newHotkey) => settings.AbsoluteVolumeDownHotkey = newHotkey);

            WireConflictDetection();
        }

        private struct ShortcutEntry
        {
            public string Label;
            public HotkeyViewModel Vm;
        }

        private void WireConflictDetection()
        {
            var entries = new[]
            {
                new ShortcutEntry { Label = Properties.Resources.SettingsOpenEarTrumpetText,       Vm = OpenFlyoutHotkey },
                new ShortcutEntry { Label = Properties.Resources.SettingsOpenMixerText,            Vm = OpenMixerHotkey },
                new ShortcutEntry { Label = Properties.Resources.SettingsOpenSettingsText,         Vm = OpenSettingsHotkey },
                new ShortcutEntry { Label = Properties.Resources.SettingsAbsoluteVolumeUpText,     Vm = AbsoluteVolumeUpHotkey },
                new ShortcutEntry { Label = Properties.Resources.SettingsAbsoluteVolumeDownText,   Vm = AbsoluteVolumeDownHotkey },
            };

            foreach (var entry in entries)
            {
                var self = entry.Vm;
                self.FindConflict = (hotkey) =>
                    entries
                        .Where(x => x.Vm != self && !x.Vm.Hotkey.IsEmpty && x.Vm.Hotkey.Equals(hotkey))
                        .Select(x => "Already used by \"" + x.Label + "\"")
                        .FirstOrDefault();
            }
        }
    }
}
