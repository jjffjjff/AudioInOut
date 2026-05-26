using AudioInOut.Actions.DataModel.Serialization;

namespace AudioInOut.Actions.ViewModel.Actions
{
    class SetDefaultDeviceActionViewModel : PartViewModel
    {
        public DeviceListViewModel Device { get; }

        public SetDefaultDeviceActionViewModel(SetDefaultDeviceAction action) : base(action)
        {
            Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.Recording);
            Attach(Device);
        }
    }
}
