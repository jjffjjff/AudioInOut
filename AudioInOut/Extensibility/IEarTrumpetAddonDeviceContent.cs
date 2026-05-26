using System;
using AudioInOut.UI.ViewModels;
using System.Collections.Generic;

namespace AudioInOut.Extensibility
{
    public interface IEarTrumpetAddonDeviceContent
    {
        object GetContentForDevice(string deviceId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForDevice(string deviceId);
    }
}
