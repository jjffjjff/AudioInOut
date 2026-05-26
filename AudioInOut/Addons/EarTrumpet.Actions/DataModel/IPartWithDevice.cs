using AudioInOut.Actions.DataModel.Serialization;

namespace AudioInOut.Actions.DataModel
{
    public interface IPartWithDevice
    {
        Device Device { get; set; }
    }
}
