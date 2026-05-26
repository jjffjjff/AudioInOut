using System.ComponentModel;

namespace AudioInOut.DataModel.WindowsAudio
{
    public interface IAudioDeviceSessionChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}