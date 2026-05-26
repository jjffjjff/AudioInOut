using System;

namespace AudioInOut.Integration
{
    public enum OverlayWindowEvent
    {
        Focus,
        Blur,
        Resize,
        DragStart,
        DragEnd,
        Transitioning,
    }

    public interface IOverlayClient
    {
        // Register the app's primary window handle with the daemon.
        void Register(IntPtr windowHandle, string appId);

        // Forward a window state event to the daemon.
        void SendEvent(OverlayWindowEvent kind);

        // Called by the auth/payment flow once the user has paid for this app_id.
        void Unlock(string appId);
    }
}
