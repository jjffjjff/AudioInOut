using System;
using AudioInOut.UI.ViewModels;
using System.Collections.Generic;

namespace AudioInOut.Extensibility
{
    public interface IEarTrumpetAddonAppContent
    {
        object GetContentForApp(string deviceId, string appId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForApp(string deviceId, string appId);
    }
}
