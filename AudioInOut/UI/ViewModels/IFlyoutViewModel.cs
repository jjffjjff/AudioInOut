using AudioInOut.UI.Helpers;
using System;

namespace AudioInOut.UI.ViewModels
{
    public interface IFlyoutViewModel
    {
        FlyoutViewState State { get; }
        bool IsExpandingOrCollapsing { get; }

        event EventHandler<object> StateChanged;
        event EventHandler<object> WindowSizeInvalidated;

        void ChangeState(FlyoutViewState state);
        void UpdateWindowPos(double top, double left, double height, double width);
    }
}