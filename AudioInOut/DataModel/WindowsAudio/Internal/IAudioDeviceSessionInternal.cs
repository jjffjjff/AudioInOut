using AudioInOut.DataModel.Audio;
using System;

namespace AudioInOut.DataModel.WindowsAudio.Internal
{
    interface IAudioDeviceSessionInternal : IAudioDeviceSession
    {
        Guid GroupingParam { get; }
        void Hide();
        void UnHide();
        void MoveToDevice(string id, bool hide);
        void UpdatePeakValueBackground();
    }
}
