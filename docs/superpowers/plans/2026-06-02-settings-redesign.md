# Settings Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the two-level category-tile/pages settings UI with a single scrollable page, a flat left sidebar, and a search box at the bottom of the sidebar.

**Architecture:** New `SettingsWindowViewModel` owns 4 section VMs directly (AppBehavior, ScrollBehavior, Shortcuts, About). `SettingsWindow.xaml` replaces the main/subview toggle with a permanent two-column layout. Scroll-spy and search live in the view's code-behind (`SettingsWindow.xaml.cs`).

**Tech Stack:** C# / WPF / .NET 4.6.2, x86. No new NuGet packages.

**Build command:**
```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```
Output: `Build\Debug\AudioInOut.exe`

---

### Task 1: Add `FloatingMixerPlaceholderEnabled` to `AppSettings`

**Files:**
- Modify: `AudioInOut/AppSettings.cs`

- [ ] **Add the property** — insert after the `UseLogarithmicVolume` block (before `FullMixerWindowPlacement`):

```csharp
public bool FloatingMixerPlaceholderEnabled
{
    get => _settings.Get("FloatingMixerPlaceholderEnabled", false);
    set => _settings.Set("FloatingMixerPlaceholderEnabled", value);
}
```

- [ ] **Build to confirm no errors**

```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```

Expected: `Build succeeded.`

- [ ] **Commit**

```
git add AudioInOut/AppSettings.cs
git commit -m "feat(settings): add FloatingMixerPlaceholderEnabled to AppSettings"
```

---

### Task 2: Create `AppBehaviorViewModel`

**Files:**
- Create: `AudioInOut/UI/ViewModels/AppBehaviorViewModel.cs`

- [ ] **Create the file** with this exact content:

```csharp
using AudioInOut.UI.Helpers;
using Microsoft.Win32;
using System.Diagnostics;

namespace AudioInOut.UI.ViewModels
{
    public class AppBehaviorViewModel : BindableBase
    {
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RunKeyName = "AudioInOut";

        private readonly AppSettings _settings;

        public bool UseLogarithmicVolume
        {
            get => _settings.UseLogarithmicVolume;
            set => _settings.UseLogarithmicVolume = value;
        }

        public bool StartWithWindows
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                    return key?.GetValue(RunKeyName) != null;
            }
            set
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;
                    if (value)
                    {
                        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                        if (exePath != null) key.SetValue(RunKeyName, exePath);
                    }
                    else
                        key.DeleteValue(RunKeyName, throwOnMissingValue: false);
                }
            }
        }

        public bool FloatingMixerPlaceholderEnabled
        {
            get => _settings.FloatingMixerPlaceholderEnabled;
            set => _settings.FloatingMixerPlaceholderEnabled = value;
        }

        public AppBehaviorViewModel(AppSettings settings)
        {
            _settings = settings;
        }
    }
}
```

