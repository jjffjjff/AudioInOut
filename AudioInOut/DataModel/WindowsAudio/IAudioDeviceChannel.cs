using System.ComponentModel;

namespace AudioInOut.DataModel.WindowsAudio
{
    public interface IAudioDeviceChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}