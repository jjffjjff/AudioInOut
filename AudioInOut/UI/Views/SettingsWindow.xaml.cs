using AudioInOut.Extensions;
using AudioInOut.Interop;
using AudioInOut.Interop.Helpers;
using AudioInOut.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

namespace AudioInOut.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private bool _isScrollSpy;
        private readonly List<(TextBlock tb, string text)> _highlighted = new List<(TextBlock, string)>();

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
            new FrameworkElement[] { SectionAppearance, SectionAppBehavior, SectionScrollBehavior, SectionFloatingMixer, SectionShortcuts, SectionAbout };

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
            if (DataContext is SettingsWindowViewModel vm && vm.SelectedSectionIndex != current)
                vm.SelectedSectionIndex = current;
            _isScrollSpy = false;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearHighlights();

            var query = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            var sections = GetSections();
            var sectionNames = new[] { "Appearance", "App behavior", "Scroll behavior", "Floating Mixer", "Shortcuts", "About" };

            for (int i = 0; i < sectionNames.Length; i++)
            {
                if (sectionNames[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ScrollToSection(sections[i]);
                    ApplyHighlights(sections[i], query);
                    return;
                }
            }

            for (int i = 0; i < sections.Length; i++)
            {
                if (SectionContainsText(sections[i], query))
                {
                    ScrollToSection(sections[i]);
                    ApplyHighlights(sections[i], query);
                    return;
                }
            }
        }

        private void ClearHighlights()
        {
            foreach (var (tb, text) in _highlighted)
                tb.Text = text;
            _highlighted.Clear();
        }

        private void ApplyHighlights(DependencyObject element, string query)
        {
            if (element is TextBlock tb && !string.IsNullOrEmpty(tb.Text))
            {
                var text = tb.Text;
                int idx = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    _highlighted.Add((tb, text));
                    tb.Inlines.Clear();
                    if (idx > 0)
                        tb.Inlines.Add(new Run(text.Substring(0, idx)));
                    tb.Inlines.Add(new Run(text.Substring(idx, query.Length))
                    {
                        Background = new SolidColorBrush(Color.FromArgb(200, 255, 165, 0))
                    });
                    if (idx + query.Length < text.Length)
                        tb.Inlines.Add(new Run(text.Substring(idx + query.Length)));
                }
            }

            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
                ApplyHighlights(VisualTreeHelper.GetChild(element, i), query);
        }

        private void ScrollToSection(FrameworkElement section)
        {
            var transform = section.TransformToAncestor(ContentScrollViewer);
            var pos = transform.Transform(new Point(0, 0));
            ContentScrollViewer.ScrollToVerticalOffset(ContentScrollViewer.VerticalOffset + pos.Y);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = System.Windows.WindowState.Minimized;

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == System.Windows.WindowState.Maximized
                ? System.Windows.WindowState.Normal
                : System.Windows.WindowState.Maximized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

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
