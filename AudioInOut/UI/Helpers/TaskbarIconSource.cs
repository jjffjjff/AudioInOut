using AudioInOut.DataModel;
using AudioInOut.Interop;
using AudioInOut.Interop.Helpers;
using AudioInOut.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing;

namespace AudioInOut.UI.Helpers
{
    public class TaskbarIconSource : IShellNotifyIconSource
    {
        enum IconKind
        {
            AppIcon,
            AppIcon_LightTheme,
            Muted,
            SpeakerZeroBars,
            SpeakerOneBar,
            SpeakerTwoBars,
            SpeakerThreeBars,
            NoDevice,
        }

        public event Action<IShellNotifyIconSource> Changed;

        public Icon Current { get; private set; }

        private readonly DeviceCollectionViewModel _collection;
        private readonly AppSettings _settings;
        private bool _isMouseOver;
        private string _hash;
        private IconKind _kind;

        public TaskbarIconSource(DeviceCollectionViewModel collection, AppSettings settings)
        {
            _collection = collection;
            _settings = settings;

            _settings.UseLegacyIconChanged += (_, __) => CheckForUpdate();
            collection.TrayPropertyChanged += OnTrayPropertyChanged;

            OnTrayPropertyChanged();
        }

        public void OnMouseOverChanged(bool isMouseOver)
        {
            _isMouseOver = isMouseOver;
            CheckForUpdate();
        }

        public void CheckForUpdate()
        {
            var nextHash = GetHash();
            if (nextHash != _hash)
            {
                Trace.WriteLine($"TaskbarIconSource Changed: {nextHash}");
                _hash = nextHash;
                using (var old = Current)
                {
                    Current = SelectAndLoadIcon(_kind);
                    Changed?.Invoke(this);
                }
            }
        }

        private void OnTrayPropertyChanged()
        {
            _kind = IconKindFromDeviceCollection(_collection);
            CheckForUpdate();
        }

        private Icon SelectAndLoadIcon(IconKind kind)
        {
            if (_settings.UseLegacyIcon)
            {
                kind = IconKind.AppIcon;
            }

            try
            {
                if (System.Windows.SystemParameters.HighContrast)
                {
                    using (var icon = LoadIcon(kind))
                    {
                        return ColorIconForHighContrast(icon, kind, _isMouseOver);
                    }
                }
                else if (SystemSettings.IsSystemLightTheme)
                {
                    if (kind == IconKind.AppIcon)
                    {
                        return LoadIcon(IconKind.AppIcon_LightTheme);
                    }
                    else
                    {
                        using (var icon = LoadIcon(kind))
                        {
                            return ColorIconForLightTheme(icon, kind);
                        }
                    }
                }
                else
                {
                    return LoadIcon(kind);
                }
            }
            // Legacy fallback if SndVolSSD.dll icons are unavailable.
            catch (Exception ex) when (kind != IconKind.AppIcon)
            {
                Trace.WriteLine($"TaskbarIconSource LoadIcon: {ex}");
                return SelectAndLoadIcon(IconKind.AppIcon);
            }
        }

        private static Icon LoadIcon(IconKind kind)
        {
            uint dpi = WindowsTaskbar.Dpi;
            switch (kind)
            {
                case IconKind.AppIcon:
                    return IconHelper.LoadIconForTaskbar((string)App.Current.Resources["AudioInOutIconDark"], dpi);
                case IconKind.AppIcon_LightTheme:
                    return IconHelper.LoadIconForTaskbar((string)App.Current.Resources["AudioInOutIconLight"], dpi);
                case IconKind.Muted:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.Muted), dpi);
                case IconKind.NoDevice:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.NoDevice), dpi);
                case IconKind.SpeakerZeroBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerZeroBars), dpi);
                case IconKind.SpeakerOneBar:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerOneBar), dpi);
                case IconKind.SpeakerTwoBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerTwoBars), dpi);
                case IconKind.SpeakerThreeBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerThreeBars), dpi);
                default: throw new NotImplementedException();
            }
        }

        private string GetHash() =>
            $"kind={_kind} " +
            $"{(System.Windows.SystemParameters.HighContrast ? $"hc=true mouse={_isMouseOver} " : "")}" +
            $"dpi={WindowsTaskbar.Dpi} " +
            $"isSysLight={SystemSettings.IsSystemLightTheme} " +
            $"isLegacy={_settings.UseLegacyIcon}";

        // Only fill part of the icon, so we can preserve the red X.
        private static double GetIconFillPercent(IconKind kind) => kind == IconKind.NoDevice ? 0.4 : 1;

        private static Icon ColorIconForLightTheme(Icon darkIcon, IconKind kind)
        {
            return IconHelper.ColorIcon(darkIcon, GetIconFillPercent(kind), System.Windows.Media.Colors.Black);
        }

        private static Icon ColorIconForHighContrast(Icon darkIcon, IconKind kind, bool isMouseOver)
        {
            return IconHelper.ColorIcon(darkIcon, GetIconFillPercent(kind),
                isMouseOver ? System.Windows.SystemColors.HighlightTextColor : System.Windows.SystemColors.WindowTextColor);
        }

        private static IconKind IconKindFromDeviceCollection(DeviceCollectionViewModel collectionViewModel)
        {
            if (collectionViewModel.Default != null)
            {
                switch (collectionViewModel.Default.IconKind)
                {
                    case DeviceViewModel.DeviceIconKind.Mute:
                        return IconKind.Muted;
                    case DeviceViewModel.DeviceIconKind.Bar0:
                        return IconKind.SpeakerZeroBars;
                    case DeviceViewModel.DeviceIconKind.Bar1:
                        return IconKind.SpeakerOneBar;
                    case DeviceViewModel.DeviceIconKind.Bar2:
                        return IconKind.SpeakerTwoBars;
                    case DeviceViewModel.DeviceIconKind.Bar3:
                        return IconKind.SpeakerThreeBars;
                    default: throw new NotImplementedException();
                }
            }
            return IconKind.NoDevice;
        }
    }
}
