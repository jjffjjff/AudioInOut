using AudioInOut.Interop.Helpers;
using AudioInOut.UI.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioInOut.UI.ViewModels
{
    public class HotkeyViewModel : BindableBase
    {
        private string _hotkeyText;
        public string HotkeyText
        {
            get => _hotkeyText;
            set
            {
                if (_hotkeyText != value)
                {
                    _hotkeyText = value;
                    RaisePropertyChanged(nameof(HotkeyText));

                    if (string.IsNullOrWhiteSpace(_hotkeyText))
                    {
                        _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                        _hotkey.Key = System.Windows.Forms.Keys.None;
                    }
                }
            }
        }

        public bool IsHotkeySet => !_hotkey.IsEmpty;

        public HotkeyData Hotkey => _hotkey;

        // Set by AudioInOutShortcutsPageViewModel after all VMs are constructed.
        // Returns name of conflicting action, or null if no conflict.
        public Func<HotkeyData, string> FindConflict { get; set; }

        public string ConflictWarning
        {
            get
            {
                if (_hotkey.IsEmpty || FindConflict == null) return null;
                return FindConflict(_hotkey);
            }
        }

        public ICommand ResetCommand { get; }

        private readonly Action<HotkeyData> _save;
        private HotkeyData _hotkey;
        private HotkeyData _savedHotkey;

        public HotkeyViewModel(HotkeyData hotkey, Action<HotkeyData> save)
        {
            _hotkey = hotkey;
            _savedHotkey = new HotkeyData { Key = hotkey.Key, Modifiers = hotkey.Modifiers };
            _save = save;
            ResetCommand = new RelayCommand(Reset);

            SetHotkeyText();
        }

        public void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key == Key.Tab)
                return;

            e.Handled = true;

            if (key == Key.Escape || key == Key.Back)
            {
                _hotkey.Key = System.Windows.Forms.Keys.None;
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
            }
            else
            {
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                _hotkey.Key = System.Windows.Forms.Keys.None;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    _hotkey.Modifiers = System.Windows.Forms.Keys.Control;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    _hotkey.Modifiers |= System.Windows.Forms.Keys.Shift;

                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    _hotkey.Modifiers |= System.Windows.Forms.Keys.Alt;

                if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
                    _hotkey.Modifiers |= System.Windows.Forms.Keys.LWin;

                if (key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.CapsLock || key == Key.LWin || key == Key.RWin)
                {
                    // modifier-only keypress — wait for main key
                }
                else
                {
                    _hotkey.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
                }
            }
            SetHotkeyText();
        }

        public void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // Disallow modifier-only hotkeys (e.g. Alt+None)
            if (_hotkey.Key == System.Windows.Forms.Keys.None &&
                _hotkey.Modifiers != System.Windows.Forms.Keys.None)
            {
                _hotkey.Modifiers = System.Windows.Forms.Keys.None;
                SetHotkeyText();
            }

            if (_hotkey != _savedHotkey)
            {
                _save(_hotkey);
                _savedHotkey = new HotkeyData { Key = _hotkey.Key, Modifiers = _hotkey.Modifiers };
            }
            HotkeyManager.Current.Resume();
        }

        public void OnGotFocus(object sender, RoutedEventArgs e)
        {
            HotkeyManager.Current.Pause();
            (sender as TextBox)?.SelectAll();
        }

        private void Reset()
        {
            _hotkey.Key = System.Windows.Forms.Keys.None;
            _hotkey.Modifiers = System.Windows.Forms.Keys.None;
            SetHotkeyText();
            _save(_hotkey);
            _savedHotkey = new HotkeyData { Key = _hotkey.Key, Modifiers = _hotkey.Modifiers };
        }

        private void SetHotkeyText()
        {
            HotkeyText = _hotkey.ToString().Replace(System.Windows.Forms.Keys.None.ToString(), "");
            RaisePropertyChanged(nameof(IsHotkeySet));
            RaisePropertyChanged(nameof(ConflictWarning));
        }
    }
}
