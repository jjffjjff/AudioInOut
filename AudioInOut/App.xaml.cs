using AudioInOut.DataModel.WindowsAudio;
using AudioInOut.Diagnosis;
using AudioInOut.Extensibility;
using AudioInOut.Extensibility.Hosting;
using AudioInOut.Extensions;
using AudioInOut.Integration;
using AudioInOut.Interop;
using AudioInOut.Interop.Helpers;
using AudioInOut.UI.Helpers;
using AudioInOut.UI.ViewModels;
using AudioInOut.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace AudioInOut
{
    public partial class App
    {
        public static bool IsShuttingDown { get; private set; }
        public static bool HasIdentity { get; private set; }
        public static bool HasDevIdentity { get; private set; }
        public static string PackageName { get; private set; }
        public static Version PackageVersion { get; private set; }
        public static TimeSpan Duration => s_appTimer.Elapsed;

        public FlyoutWindow FlyoutWindow { get; private set; }
        public DeviceCollectionViewModel CollectionViewModel { get; private set; }
        public DeviceCollectionViewModel RecordingCollectionViewModel { get; private set; }

        private static readonly Stopwatch s_appTimer = Stopwatch.StartNew();
        private FlyoutViewModel _flyoutViewModel;
        private IOverlayClient _overlayClient;

        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private ErrorReporter _errorReporter;

        public static AppSettings Settings { get; private set; }

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            Exit += (_, __) => IsShuttingDown = true;
            HasIdentity = PackageHelper.CheckHasIdentity();
            HasDevIdentity = PackageHelper.HasDevIdentity();
            PackageVersion = PackageHelper.GetVersion(HasIdentity);
            PackageName = PackageHelper.GetFamilyName(HasIdentity);

            Settings = new AppSettings();
            _errorReporter = new ErrorReporter();

            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    ErrorReporter.LogWarning(ex);
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            _overlayClient = new NoopOverlayClient();
            ((UI.Themes.Manager)Resources["ThemeManager"]).Load();

            var deviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            deviceManager.Loaded += (_, __) => CompleteStartup();
            CollectionViewModel = new DeviceCollectionViewModel(deviceManager, Settings);
            var recordingDeviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Recording);
            RecordingCollectionViewModel = new DeviceCollectionViewModel(recordingDeviceManager, Settings);

            _trayIcon = new ShellNotifyIcon(new TaskbarIconSource(CollectionViewModel, Settings));
            Exit += (_, __) => _trayIcon.IsVisible = false;
            CollectionViewModel.TrayPropertyChanged += () => _trayIcon.SetTooltip(CollectionViewModel.GetTrayToolTip());

            _flyoutViewModel = new FlyoutViewModel(CollectionViewModel, () => _trayIcon.SetFocus(), Settings);
            FlyoutWindow = new FlyoutWindow(_flyoutViewModel);
            // Initialize the FlyoutWindow last because its Show/Hide cycle will pump messages, causing UI frames
            // to be executed, breaking the assumption that startup is complete.
            FlyoutWindow.Initialize();
            var helper = new WindowInteropHelper(FlyoutWindow);
            _overlayClient.Register(helper.Handle, OverlayAppId.Value);
        }

        private void CompleteStartup()
        {
            AddonManager.Load(shouldLoadInternalAddons: HasDevIdentity);
            Exit += (_, __) => AddonManager.Shutdown();
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);

            Settings.FlyoutHotkeyTyped += () => _flyoutViewModel.OpenFlyout(InputType.Keyboard);
            Settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            Settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();
            Settings.AbsoluteVolumeUpHotkeyTyped += AbsoluteVolumeIncrement;
            Settings.AbsoluteVolumeDownHotkeyTyped += AbsoluteVolumeDecrement;
            Settings.RegisterHotkeys();

            _trayIcon.PrimaryInvoke += (_, type) => _flyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, args) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems(), args.Point);
            _trayIcon.TertiaryInvoke += (_, __) => CollectionViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += trayIconScrolled;
            _trayIcon.SetTooltip(CollectionViewModel.GetTrayToolTip());
            _trayIcon.IsVisible = true;
        }

        private void trayIconScrolled(object _, int wheelDelta)
        {
            if (Settings.UseScrollWheelInTray && (!Settings.UseGlobalMouseWheelHook || _flyoutViewModel.State == FlyoutViewState.Hidden))
            {
                var hWndTray = WindowsTaskbar.GetTrayToolbarWindowHwnd();
                var hWndTooltip = User32.SendMessage(hWndTray, User32.TB_GETTOOLTIPS, IntPtr.Zero, IntPtr.Zero);
                User32.SendMessage(hWndTooltip, User32.TTM_POPUP, IntPtr.Zero, IntPtr.Zero);
                
                CollectionViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            }
        }

        private bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont") ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format");
        }

        private void OnCriticalFontLoadFailure()
        {
            Trace.WriteLine($"App OnCriticalFontLoadFailure");

            new Thread(() =>
            {
                if (MessageBox.Show(
                    AudioInOut.Properties.Resources.CriticalFailureFontLookupHelpText,
                    AudioInOut.Properties.Resources.CriticalFailureDialogHeaderText,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Trace.WriteLine($"App OnCriticalFontLoadFailure OK");
                    ProcessHelper.StartNoThrow("https://eartrumpet.app/jmp/fixfonts");
                }
                Environment.Exit(0);
            }).Start();

            // Stop execution because callbacks to the UI thread will likely cause another cascading font error.
            new AutoResetEvent(false).WaitOne();
        }

        private IEnumerable<ContextMenuItem> GetTrayContextMenuItems()
        {
            var ret = new List<ContextMenuItem>();

            ret.Add(new ContextMenuSectionTitle(AudioInOut.Properties.Resources.ContextMenuOutputDevicesTitle));

            var outputDevices = CollectionViewModel.AllDevices
                .OrderBy(x => x.DisplayName)
                .Select(dev => new ContextMenuItem
                {
                    DisplayName = dev.DisplayName,
                    IsChecked = dev.Id == CollectionViewModel.Default?.Id,
                    Command = new RelayCommand(() => dev.MakeDefaultDevice()),
                })
                .ToList();

            if (outputDevices.Any())
                ret.AddRange(outputDevices);
            else
                ret.Add(new ContextMenuItem { DisplayName = AudioInOut.Properties.Resources.ContextMenuNoDevices, IsEnabled = false });

            ret.Add(new ContextMenuSeparator());
            ret.Add(new ContextMenuSectionTitle(AudioInOut.Properties.Resources.ContextMenuInputDevicesTitle));

            var inputDevices = RecordingCollectionViewModel.AllDevices
                .OrderBy(x => x.DisplayName)
                .Select(dev => new ContextMenuItem
                {
                    DisplayName = dev.DisplayName,
                    IsChecked = dev.Id == RecordingCollectionViewModel.Default?.Id,
                    Command = new RelayCommand(() => dev.MakeDefaultDevice()),
                })
                .ToList();

            if (inputDevices.Any())
                ret.AddRange(inputDevices);
            else
                ret.Add(new ContextMenuItem { DisplayName = AudioInOut.Properties.Resources.ContextMenuNoRecordingDevices, IsEnabled = false });

            ret.Add(new ContextMenuSeparator());
            ret.Add(new ContextMenuItem
            {
                DisplayName = AudioInOut.Properties.Resources.ContextMenuSoundSettings,
                Command = new RelayCommand(() => SettingsPageHelper.Open("sound")),
            });
            ret.Add(new ContextMenuItem
            {
                DisplayName = AudioInOut.Properties.Resources.ContextMenuVolumeMixer,
                Command = new RelayCommand(LegacyControlPanelHelper.StartLegacyAudioMixer),
            });
            ret.Add(new ContextMenuItem
            {
                DisplayName = AudioInOut.Properties.Resources.ContextMenuFloatingMixer,
                Command = new RelayCommand(_mixerWindow.OpenOrBringToFront),
            });

            ret.Add(new ContextMenuSeparator());
            ret.Add(new ContextMenuItem
            {
                DisplayName = AudioInOut.Properties.Resources.ContextMenuSettingsTooltip,
                IconGlyph = "\xE713",
                Command = new RelayCommand(_settingsWindow.OpenOrBringToFront),
            });
            ret.Add(new ContextMenuItem
            {
                DisplayName = AudioInOut.Properties.Resources.ContextMenuExitTooltip,
                IconGlyph = "\xE8BB",
                Command = new RelayCommand(Shutdown),
            });

            return ret;
        }

        private Window CreateSettingsExperience()
        {
            var viewModel = new SettingsWindowViewModel(Settings, () => _errorReporter.DisplayDiagnosticData());
            return new SettingsWindow { DataContext = viewModel };
        }

        // Called by the future auth/payment flow once the user has paid for OverlayAppId.Value.
        public void UnlockOverlay()
        {
            _overlayClient?.Unlock(OverlayAppId.Value);
        }

        private Window CreateMixerExperience() => new FullWindow { DataContext = new FullWindowViewModel(CollectionViewModel) };

        private void AbsoluteVolumeIncrement()
        {
            foreach (var device in CollectionViewModel.AllDevices.Where(d => !d.IsMuted || d.IsAbsMuted))
            {
                // in any case this device is not abs muted anymore
                device.IsAbsMuted = false;
                device.IncrementVolume(2);
            }
        }

        private void AbsoluteVolumeDecrement()
        {
            foreach (var device in CollectionViewModel.AllDevices.Where(d => !d.IsMuted))
            {
                // if device is not muted but will be muted by 
                bool wasMuted = device.IsMuted;
                // device.IncrementVolume(-2);
                device.Volume -= 2;
                // if device is muted by this absolute down
                // .IsMuted is not already updated
                if (!wasMuted == (device.Volume <= 0))
                {
                    device.IsAbsMuted = true;
                }
            }
        }
    }
}