- [ ] **Add to csproj** — open `AudioInOut/AudioInOut.csproj`, find the line:
```xml
<Compile Include="UI\ViewModels\BackstackViewModel.cs" />
```
Insert immediately before it:
```xml
<Compile Include="UI\ViewModels\AppBehaviorViewModel.cs" />
```

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.`

- [ ] **Commit**

```
git add AudioInOut/UI/ViewModels/AppBehaviorViewModel.cs AudioInOut/AudioInOut.csproj
git commit -m "feat(settings): add AppBehaviorViewModel"
```

---

### Task 3: Create `SettingsWindowViewModel`

**Files:**
- Create: `AudioInOut/UI/ViewModels/SettingsWindowViewModel.cs`

This replaces `SettingsViewModel`. It exposes the four section VMs and properties needed by the XAML.

- [ ] **Create the file** with this exact content:

```csharp
using AudioInOut.UI.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace AudioInOut.UI.ViewModels
{
    class SettingsWindowViewModel : BindableBase
    {
        public string Title { get; } = Properties.Resources.SettingsWindowText;
        public AppBehaviorViewModel AppBehavior { get; }
        public EarTrumpetMouseSettingsPageViewModel ScrollBehavior { get; }
        public EarTrumpetShortcutsPageViewModel Shortcuts { get; }
        public EarTrumpetAboutPageViewModel About { get; }

        private SettingsDialogViewModel _dialog;
        public SettingsDialogViewModel Dialog
        {
            get => _dialog;
            set
            {
                if (_dialog != value)
                {
                    _dialog = value;
                    RaisePropertyChanged(nameof(Dialog));
                }
            }
        }

        private int _selectedSectionIndex;
        public int SelectedSectionIndex
        {
            get => _selectedSectionIndex;
            set
            {
                if (_selectedSectionIndex != value)
                {
                    _selectedSectionIndex = value;
                    RaisePropertyChanged(nameof(SelectedSectionIndex));
                }
            }
        }

        private WindowViewState _state;

        public SettingsWindowViewModel(AppSettings settings, Action openDiagnostics)
        {
            AppBehavior = new AppBehaviorViewModel(settings);
            ScrollBehavior = new EarTrumpetMouseSettingsPageViewModel(settings);
            Shortcuts = new EarTrumpetShortcutsPageViewModel(settings);
            About = new EarTrumpetAboutPageViewModel(openDiagnostics, settings);
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            switch (_state)
            {
                case WindowViewState.Open:
                    _state = WindowViewState.Closing;
                    e.Cancel = true;
                    WindowAnimationLibrary.BeginWindowExitAnimation((Window)sender, () =>
                    {
                        _state = WindowViewState.CloseReady;
                        Window.GetWindow((DependencyObject)sender)?.Close();
                    });
                    break;
                case WindowViewState.Closing:
                    e.Cancel = true;
                    break;
                case WindowViewState.CloseReady:
                    break;
            }
        }
    }
}
```

- [ ] **Add to csproj** — find the line:
```xml
<Compile Include="UI\ViewModels\SettingsViewModel.cs" />
```
Insert immediately before it:
```xml
<Compile Include="UI\ViewModels\SettingsWindowViewModel.cs" />
```

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.`

- [ ] **Commit**

```
git add AudioInOut/UI/ViewModels/SettingsWindowViewModel.cs AudioInOut/AudioInOut.csproj
git commit -m "feat(settings): add SettingsWindowViewModel"
```

---

### Task 4: Rewrite `SettingsWindow.xaml`

**Files:**
- Modify: `AudioInOut/UI/Views/SettingsWindow.xaml`

Replace the entire file with the following. Key changes vs. original:
- Removed `b:FrameworkElementEx.DisplaySettingsChanged` (no handler in new VM)
- Removed `BackButton`, home navigation, ComboBox search
- Removed DataTemplates for Community/Legacy/Category/Header VMs
- Removed `SettingsListView`, `SettingsListViewItem`, `MainList`, `MainListItem` styles
- Added `SidebarListViewItem` / `SidebarListView` styles
- Replaced Main+Subview grid pair with permanent two-column layout
- Background column split now 220/\* to match sidebar width

- [ ] **Replace `SettingsWindow.xaml`** with this content:

