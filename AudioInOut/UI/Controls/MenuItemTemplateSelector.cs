using AudioInOut.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AudioInOut.UI.Controls
{
    public class MenuItemTemplateSelector : ItemContainerTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
        {
            var key = "";
            if (item is ContextMenuSeparator)
            {
                key = "ContextMenuSeparatorTemplate";
            }
            else if (item is ContextMenuSectionTitle)
            {
                key = "ContextMenuSectionTitleTemplate";
            }
            else if (item is ContextMenuItem && ((ContextMenuItem)item).Children != null)
            {
                key = "ContextMenuSubItemTemplate";
            }
            else if (item is ContextMenuItem)
            {
                key = "ContextMenuItemTemplate";
            }
            else throw new NotImplementedException();

            return (DataTemplate)parentItemsControl.FindResource(key);
        }
    }
}
