using AudioInOut.Extensions;
using AudioInOut.Interop;
using AudioInOut.Interop.Helpers;
using AudioInOut.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace AudioInOut.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private bool _isScrollSpy;

        public SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("SettingsWindow Closed");

            InitializeComponent();

            SourceInitialized += (sender, __) =>
            {
                this.Cloak();
                this.EnableRoundedCornersIfApplicable();

                if (App.Settings.SettingsWindowPlacement != null)
                {
                    User32.SetWindowPlacement(new WindowInteropHelper((Window)sender).Handle, App.Settings.SettingsWindowPlacement.Value);
                }
            };

            StateChanged += OnWindowStateChanged;

            Closing += (sender, __) =>
            {
                if (User32.GetWindowPlacement(new WindowInteropHelper((Window)sender).Handle, out var placement))
                {
                    App.Settings.SettingsWindowPlacement = placement;
                }
            };
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            var chrome = System.Windows.Shell.WindowChrome.GetWindowChrome(this);
            chrome.ResizeBorderThickness = WindowState == WindowState.Maximized ? new Thickness(0) : SystemParameters.WindowResizeBorderThickness;

            if (WindowState == WindowState.Maximized)
            {
                WindowSizeHelper.RestrictMaximizedSizeToWorkArea(this);
            }
        }

        private FrameworkElement[] GetSections() =>
            new FrameworkElement[] { SectionAppBehavior, SectionScrollBehavior, SectionFloatingMixer, SectionShortcuts, SectionAbout };

        private void SectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isScrollSpy || e.AddedItems.Count == 0) return;

            var sections = GetSections();
            int index = SectionList.SelectedIndex;
            if (index < 0 || index >= sections.Length) return;

            var transform = sections[index].TransformToAncestor(ContentScrollViewer);
            var pos = transform.Transform(new Point(0, 0));
            ContentScrollViewer.ScrollToVerticalOffset(ContentScrollViewer.VerticalOffset + pos.Y);
        }

        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sections = GetSections();
            int current = 0;
            double closest = double.MaxValue;

            for (int i = 0; i < sections.Length; i++)
            {
                var transform = sections[i].TransformToAncestor(ContentScrollViewer);
                var pos = transform.Transform(new Point(0, 0));
                double dist = Math.Abs(pos.Y);
                if (dist < closest)
                {
                    closest = dist;
                    current = i;
                }
            }

            _isScrollSpy = true;
            var vm = (SettingsWindowViewModel)DataContext;
            if (vm.SelectedSectionIndex != current)
                vm.SelectedSectionIndex = current;
            _isScrollSpy = false;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            var sections = GetSections();
            var sectionNames = new[] { "App behavior", "Scroll behavior", "Floating Mixer", "Shortcuts", "About" };

            for (int i = 0; i < sectionNames.Length; i++)
            {
                if (sectionNames[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ScrollToSection(sections[i]);
                    return;
                }
            }

            for (int i = 0; i < sections.Length; i++)
            {
                if (SectionContainsText(sections[i], query))
                {
                    ScrollToSection(sections[i]);
                    return;
                }
            }
        }

        private void ScrollToSection(FrameworkElement section)
        {
            var transform = section.TransformToAncestor(ContentScrollViewer);
            var pos = transform.Transform(new Point(0, 0));
            ContentScrollViewer.ScrollToVerticalOffset(ContentScrollViewer.VerticalOffset + pos.Y);
        }

        private static bool SectionContainsText(DependencyObject element, string query)
        {
            if (element is TextBlock tb && tb.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (element is ContentControl cc && (cc.Content as string)?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                if (SectionContainsText(VisualTreeHelper.GetChild(element, i), query))
                    return true;
            }
            return false;
        }
    }
}
