namespace AudioInOut.DataModel.WindowsAudio.Internal
{
    interface IAudioDeviceInternal
    {
        void UpdatePeakValue();
        void MoveHiddenAppsToDevice(string appId, string id);
        void UnhideSessionsForProcessId(int processId);
    }
}