```xml
<Window x:Class="AudioInOut.UI.Views.SettingsWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:Event="clr-namespace:AudioInOut.Extensions.EventBinding"
        xmlns:Theme="clr-namespace:AudioInOut.UI.Themes"
        xmlns:b="clr-namespace:AudioInOut.UI.Behaviors"
        xmlns:resx="clr-namespace:AudioInOut.Properties"
        xmlns:vm="clr-namespace:AudioInOut.UI.ViewModels"
        Name="WindowRoot"
        Title="{Binding Title}"
        Width="800"
        Height="600"
        MinWidth="600"
        MinHeight="320"
        Theme:AcrylicBrush.Background="AcrylicColor_Settings"
        Theme:Options.Source="App"
        Background="Transparent"
        Closing="{Event:Binding OnClosing}"
        ResizeMode="CanResize"
        Style="{StaticResource DialogWindowStyle}"
        TextOptions.TextFormattingMode="Display"
        UseLayoutRounding="True"
        WindowStartupLocation="CenterScreen">
    <Window.Resources>

        <!--  Shortcuts page content  -->
        <DataTemplate DataType="{x:Type vm:EarTrumpetShortcutsPageViewModel}">
            <StackPanel Orientation="Vertical">
                <TextBlock Style="{StaticResource BodyText}" Text="{x:Static resx:Resources.SettingsOpenEarTrumpetText}" />
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="Auto" />
                    </Grid.RowDefinitions>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.DefaultHotkeyDescriptionText}" />
                    <TextBlock Grid.Column="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodyText}"
                               Text="{Binding DefaultHotKey}" />
                    <TextBlock Grid.Row="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.HotkeyDescriptionText}" />
                    <ContentControl Grid.Row="1"
                                    Grid.Column="1"
                                    Content="{Binding OpenFlyoutHotkey}"
                                    IsTabStop="False" />
                </Grid>
                <TextBlock Style="{StaticResource BodyText}" Text="{x:Static resx:Resources.SettingsOpenMixerText}" />
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="Auto" />
                    </Grid.RowDefinitions>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.DefaultHotkeyDescriptionText}" />
                    <TextBlock Grid.Column="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodyText}"
                               Text="{Binding DefaultMixerHotKey}" />
                    <TextBlock Grid.Row="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.HotkeyDescriptionText}" />
                    <ContentControl Grid.Row="1"
                                    Grid.Column="1"
                                    Content="{Binding OpenMixerHotkey}"
                                    IsTabStop="False" />
                </Grid>
                <TextBlock Style="{StaticResource BodyText}" Text="{x:Static resx:Resources.SettingsOpenSettingsText}" />
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="Auto" />
                    </Grid.RowDefinitions>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.DefaultHotkeyDescriptionText}" />
                    <TextBlock Grid.Column="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodyText}"
                               Text="{Binding DefaultSettingsHotKey}" />
                    <TextBlock Grid.Row="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.HotkeyDescriptionText}" />
                    <ContentControl Grid.Row="1"
                                    Grid.Column="1"
                                    Content="{Binding OpenSettingsHotkey}"
                                    IsTabStop="False" />
                </Grid>
                <TextBlock Style="{StaticResource BodyText}" Text="{x:Static resx:Resources.SettingsAbsoluteVolumeUpText}" />
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="Auto" />
                    </Grid.RowDefinitions>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.DefaultHotkeyDescriptionText}" />
                    <TextBlock Grid.Column="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodyText}"
                               Text="{Binding DefaultSettingsHotKey}" />
                    <TextBlock Grid.Row="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.HotkeyDescriptionText}" />
                    <ContentControl Grid.Row="1"
                                    Grid.Column="1"
                                    Content="{Binding AbsoluteVolumeUpHotkey}"
                                    IsTabStop="False" />
                </Grid>
                <TextBlock Style="{StaticResource BodyText}" Text="{x:Static resx:Resources.SettingsAbsoluteVolumeDownText}" />
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="Auto" />
                    </Grid.RowDefinitions>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.DefaultHotkeyDescriptionText}" />
                    <TextBlock Grid.Column="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodyText}"
                               Text="{Binding DefaultSettingsHotKey}" />
                    <TextBlock Grid.Row="1"
                               VerticalAlignment="Center"
                               Style="{StaticResource BodySubText}"
                               Text="{x:Static resx:Resources.HotkeyDescriptionText}" />
                    <ContentControl Grid.Row="1"
                                    Grid.Column="1"
                                    Content="{Binding AbsoluteVolumeDownHotkey}"
                                    IsTabStop="False" />
                </Grid>
            </StackPanel>
        </DataTemplate>

        <!--  About page content  -->
        <DataTemplate DataType="{x:Type vm:EarTrumpetAboutPageViewModel}">
            <StackPanel>
                <StackPanel Margin="0,8,0,8" Orientation="Horizontal">
                    <TextBlock FontSize="20" Text="Audio-InOut" />
                </StackPanel>
                <StackPanel Orientation="Horizontal">
                    <StackPanel>
                        <TextBlock VerticalAlignment="Center"
                                   Style="{StaticResource BodyText}"
                                   Text="{Binding AboutText}" />
                        <TextBlock VerticalAlignment="Center"
                                   Style="{StaticResource BodyText}"
                                   Text="Based on EarTrumpet by File-New-Project (MIT License)" />
                        <TextBlock Style="{StaticResource HyperlinkBlock}">
                            <Hyperlink Command="{Binding OpenPrivacyPolicyCommand}">
                                <Run Text="{x:Static resx:Resources.PrivacyPolicyText}" />
                            </Hyperlink>
                        </TextBlock>
                        <TextBlock Style="{StaticResource HyperlinkBlock}">
                            <Hyperlink Command="{Binding OpenFeedbackCommand}">
                                <Run Text="{x:Static resx:Resources.ContextMenuSendFeedback}" />
                            </Hyperlink>
                        </TextBlock>
                        <TextBlock Style="{StaticResource HyperlinkBlock}">
                            <Hyperlink Command="{Binding OpenDiagnosticsCommand}">
                                <Run Text="{x:Static resx:Resources.TroubleshootEarTrumpetText}" />
                            </Hyperlink>
                        </TextBlock>
                    </StackPanel>
                </StackPanel>
            </StackPanel>
        </DataTemplate>

        <!--  Sidebar item style  -->
        <Style x:Key="SidebarListViewItem" TargetType="{x:Type ListViewItem}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="Height" Value="36" />
            <Setter Property="Padding" Value="16,0,0,0" />
            <Setter Property="HorizontalContentAlignment" Value="Left" />
            <Setter Property="VerticalContentAlignment" Value="Center" />
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="FocusVisualStyle" Value="{StaticResource Windows10FocusVisualStyle}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ListViewItem}">
                        <Border x:Name="Bd"
                                Padding="{TemplateBinding Padding}"
                                Background="{TemplateBinding Background}"
                                SnapsToDevicePixels="true">
                            <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                              VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter TargetName="Bd" Property="Theme:Brush.Background" Value="Theme={Theme}ListLow, HighContrast=Highlight" />
                            </Trigger>
                            <MultiTrigger>
                                <MultiTrigger.Conditions>
                                    <Condition Property="IsMouseOver" Value="True" />
                                    <Condition Property="IsSelected" Value="False" />
                                </MultiTrigger.Conditions>
                                <Setter TargetName="Bd" Property="Theme:Brush.Background" Value="Theme={Theme}ListLow, HighContrast=Highlight" />
                            </MultiTrigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <!--  Sidebar ListView  -->
        <Style x:Key="SidebarListView" TargetType="{x:Type ListView}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="HorizontalContentAlignment" Value="Stretch" />
            <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto" />
            <Setter Property="ScrollViewer.CanContentScroll" Value="true" />
            <Setter Property="ItemContainerStyle" Value="{StaticResource SidebarListViewItem}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ListView}">
                        <Border Background="{TemplateBinding Background}" SnapsToDevicePixels="true">
                            <ScrollViewer Padding="{TemplateBinding Padding}" Focusable="false">
                                <ItemsPresenter SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}" />
                            </ScrollViewer>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

    </Window.Resources>

    <Grid>
        <!--  Background — left column matches sidebar width  -->
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="220" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <Border Name="AcrylicBackgroundActive" Theme:Brush.Background="AcrylicBackground" />
            <Border Name="AcrylicBackgroundWhileDragging">
                <Border.Style>
                    <Style TargetType="{x:Type Border}">
                        <Setter Property="Theme:Brush.Background" Value="AcrylicBackgroundFallback" />
                        <Setter Property="Opacity" Value="0" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding ElementName=WindowRoot, Path=(Theme:AcrylicBrush.IsSuppressed)}" Value="True">
                                <DataTrigger.EnterActions>
                                    <BeginStoryboard Name="IsSuppressed_Enter_AnimationsOn">
                                        <Storyboard>
                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                             From="0"
                                                             To="1"
                                                             Duration="00:00:00" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </DataTrigger.EnterActions>
                                <DataTrigger.ExitActions>
                                    <BeginStoryboard Name="IsSuppressed_Exit_AnimationsOn">
                                        <Storyboard>
                                            <DoubleAnimationUsingKeyFrames BeginTime="0:0:0"
                                                                           Storyboard.TargetProperty="Opacity"
                                                                           Duration="0:00:00.25">
                                                <EasingDoubleKeyFrame KeyTime="0:0:0" Value="1" />
                                                <EasingDoubleKeyFrame KeyTime="0:0:0.1" Value="1" />
                                                <EasingDoubleKeyFrame KeyTime="0:0:0.25" Value="0" />
                                            </EasingDoubleKeyFrame>
                                        </Storyboard>
                                    </BeginStoryboard>
                                </DataTrigger.ExitActions>
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Border.Style>
            </Border>
            <Border Name="AcrylicBackgroundInactive">
                <Border.Style>
                    <Style TargetType="{x:Type Border}">
                        <Setter Property="Theme:Brush.Background" Value="AcrylicBackgroundFallback" />
                        <Setter Property="Opacity" Value="1" />
                        <Style.Triggers>
                            <MultiDataTrigger>
                                <MultiDataTrigger.Conditions>
                                    <Condition Binding="{Binding Source={StaticResource ThemeManager}, Path=AnimationsEnabled}" Value="True" />
                                    <Condition Binding="{Binding ElementName=WindowRoot, Path=IsActive}" Value="True" />
                                </MultiDataTrigger.Conditions>
                                <MultiDataTrigger.EnterActions>
                                    <StopStoryboard BeginStoryboardName="IsActive_Exit_AnimationsOff" />
                                    <StopStoryboard BeginStoryboardName="IsActive_Enter_AnimationsOff" />
                                    <BeginStoryboard Name="IsActive_Enter_AnimationsOn">
                                        <Storyboard>
                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                             From="1"
                                                             To="0"
                                                             Duration="00:00:00.15" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </MultiDataTrigger.EnterActions>
                                <MultiDataTrigger.ExitActions>
                                    <StopStoryboard BeginStoryboardName="IsActive_Enter_AnimationsOff" />
                                    <StopStoryboard BeginStoryboardName="IsActive_Exit_AnimationsOff" />
                                    <BeginStoryboard Name="IsActive_Exit_AnimationsOn">
                                        <Storyboard>
                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                             From="0"
                                                             To="1"
                                                             Duration="00:00:00.15" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </MultiDataTrigger.ExitActions>
                            </MultiDataTrigger>
                            <MultiDataTrigger>
                                <MultiDataTrigger.Conditions>
                                    <Condition Binding="{Binding Source={StaticResource ThemeManager}, Path=AnimationsEnabled}" Value="False" />
                                    <Condition Binding="{Binding ElementName=WindowRoot, Path=IsActive}" Value="True" />
                                </MultiDataTrigger.Conditions>
                                <MultiDataTrigger.EnterActions>
                                    <StopStoryboard BeginStoryboardName="IsActive_Enter_AnimationsOn" />
                                    <StopStoryboard BeginStoryboardName="IsActive_Exit_AnimationsOn" />
                                    <BeginStoryboard Name="IsActive_Enter_AnimationsOff">
                                        <Storyboard>
                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                             From="1"
                                                             To="0"
                                                             Duration="00:00:00" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </MultiDataTrigger.EnterActions>
                                <MultiDataTrigger.ExitActions>
                                    <StopStoryboard BeginStoryboardName="IsActive_Exit_AnimationsOn" />
                                    <StopStoryboard BeginStoryboardName="IsActive_Enter_AnimationsOn" />
                                    <BeginStoryboard Name="IsActive_Exit_AnimationsOff">
                                        <Storyboard>
                                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                                             From="0"
                                                             To="1"
                                                             Duration="00:00:00" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </MultiDataTrigger.ExitActions>
                            </MultiDataTrigger>
                        </Style.Triggers>
                    </Style>
                </Border.Style>
            </Border>
            <Border Grid.Column="1" Theme:Brush.Background="Background" />
        </Grid>

        <DockPanel LastChildFill="True">
            <!--  TitleBar  -->
            <Grid DockPanel.Dock="Top">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <TextBlock Margin="14,0,0,0"
                           Text="{Binding Title}"
                           TextAlignment="Left" />
                <Button x:Name="MinimizeButton"
                        Grid.Column="1"
                        Style="{StaticResource MinimizeButton}" />
                <Button x:Name="MaximizeRestoreButton"
                        Grid.Column="2"
                        Style="{StaticResource MaximizeButton}" />
                <Button x:Name="CloseButton"
                        Grid.Column="3"
                        Style="{StaticResource CloseButtonStyle}" />
            </Grid>

            <!--  Content  -->
            <Grid>
                <!--  Two-column layout: sidebar + scrollable content  -->
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="220" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>

                    <!--  Left sidebar  -->
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="*" />
                            <RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>

                        <ListView x:Name="SectionList"
                                  SelectedIndex="{Binding SelectedSectionIndex, Mode=TwoWay}"
                                  SelectionChanged="SectionList_SelectionChanged"
                                  Style="{StaticResource SidebarListView}">
                            <ListViewItem Content="App behavior" />
                            <ListViewItem Content="Scroll behavior" />
                            <ListViewItem Content="Floating Mixer" />
                            <ListViewItem Content="Shortcuts" />
                            <ListViewItem Content="About" />
                        </ListView>

                        <!--  Search box  -->
                        <Grid Grid.Row="1" Margin="12,8,12,12">
                            <TextBox x:Name="SearchBox"
                                     TextChanged="SearchBox_TextChanged" />
                            <TextBlock IsHitTestVisible="False"
                                       Margin="4,0,0,0"
                                       VerticalAlignment="Center"
                                       Theme:Brush.Foreground="GrayText"
                                       Text="Search...">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Visibility" Value="Collapsed" />
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding ElementName=SearchBox, Path=Text}" Value="">
                                                <Setter Property="Visibility" Value="Visible" />
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </Grid>
                    </Grid>

                    <!--  Right: scrollable single-page content  -->
                    <Border Grid.Column="1" Theme:Brush.BorderBrush="Theme=Transparent, HighContrast=Text">
                        <Border.Style>
                            <Style TargetType="Border">
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding Source={StaticResource ThemeManager}, Path=IsHighContrast}" Value="True">
                                        <Setter Property="BorderThickness" Value="1,0,0,0" />
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Border.Style>
                        <ScrollViewer x:Name="ContentScrollViewer"
                                      ScrollChanged="ContentScrollViewer_ScrollChanged"
                                      HorizontalScrollBarVisibility="Disabled"
                                      VerticalScrollBarVisibility="Auto">
                            <StackPanel Margin="24,8,24,24">

                                <!--  App behavior  -->
                                <Border x:Name="SectionAppBehavior" Padding="0,16,0,16">
                                    <StackPanel>
                                        <TextBlock Margin="0,0,0,12"
                                                   FontSize="18"
                                                   FontWeight="SemiBold"
                                                   Text="App behavior" />
                                        <CheckBox Margin="0,4,0,4"
                                                  HorizontalAlignment="Left"
                                                  Content="{x:Static resx:Resources.SettingsUseLogarithmicVolume}"
                                                  IsChecked="{Binding AppBehavior.UseLogarithmicVolume, Mode=TwoWay}" />
                                        <CheckBox Margin="0,4,0,4"
                                                  HorizontalAlignment="Left"
                                                  Content="Start with Windows"
                                                  IsChecked="{Binding AppBehavior.StartWithWindows, Mode=TwoWay}" />
                                    </StackPanel>
                                </Border>

                                <Separator />

                                <!--  Scroll behavior  -->
                                <Border x:Name="SectionScrollBehavior" Padding="0,16,0,16">
                                    <StackPanel>
                                        <TextBlock Margin="0,0,0,12"
                                                   FontSize="18"
                                                   FontWeight="SemiBold"
                                                   Text="Scroll behavior" />
                                        <CheckBox Margin="0,4,0,4"
                                                  HorizontalAlignment="Left"
                                                  Content="{x:Static resx:Resources.SettingsUseScrollWheelInTray}"
                                                  IsChecked="{Binding ScrollBehavior.UseScrollWheelInTray, Mode=TwoWay}" />
                                        <CheckBox Margin="0,4,0,4"
                                                  HorizontalAlignment="Left"
                                                  Content="{x:Static resx:Resources.SettingsUseGlobalMouseWheelHook}"
                                                  IsChecked="{Binding ScrollBehavior.UseGlobalMouseWheelHook, Mode=TwoWay}" />
                                    </StackPanel>
                                </Border>

                                <Separator />

                                <!--  Floating Mixer  -->
                                <Border x:Name="SectionFloatingMixer" Padding="0,16,0,16">
                                    <StackPanel>
                                        <TextBlock Margin="0,0,0,12"
                                                   FontSize="18"
                                                   FontWeight="SemiBold"
                                                   Text="Floating Mixer" />
                                        <CheckBox Margin="0,4,0,4"
                                                  HorizontalAlignment="Left"
                                                  Content="Enable floating mixer (coming soon)"
                                                  IsChecked="{Binding AppBehavior.FloatingMixerPlaceholderEnabled, Mode=TwoWay}" />
                                    </StackPanel>
                                </Border>

                                <Separator />

                                <!--  Shortcuts  -->
                                <Border x:Name="SectionShortcuts" Padding="0,16,0,16">
                                    <StackPanel>
                                        <TextBlock Margin="0,0,0,12"
                                                   FontSize="18"
                                                   FontWeight="SemiBold"
                                                   Text="Shortcuts" />
                                        <ContentControl Content="{Binding Shortcuts}"
                                                        FocusVisualStyle="{x:Null}"
                                                        Focusable="False"
                                                        IsTabStop="False" />
                                    </StackPanel>
                                </Border>

                                <Separator />

                                <!--  About  -->
                                <Border x:Name="SectionAbout" Padding="0,16,0,16">
                                    <StackPanel>
                                        <TextBlock Margin="0,0,0,12"
                                                   FontSize="18"
                                                   FontWeight="SemiBold"
                                                   Text="About" />
                                        <ContentControl Content="{Binding About}"
                                                        FocusVisualStyle="{x:Null}"
                                                        Focusable="False"
                                                        IsTabStop="False" />
                                    </StackPanel>
                                </Border>

                            </StackPanel>
                        </ScrollViewer>
                    </Border>
                </Grid>

                <!--  Dialog overlay (unchanged)  -->
                <Grid>
                    <Grid.Style>
                        <Style TargetType="Grid">
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Dialog}" Value="{x:Null}">
                                    <Setter Property="Visibility" Value="Collapsed" />
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Grid.Style>
                    <Grid Background="LightGray" Opacity="0.5" />
                    <Border MinWidth="300"
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"
                            Theme:Brush.BorderBrush="SystemAccent"
                            BorderThickness="1">
                        <Grid Theme:Brush.Background="Background">
                            <Grid Margin="12" KeyboardNavigation.TabNavigation="Cycle">
                                <Grid.RowDefinitions>
                                    <RowDefinition Height="*" />
                                    <RowDefinition Height="Auto" />
                                </Grid.RowDefinitions>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*" />
                                    <ColumnDefinition Width="*" />
                                </Grid.ColumnDefinitions>
                                <StackPanel Grid.ColumnSpan="2" Orientation="Vertical">
                                    <TextBlock Margin="12,12,12,0"
                                               Style="{StaticResource HeadingText}"
                                               Text="{Binding Dialog.Title}" />
                                    <TextBlock Margin="12,12,12,24"
                                               Style="{StaticResource BodyText}"
                                               Text="{Binding Dialog.Description}" />
                                </StackPanel>
                                <Button Grid.Row="1"
                                        Margin="12,12,2,12"
                                        HorizontalAlignment="Stretch"
                                        Theme:Brush.Background="SystemAccent"
                                        Theme:Brush.Foreground="Light=ApplicationTextDarkTheme, Dark=Text"
                                        Command="{Binding Dialog.Button1Command}"
                                        Content="{Binding Dialog.Button1Text}" />
                                <Button Grid.Row="1"
                                        Grid.Column="1"
                                        Margin="2,12,12,12"
                                        HorizontalAlignment="Stretch"
                                        Command="{Binding Dialog.Button2Command}"
                                        Content="{Binding Dialog.Button2Text}" />
                            </Grid>
                        </Grid>
                    </Border>
                </Grid>
            </Grid>
        </DockPanel>
    </Grid>
</Window>
```

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.` (XAML parse errors show up as build errors in this project.)

- [ ] **Commit**

```
git add AudioInOut/UI/Views/SettingsWindow.xaml
git commit -m "feat(settings): rewrite SettingsWindow.xaml with sidebar layout"
```

---

### Task 5: Update `SettingsWindow.xaml.cs` with scroll-spy and search

**Files:**
- Modify: `AudioInOut/UI/Views/SettingsWindow.xaml.cs`

- [ ] **Replace `SettingsWindow.xaml.cs`** with this content:

```csharp
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
```

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.`

