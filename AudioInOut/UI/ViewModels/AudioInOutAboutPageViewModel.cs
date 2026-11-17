using AudioInOut.Interop.Helpers;
using AudioInOut.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AudioInOut.UI.ViewModels
{
    class AudioInOutAboutPageViewModel : SettingsPageViewModel
    {
        public ICommand OpenAboutCommand { get; }
        public ICommand OpenFeedbackCommand { get; }
        public ICommand OpenPrivacyPolicyCommand { get; }
        public string AboutText { get; }

        private readonly AppSettings _settings;

        public AudioInOutAboutPageViewModel(AppSettings settings) : base(null)
        {
            _settings = settings;
            Glyph = "\xE946";
            Title = Properties.Resources.AboutTitle;
            AboutText = $"Audio-InOut {App.PackageVersion}";

            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenFeedbackCommand = new RelayCommand(OpenGitHubIssueChooser);
            OpenPrivacyPolicyCommand = new RelayCommand(OpenPrivacyPolicy);
        }

        private void OpenGitHubIssueChooser() => ProcessHelper.StartNoThrow("https://github.com/jjffjjff/AudioInOut/issues");
        private void OpenAbout() { }
        private void OpenPrivacyPolicy() => ProcessHelper.StartNoThrow("https://github.com/jjffjjff/AudioInOut");
    }
}
