using System.Windows;

namespace AudioInOut.UI.ViewModels
{
    public interface IPopupHostViewModel
    {
        void OpenPopup(object vm, FrameworkElement container);
    }
}