- [ ] **Commit**

```
git add AudioInOut/UI/Views/SettingsWindow.xaml.cs
git commit -m "feat(settings): add scroll-spy and search to SettingsWindow code-behind"
```

---

### Task 6: Update `App.xaml.cs`

**Files:**
- Modify: `AudioInOut/App.xaml.cs`

- [ ] **Replace `CreateSettingsExperience`** — find this method (lines ~241–269) and replace it entirely:

```csharp
private Window CreateSettingsExperience()
{
    var viewModel = new SettingsWindowViewModel(Settings, () => _errorReporter.DisplayDiagnosticData());
    return new SettingsWindow { DataContext = viewModel };
}
```

- [ ] **Remove `CreateAddonSettingsPage`** — delete this method entirely (lines ~271–280):

```csharp
private SettingsCategoryViewModel CreateAddonSettingsPage(IEarTrumpetAddonSettingsPage addonSettingsPage)
{
    var addon = (EarTrumpetAddon)addonSettingsPage;
    var category = addonSettingsPage.GetSettingsCategory();

    if (!addon.IsInternal())
    {
        category.Pages.Add(new AddonAboutPageViewModel(addon));
    }
    return category;
}
```

- [ ] **Remove unused `using` statements** from `App.xaml.cs` if the compiler flags any (e.g. `System.Linq` may still be used elsewhere — check before removing).

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.`

- [ ] **Commit**

```
git add AudioInOut/App.xaml.cs
git commit -m "feat(settings): wire App.xaml.cs to SettingsWindowViewModel"
```

---

### Task 7: Update `AudioInOut.csproj` — remove deleted files

**Files:**
- Modify: `AudioInOut/AudioInOut.csproj`

- [ ] **Remove these four `<Compile>` entries** from the csproj:

```xml
<Compile Include="UI\ViewModels\BackstackViewModel.cs" />
<Compile Include="UI\ViewModels\EarTrumpetCommunitySettingsPageViewModel.cs" />
<Compile Include="UI\ViewModels\EarTrumpetLegacySettingsPageViewModel.cs" />
<Compile Include="UI\ViewModels\SettingsViewModel.cs" />
```

- [ ] **Build to confirm no errors**

Expected: `Build succeeded.` (MSBuild will not complain about missing files referenced in `<Compile>` until they are compiled — removing the entries now prevents build errors when files are deleted next.)

- [ ] **Commit**

```
git add AudioInOut/AudioInOut.csproj
git commit -m "chore(settings): remove deleted VMs from csproj"
```

---

### Task 8: Delete dead files

**Files to delete:**
- `AudioInOut/UI/ViewModels/BackstackViewModel.cs`
- `AudioInOut/UI/ViewModels/EarTrumpetCommunitySettingsPageViewModel.cs`
- `AudioInOut/UI/ViewModels/EarTrumpetLegacySettingsPageViewModel.cs`
- `AudioInOut/UI/ViewModels/SettingsViewModel.cs`

- [ ] **Delete the files**

```
git rm AudioInOut/UI/ViewModels/BackstackViewModel.cs
git rm AudioInOut/UI/ViewModels/EarTrumpetCommunitySettingsPageViewModel.cs
git rm AudioInOut/UI/ViewModels/EarTrumpetLegacySettingsPageViewModel.cs
git rm AudioInOut/UI/ViewModels/SettingsViewModel.cs
```

- [ ] **Build to confirm nothing broke**

Expected: `Build succeeded.`

- [ ] **Commit**

```
git commit -m "chore(settings): delete legacy settings VMs and BackstackViewModel"
```

---

### Task 9: Smoke-test in the running app

- [ ] **Launch the app**

```
Build\Debug\AudioInOut.exe
```

- [ ] **Open settings** via tray right-click → Settings

Expected: Window opens directly to the two-column layout. No tile screen.

- [ ] **Click each sidebar item** — App behavior, Scroll behavior, Floating Mixer, Shortcuts, About

Expected: Right panel scrolls to the corresponding section. Sidebar item stays highlighted.

- [ ] **Scroll the right panel manually**

Expected: Sidebar highlight updates to follow the visible section.

- [ ] **Type in the search box**

Type "scroll" → right panel jumps to Scroll behavior section.
Type "about" → right panel jumps to About section.
Type "windows" → right panel jumps to App behavior section (matches "Start with Windows").

- [ ] **Toggle "Start with Windows"**

Check registry: `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\AudioInOut` should appear/disappear.

- [ ] **Toggle "Enable floating mixer (coming soon)"**

Expected: toggle responds visually, no crash.

- [ ] **Close settings** — window should animate out.

- [ ] **Done** — if all checks pass, the feature is complete.
