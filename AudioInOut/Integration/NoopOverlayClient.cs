using System;
using System.Diagnostics;

namespace AudioInOut.Integration
{
    // Placeholder until the Project B daemon ships. All calls are traced and discarded.
    public sealed class NoopOverlayClient : IOverlayClient
    {
        public void Register(IntPtr windowHandle, string appId)
        {
            Trace.WriteLine($"NoopOverlayClient.Register hwnd={windowHandle:X} appId={appId}");
        }

        public void SendEvent(OverlayWindowEvent kind)
        {
            Trace.WriteLine($"NoopOverlayClient.SendEvent kind={kind}");
        }

        public void Unlock(string appId)
        {
            Trace.WriteLine($"NoopOverlayClient.Unlock appId={appId}");
        }
    }
}
