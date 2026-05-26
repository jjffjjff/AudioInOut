using AudioInOut.UI.ViewModels;
using System.Collections.Generic;

namespace AudioInOut.Extensibility
{
    public interface IEarTrumpetAddonNotificationAreaContextMenu
    {
        IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems { get; }
    }
}