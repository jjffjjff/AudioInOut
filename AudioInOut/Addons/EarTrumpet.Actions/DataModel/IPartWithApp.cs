using AudioInOut.Actions.DataModel.Serialization;

namespace AudioInOut.Actions.DataModel
{
    interface IPartWithApp
    {
        AppRef App { get; set; }
    }
}
