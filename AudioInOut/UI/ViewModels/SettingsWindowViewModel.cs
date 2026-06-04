using AudioInOut.UI.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace AudioInOut.UI.ViewModels
{
    class SettingsWindowViewModel : BindableBase
    {
        public string Title { get; } = Properties.Resources.SettingsWindowText;
        public AppBehaviorViewModel AppBehavior { get; }
        public EarTrumpetMouseSettingsPageViewModel ScrollBehavior { get; }
        public EarTrumpetShortcutsPageViewModel Shortcuts { get; }
        public EarTrumpetAboutPageViewModel About { get; }
        public AppearanceSettingsViewModel Appearance { get; }

        private SettingsDialogViewModel _dialog;
        public SettingsDialogViewModel Dialog
        {
            get => _dialog;
            set
            {
                if (_dialog != value)
                {
                    _dialog = value;
                    RaisePropertyChanged(nameof(Dialog));
                }
            }
        }

        private int _selectedSectionIndex;
        public int SelectedSectionIndex
        {
            get => _selectedSectionIndex;
            set
            {
                if (_selectedSectionIndex != value)
                {
                    _selectedSectionIndex = value;
                    RaisePropertyChanged(nameof(SelectedSectionIndex));
                }
            }
        }

        private WindowViewState _state;

        public SettingsWindowViewModel(AppSettings settings, Action openDiagnostics)
        {
            AppBehavior = new AppBehaviorViewModel(settings);
            ScrollBehavior = new EarTrumpetMouseSettingsPageViewModel(settings);
            Shortcuts = new EarTrumpetShortcutsPageViewModel(settings);
            About = new EarTrumpetAboutPageViewModel(openDiagnostics, settings);
            Appearance = new AppearanceSettingsViewModel();
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            switch (_state)
            {
                case WindowViewState.Open:
                    _state = WindowViewState.Closing;
                    e.Cancel = true;
                    WindowAnimationLibrary.BeginWindowExitAnimation((Window)sender, () =>
                    {
                        _state = WindowViewState.CloseReady;
                        Window.GetWindow((DependencyObject)sender)?.Close();
                    });
                    break;
                case WindowViewState.Closing:
                    e.Cancel = true;
                    break;
                case WindowViewState.CloseReady:
                    break;
            }
        }
    }
}
