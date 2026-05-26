using AudioInOut.UI.Helpers;
using System;

namespace AudioInOut.UI.ViewModels
{
    public interface ISettingsViewModel
    {
        void ShowDialog(string title, string description, string btn1, string btn2, Action btn1Clicked, Action btn2Clicked);
        void CompleteNavigation(NavigationCookie cookie);
    }
}