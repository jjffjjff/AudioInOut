using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace AudioInOut.UI.ViewModels
{
    public class DeviceEntryViewModel : BindableBase
    {
        private readonly AppSettings _settings;
        private string _deviceName;
        private bool _isConnected;

        public string DeviceId { get; }

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    RaisePropertyChanged(nameof(DeviceName));
                }
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    RaisePropertyChanged(nameof(IsConnected));
                }
            }
        }

        private string _aliasText;
        public string AliasText
        {
            get => _aliasText;
            set
            {
                if (_aliasText != value)
                {
                    _aliasText = value;
                    RaisePropertyChanged(nameof(AliasText));
                    _settings.SetDeviceAlias(DeviceId, value);
                }
            }
        }

        public bool IsHidden
        {
            get => _settings.IsDeviceHidden(DeviceId);
            set
            {
                _settings.SetDeviceHidden(DeviceId, value);
                RaisePropertyChanged(nameof(IsHidden));
            }
        }

        public bool IsModified =>
            !string.IsNullOrEmpty(_settings.GetDeviceAlias(DeviceId)) || _settings.IsDeviceHidden(DeviceId);

        public DeviceEntryViewModel(string deviceId, string deviceName, AppSettings settings, bool isConnected = true)
        {
            DeviceId = deviceId;
            _deviceName = deviceName;
            _isConnected = isConnected;
            _settings = settings;
            _aliasText = _settings.GetDeviceAlias(deviceId) ?? string.Empty;
        }

        public DeviceEntryViewModel(string deviceId, AppSettings settings)
            : this(deviceId, MakeDisconnectedName(deviceId, settings), settings, false)
        {
        }

        internal static string MakeDisconnectedName(string deviceId, AppSettings settings)
        {
            var name = settings.GetStoredDeviceName(deviceId);
            return string.IsNullOrEmpty(name) ? "(not connected)" : "(not connected) · " + name;
        }
    }

    public class DevicesPageViewModel : BindableBase
    {
        public ObservableCollection<DeviceEntryViewModel> OutputDevices { get; } = new ObservableCollection<DeviceEntryViewModel>();
        public ObservableCollection<DeviceEntryViewModel> InputDevices { get; } = new ObservableCollection<DeviceEntryViewModel>();

        private readonly AppSettings _settings;
        private readonly DeviceCollectionViewModel _outputCollection;
        private readonly DeviceCollectionViewModel _inputCollection;

        public DevicesPageViewModel(DeviceCollectionViewModel outputCollection, DeviceCollectionViewModel inputCollection, AppSettings settings)
        {
            _settings = settings;
            _outputCollection = outputCollection;
            _inputCollection = inputCollection;

            Populate(OutputDevices, outputCollection, isOutput: true);
            Populate(InputDevices, inputCollection, isOutput: false);

            outputCollection.AllDevices.CollectionChanged += (_, e) => OnCollectionChanged(e, OutputDevices, outputCollection);
            inputCollection.AllDevices.CollectionChanged += (_, e) => OnCollectionChanged(e, InputDevices, inputCollection);
        }

        private static int GetSortGroup(DeviceEntryViewModel e)
        {
            if (!e.IsConnected) return 2;
            if (e.IsHidden) return 1;
            return 0;
        }

        private static void ResortDevices(ObservableCollection<DeviceEntryViewModel> target)
        {
            var sorted = target
                .OrderBy(e => GetSortGroup(e))
                .ThenBy(e => e.DeviceName ?? "", System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                int cur = target.IndexOf(sorted[i]);
                if (cur != i) target.Move(cur, i);
            }
        }

        private void AddEntry(ObservableCollection<DeviceEntryViewModel> target, DeviceEntryViewModel entry)
        {
            target.Add(entry);
            entry.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DeviceEntryViewModel.IsHidden) ||
                    e.PropertyName == nameof(DeviceEntryViewModel.IsConnected))
                    ResortDevices(target);
            };
        }

        private void Populate(ObservableCollection<DeviceEntryViewModel> target, DeviceCollectionViewModel collection, bool isOutput)
        {
            var connectedIds = new HashSet<string>();
            foreach (var d in collection.AllDevices)
            {
                connectedIds.Add(d.Id);
                AddEntry(target, new DeviceEntryViewModel(d.Id, d.OriginalDisplayName, _settings));
            }

            foreach (var entry in _settings.GetModifiedDeviceIds())
            {
                var id = entry.Item1;
                var entryIsOutput = entry.Item2;
                if (entryIsOutput == isOutput && !connectedIds.Contains(id))
                    AddEntry(target, new DeviceEntryViewModel(id, _settings));
            }

            ResortDevices(target);
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e, ObservableCollection<DeviceEntryViewModel> target, DeviceCollectionViewModel collection)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var addedDev = (DeviceViewModel)e.NewItems[0];
                    var existingEntry = target.FirstOrDefault(x => x.DeviceId == addedDev.Id);
                    if (existingEntry != null)
                    {
                        existingEntry.DeviceName = addedDev.OriginalDisplayName;
                        existingEntry.IsConnected = true;
                    }
                    else
                        AddEntry(target, new DeviceEntryViewModel(addedDev.Id, addedDev.OriginalDisplayName, _settings));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var removedDev = (DeviceViewModel)e.OldItems[0];
                    var removedEntry = target.FirstOrDefault(x => x.DeviceId == removedDev.Id);
                    if (removedEntry != null)
                    {
                        if (removedEntry.IsModified)
                        {
                            removedEntry.DeviceName = DeviceEntryViewModel.MakeDisconnectedName(removedDev.Id, _settings);
                            removedEntry.IsConnected = false;
                        }
                        else
                            target.Remove(removedEntry);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    var connectedMap = collection.AllDevices.ToDictionary(d => d.Id);
                    for (int i = target.Count - 1; i >= 0; i--)
                    {
                        var item = target[i];
                        if (connectedMap.TryGetValue(item.DeviceId, out var dev))
                        {
                            item.DeviceName = dev.OriginalDisplayName;
                            item.IsConnected = true;
                        }
                        else if (item.IsModified)
                        {
                            item.DeviceName = DeviceEntryViewModel.MakeDisconnectedName(item.DeviceId, _settings);
                            item.IsConnected = false;
                        }
                        else
                            target.RemoveAt(i);
                    }
                    foreach (var d in collection.AllDevices)
                    {
                        if (!target.Any(x => x.DeviceId == d.Id))
                            AddEntry(target, new DeviceEntryViewModel(d.Id, d.OriginalDisplayName, _settings));
                    }
                    ResortDevices(target);
                    break;
            }
        }
    }
}
