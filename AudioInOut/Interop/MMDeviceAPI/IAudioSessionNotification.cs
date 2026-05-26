using System.Runtime.InteropServices;

namespace AudioInOut.Interop.MMDeviceAPI
{
    [Guid("641DD20B-4D41-49CC-ABA3-174B9477BB08")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionNotification
    {
        void OnSessionCreated([MarshalAs(UnmanagedType.Interface)] IAudioSessionControl NewSession);
    }
}