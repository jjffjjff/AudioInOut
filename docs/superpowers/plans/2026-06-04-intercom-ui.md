# Intercom UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the "Intercom" cream/charcoal design for the AudioInOut settings window and tray right-click context menu.

**Architecture:** Two new ResourceDictionary files (`IcomLight.xaml` / `IcomDark.xaml`) define 20 named brushes. A new `AppearanceTheme` setting (Light/Dark/System) drives which dict is merged at runtime via `App.ApplyIcomTheme()`. The settings window and context menu templates reference these brushes via `DynamicResource`. The existing `Theme:Manager` system is untouched.

**Tech Stack:** C#/.NET 4.6.2, WPF/XAML, MSBuild x86

**Build command (run from repo root):**
```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```

**Output:** `Build\Debug\AudioInOut.exe`

---

## File Map

| Action | File | What changes |
|--------|------|--------------|
| CREATE | `AudioInOut/UI/Themes/IcomLight.xaml` | 20 SolidColorBrush resources (light palette) |
| CREATE | `AudioInOut/UI/Themes/IcomDark.xaml` | 20 SolidColorBrush resources (dark palette) |
| CREATE | `AudioInOut/UI/ViewModels/AppearanceSettingsViewModel.cs` | Light/Dark/System ComboBox VM |
| MODIFY | `AudioInOut/AppSettings.cs` | Add `AppearanceTheme` property |
| MODIFY | `AudioInOut/App.xaml.cs` | Add `ApplyIcomTheme()` static method + call at startup |
| MODIFY | `AudioInOut/UI/ViewModels/SettingsWindowViewModel.cs` | Add `Appearance` property |
| MODIFY | `AudioInOut/UI/Views/SettingsWindow.xaml` | Full Intercom visual redesign |
| MODIFY | `AudioInOut/UI/Views/SettingsWindow.xaml.cs` | Add Appearance section + button handlers |
| MODIFY | `AudioInOut/App.xaml` | Update ContextMenu style + DataTemplates to Intercom |
| MODIFY | `AudioInOut/AudioInOut.csproj` | Add new files to build |

---

## Task 1 — IcomLight.xaml

**Files:** Create `AudioInOut/UI/Themes/IcomLight.xaml`

- [ ] **Step 1: Create the file**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="IcomCanvas"      Color="#F5F1EC" />
    <SolidColorBrush x:Key="IcomWin"         Color="#FFFFFF" />
    <SolidColorBrush x:Key="IcomCard"        Color="#FAF7F3" />
    <SolidColorBrush x:Key="IcomSide"        Color="#FAF7F3" />
    <SolidColorBrush x:Key="IcomField"       Color="#FFFFFF" />
    <SolidColorBrush x:Key="IcomBorder"      Color="#D3CEC6" />
    <SolidColorBrush x:Key="IcomBorderSoft"  Color="#E4DED5" />
    <SolidColorBrush x:Key="IcomText"        Color="#111111" />
    <SolidColorBrush x:Key="IcomText2"       Color="#626260" />
    <SolidColorBrush x:Key="IcomText3"       Color="#9C9FA5" />
    <SolidColorBrush x:Key="IcomHover"       Color="#EFE9E0" />
    <SolidColorBrush x:Key="IcomChip"        Color="#EFE9E0" />
    <SolidColorBrush x:Key="IcomSwOff"       Color="#DCD6CC" />
    <SolidColorBrush x:Key="IcomSwOffBorder" Color="#D3CEC6" />
    <SolidColorBrush x:Key="IcomSwOn"        Color="#111111" />
    <SolidColorBrush x:Key="IcomKnob"        Color="#FFFFFF" />
    <SolidColorBrush x:Key="IcomKnobBorder"  Color="#CFC8BE" />
    <SolidColorBrush x:Key="IcomMarkBg"      Color="#111111" />
    <SolidColorBrush x:Key="IcomMarkFg"      Color="#FFFFFF" />
    <SolidColorBrush x:Key="IcomAccent"      Color="#FF5600" />
</ResourceDictionary>
```

---

## Task 2 — IcomDark.xaml

**Files:** Create `AudioInOut/UI/Themes/IcomDark.xaml`

- [ ] **Step 1: Create the file**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="IcomCanvas"      Color="#161514" />
    <SolidColorBrush x:Key="IcomWin"         Color="#211F1D" />
    <SolidColorBrush x:Key="IcomCard"        Color="#1B1A18" />
    <SolidColorBrush x:Key="IcomSide"        Color="#1B1A18" />
    <SolidColorBrush x:Key="IcomField"       Color="#1B1A18" />
    <SolidColorBrush x:Key="IcomBorder"      Color="#3A3733" />
    <SolidColorBrush x:Key="IcomBorderSoft"  Color="#2C2A27" />
    <SolidColorBrush x:Key="IcomText"        Color="#F3EFE8" />
    <SolidColorBrush x:Key="IcomText2"       Color="#A8A39A" />
    <SolidColorBrush x:Key="IcomText3"       Color="#736F67" />
    <SolidColorBrush x:Key="IcomHover"       Color="#2A2825" />
    <SolidColorBrush x:Key="IcomChip"        Color="#2A2825" />
    <SolidColorBrush x:Key="IcomSwOff"       Color="#3A3733" />
    <SolidColorBrush x:Key="IcomSwOffBorder" Color="#46423D" />
    <SolidColorBrush x:Key="IcomSwOn"        Color="#F3EFE8" />
    <SolidColorBrush x:Key="IcomKnob"        Color="#F3EFE8" />
    <SolidColorBrush x:Key="IcomKnobBorder"  Color="#F3EFE8" />
    <SolidColorBrush x:Key="IcomMarkBg"      Color="#F3EFE8" />
    <SolidColorBrush x:Key="IcomMarkFg"      Color="#161514" />
    <SolidColorBrush x:Key="IcomAccent"      Color="#FF5600" />
</ResourceDictionary>
```

---

## Task 3 — AppearanceSettingsViewModel.cs

**Files:** Create `AudioInOut/UI/ViewModels/AppearanceSettingsViewModel.cs`

- [ ] **Step 1: Create the file**

```csharp
using AudioInOut.UI.Helpers;
using System.Collections.Generic;

namespace AudioInOut.UI.ViewModels
{
    public class AppearanceSettingsViewModel : BindableBase
    {
        public List<string> ThemeOptions { get; } = new List<string> { "Light", "Dark", "System" };

        private string _selectedTheme;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    RaisePropertyChanged(nameof(SelectedTheme));
                    App.Settings.AppearanceTheme = value;
                    App.ApplyIcomTheme(value);
                }
            }
        }

        public AppearanceSettingsViewModel()
        {
            _selectedTheme = App.Settings.AppearanceTheme;
        }
    }
}
```

---

## Task 4 — AppSettings.cs

**Files:** Modify `AudioInOut/AppSettings.cs`

- [ ] **Step 1: Add `AppearanceTheme` property after the `SettingsWindowPlacement` property (before the closing brace)**

Add this inside the `AppSettings` class, before the final `}`:

```csharp
public string AppearanceTheme
{
    get => _settings.Get("AppearanceTheme", "System");
    set => _settings.Set("AppearanceTheme", value);
}
```

---

## Task 5 — App.xaml.cs

**Files:** Modify `AudioInOut/App.xaml.cs`

- [ ] **Step 1: Add `using` directives** (if not present — add after existing usings)

```csharp
using System.Linq;
```

- [ ] **Step 2: Add `ApplyIcomTheme` static method to the `App` partial class**

Add after the `Settings` property declaration (after `public static AppSettings Settings { get; private set; }`):

```csharp
public static void ApplyIcomTheme(string mode)
{
    bool isDark;
    if (mode == "Dark") isDark = true;
    else if (mode == "Light") isDark = false;
    else isDark = !DataModel.SystemSettings.IsLightTheme;

    var dictUri = new Uri(
        isDark
            ? "pack://application:,,,/AudioInOut;component/UI/Themes/IcomDark.xaml"
            : "pack://application:,,,/AudioInOut;component/UI/Themes/IcomLight.xaml",
        UriKind.Absolute);

    var merged = Application.Current.Resources.MergedDictionaries;
    var existing = merged.Where(d =>
        d.Source != null &&
        (d.Source.OriginalString.Contains("IcomLight") ||
         d.Source.OriginalString.Contains("IcomDark")))
        .ToList();
    foreach (var d in existing)
        merged.Remove(d);

    merged.Add(new ResourceDictionary { Source = dictUri });
}
```

- [ ] **Step 3: Call `ApplyIcomTheme` at startup and on system theme change**

In `ContinueStartup()`, after the line `((UI.Themes.Manager)Resources["ThemeManager"]).Load();`, add:

```csharp
ApplyIcomTheme(Settings.AppearanceTheme);
UI.Themes.Manager.Current.ThemeChanged += () =>
{
    if (Settings.AppearanceTheme == "System")
        ApplyIcomTheme("System");
};
```

---

## Task 6 — SettingsWindowViewModel.cs

**Files:** Modify `AudioInOut/UI/ViewModels/SettingsWindowViewModel.cs`

- [ ] **Step 1: Add `Appearance` property**

Add `public AppearanceSettingsViewModel Appearance { get; }` after the existing `About` property:

```csharp
public AppBehaviorViewModel AppBehavior { get; }
public EarTrumpetMouseSettingsPageViewModel ScrollBehavior { get; }
public EarTrumpetShortcutsPageViewModel Shortcuts { get; }
public EarTrumpetAboutPageViewModel About { get; }
public AppearanceSettingsViewModel Appearance { get; }
```

- [ ] **Step 2: Initialize in constructor**

In the constructor body, after `About = new EarTrumpetAboutPageViewModel(openDiagnostics, settings);`, add:

```csharp
Appearance = new AppearanceSettingsViewModel();
```

---

## Task 7 — AudioInOut.csproj

**Files:** Modify `AudioInOut/AudioInOut.csproj`

- [ ] **Step 1: Add IcomLight.xaml and IcomDark.xaml**

Find the block containing `<Page Include="UI\Mutable.xaml">` and add the two new pages immediately after it:

```xml
<Page Include="UI\Themes\IcomLight.xaml">
  <SubType>Designer</SubType>
  <Generator>MSBuild:Compile</Generator>
</Page>
<Page Include="UI\Themes\IcomDark.xaml">
  <SubType>Designer</SubType>
  <Generator>MSBuild:Compile</Generator>
</Page>
```

- [ ] **Step 2: Add AppearanceSettingsViewModel.cs**

Find `<Compile Include="UI\ViewModels\AppBehaviorViewModel.cs" />` and add after it:

```xml
<Compile Include="UI\ViewModels\AppearanceSettingsViewModel.cs" />
```

---

## Task 8 — SettingsWindow.xaml

**Files:** Replace `AudioInOut/UI/Views/SettingsWindow.xaml` entirely

- [ ] **Step 1: Replace the entire file with the following**

```xml
<Window x:Class="AudioInOut.UI.Views.SettingsWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:Event="clr-namespace:AudioInOut.Extensions.EventBinding"
        xmlns:Theme="clr-namespace:AudioInOut.UI.Themes"
        xmlns:resx="clr-namespace:AudioInOut.Properties"
        xmlns:vm="clr-namespace:AudioInOut.UI.ViewModels"
        xmlns:win="clr-namespace:System.Windows;assembly=PresentationFramework"
        Name="WindowRoot"
        Title="{Binding Title}"
        Width="800"
        Height="600"
        MinWidth="600"
        MinHeight="320"
        Background="Transparent"
        Closing="{Event:Binding OnClosing}"
        ResizeMode="CanResize"
        Style="{StaticResource DialogWindowStyle}"
        TextOptions.TextFormattingMode="Display"
        UseLayoutRounding="True"
        WindowStartupLocation="CenterScreen">
    <WindowChrome.WindowChrome>
        <WindowChrome CaptionHeight="48"
                      ResizeBorderThickness="{x:Static win:SystemParameters.WindowResizeBorderThickness}" />
    </WindowChrome.WindowChrome>
    <Window.Resources>

        <!-- Shortcuts section content -->
        <DataTemplate DataType="{x:Type vm:EarTrumpetShortcutsPageViewModel}">
            <StackPanel>
                <Border BorderBrush="{DynamicResource IcomBorderSoft}" BorderThickness="0,0,0,1" Padding="16,11">
                    <Grid>
                        <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center">
                            <TextBlock FontSize="14" Foreground="{DynamicResource IcomText}"
                                       Text="{x:Static resx:Resources.SettingsOpenEarTrumpetText}" />
                            <TextBlock FontSize="12" Foreground="{DynamicResource IcomText3}" Margin="0,2,0,0" Text="Default · None" />
                        </StackPanel>
                        <Border HorizontalAlignment="Right" VerticalAlignment="Center"
                                Background="{DynamicResource IcomField}" BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="9" Padding="10,5">
                            <ContentControl Content="{Binding OpenFlyoutHotkey}"
                                            FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                        </Border>
                    </Grid>
                </Border>
                <Border BorderBrush="{DynamicResource IcomBorderSoft}" BorderThickness="0,0,0,1" Padding="16,11">
                    <Grid>
                        <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center">
                            <TextBlock FontSize="14" Foreground="{DynamicResource IcomText}"
                                       Text="{x:Static resx:Resources.SettingsOpenMixerText}" />
                            <TextBlock FontSize="12" Foreground="{DynamicResource IcomText3}" Margin="0,2,0,0" Text="Default · None" />
                        </StackPanel>
                        <Border HorizontalAlignment="Right" VerticalAlignment="Center"
                                Background="{DynamicResource IcomField}" BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="9" Padding="10,5">
                            <ContentControl Content="{Binding OpenMixerHotkey}"
                                            FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                        </Border>
                    </Grid>
                </Border>
                <Border BorderBrush="{DynamicResource IcomBorderSoft}" BorderThickness="0,0,0,1" Padding="16,11">
                    <Grid>
                        <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center">
                            <TextBlock FontSize="14" Foreground="{DynamicResource IcomText}"
                                       Text="{x:Static resx:Resources.SettingsOpenSettingsText}" />
                            <TextBlock FontSize="12" Foreground="{DynamicResource IcomText3}" Margin="0,2,0,0" Text="Default · None" />
                        </StackPanel>
                        <Border HorizontalAlignment="Right" VerticalAlignment="Center"
                                Background="{DynamicResource IcomField}" BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="9" Padding="10,5">
                            <ContentControl Content="{Binding OpenSettingsHotkey}"
                                            FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                        </Border>
                    </Grid>
                </Border>
                <Border BorderBrush="{DynamicResource IcomBorderSoft}" BorderThickness="0,0,0,1" Padding="16,11">
                    <Grid>
                        <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center">
                            <TextBlock FontSize="14" Foreground="{DynamicResource IcomText}"
                                       Text="{x:Static resx:Resources.SettingsAbsoluteVolumeUpText}" />
                            <TextBlock FontSize="12" Foreground="{DynamicResource IcomText3}" Margin="0,2,0,0" Text="Default · None" />
                        </StackPanel>
                        <Border HorizontalAlignment="Right" VerticalAlignment="Center"
                                Background="{DynamicResource IcomField}" BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="9" Padding="10,5">
                            <ContentControl Content="{Binding AbsoluteVolumeUpHotkey}"
                                            FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                        </Border>
                    </Grid>
                </Border>
                <Border Padding="16,11">
                    <Grid>
                        <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center">
                            <TextBlock FontSize="14" Foreground="{DynamicResource IcomText}"
                                       Text="{x:Static resx:Resources.SettingsAbsoluteVolumeDownText}" />
                            <TextBlock FontSize="12" Foreground="{DynamicResource IcomText3}" Margin="0,2,0,0" Text="Default · None" />
                        </StackPanel>
                        <Border HorizontalAlignment="Right" VerticalAlignment="Center"
                                Background="{DynamicResource IcomField}" BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="9" Padding="10,5">
                            <ContentControl Content="{Binding AbsoluteVolumeDownHotkey}"
                                            FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                        </Border>
                    </Grid>
                </Border>
            </StackPanel>
        </DataTemplate>

        <!-- About section content -->
        <DataTemplate DataType="{x:Type vm:EarTrumpetAboutPageViewModel}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="52" />
                    <ColumnDefinition Width="18" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                <Border Width="52" Height="52" CornerRadius="12" VerticalAlignment="Top"
                        Background="{DynamicResource IcomMarkBg}">
                    <TextBlock Text="A·IO" FontSize="13.5" FontWeight="Medium"
                               Foreground="{DynamicResource IcomMarkFg}"
                               HorizontalAlignment="Center" VerticalAlignment="Center" />
                </Border>
                <StackPanel Grid.Column="2">
                    <TextBlock Text="Audio-InOut" FontSize="19" FontWeight="SemiBold"
                               Foreground="{DynamicResource IcomText}" />
                    <TextBlock Text="{Binding AboutText}" FontSize="13.5"
                               Foreground="{DynamicResource IcomText2}" Margin="0,3,0,0" />
                    <TextBlock Text="Based on EarTrumpet by File-New-Project (MIT License)"
                               FontSize="13.5" Foreground="{DynamicResource IcomText2}"
                               Margin="0,10,0,0" TextWrapping="Wrap" MaxWidth="360" />
                    <StackPanel Margin="0,14,0,0">
                        <TextBlock FontSize="13.5" Foreground="{DynamicResource IcomText}">
                            <Hyperlink Command="{Binding OpenPrivacyPolicyCommand}">
                                <Run Text="{x:Static resx:Resources.PrivacyPolicyText}" />
                            </Hyperlink>
                        </TextBlock>
                        <TextBlock FontSize="13.5" Foreground="{DynamicResource IcomText}" Margin="0,8,0,0">
                            <Hyperlink Command="{Binding OpenFeedbackCommand}">
                                <Run Text="{x:Static resx:Resources.ContextMenuSendFeedback}" />
                            </Hyperlink>
                        </TextBlock>
                        <TextBlock FontSize="13.5" Foreground="{DynamicResource IcomText}" Margin="0,8,0,0">
                            <Hyperlink Command="{Binding OpenDiagnosticsCommand}">
                                <Run Text="{x:Static resx:Resources.TroubleshootEarTrumpetText}" />
                            </Hyperlink>
                        </TextBlock>
                    </StackPanel>
                </StackPanel>
            </Grid>
        </DataTemplate>

        <!-- Sidebar nav item -->
        <Style x:Key="SidebarListViewItem" TargetType="{x:Type ListViewItem}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="Height" Value="34" />
            <Setter Property="Margin" Value="0,1,0,1" />
            <Setter Property="HorizontalContentAlignment" Value="Left" />
            <Setter Property="VerticalContentAlignment" Value="Center" />
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="Foreground" Value="{DynamicResource IcomText2}" />
            <Setter Property="FocusVisualStyle" Value="{StaticResource Windows10FocusVisualStyle}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ListViewItem}">
                        <Border x:Name="Bd"
                                Padding="13,0"
                                Background="{TemplateBinding Background}"
                                BorderBrush="Transparent"
                                BorderThickness="1"
                                CornerRadius="8"
                                SnapsToDevicePixels="true">
                            <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                              VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter TargetName="Bd" Property="Background" Value="{DynamicResource IcomWin}" />
                                <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource IcomBorderSoft}" />
                                <Setter Property="Foreground" Value="{DynamicResource IcomText}" />
                            </Trigger>
                            <MultiTrigger>
                                <MultiTrigger.Conditions>
                                    <Condition Property="IsMouseOver" Value="True" />
                                    <Condition Property="IsSelected" Value="False" />
                                </MultiTrigger.Conditions>
                                <Setter TargetName="Bd" Property="Background" Value="{DynamicResource IcomHover}" />
                                <Setter Property="Foreground" Value="{DynamicResource IcomText}" />
                            </MultiTrigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Sidebar ListView container -->
        <Style x:Key="SidebarListView" TargetType="{x:Type ListView}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="HorizontalContentAlignment" Value="Stretch" />
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderThickness" Value="0" />
            <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Disabled" />
            <Setter Property="ItemContainerStyle" Value="{StaticResource SidebarListViewItem}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ListView}">
                        <ItemsPresenter SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}" />
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Toggle switch (pill CheckBox) -->
        <Style x:Key="IcomToggle" TargetType="{x:Type CheckBox}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="FocusVisualStyle" Value="{x:Null}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type CheckBox}">
                        <Border x:Name="Track"
                                Width="40" Height="23"
                                CornerRadius="11.5"
                                Background="{DynamicResource IcomSwOff}"
                                BorderBrush="{DynamicResource IcomSwOffBorder}"
                                BorderThickness="1"
                                Cursor="Hand">
                            <Border x:Name="Knob"
                                    Width="17" Height="17"
                                    CornerRadius="8.5"
                                    Background="{DynamicResource IcomKnob}"
                                    BorderBrush="{DynamicResource IcomKnobBorder}"
                                    BorderThickness="1"
                                    HorizontalAlignment="Left"
                                    Margin="2,0,0,0" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsChecked" Value="True">
                                <Trigger.EnterActions>
                                    <BeginStoryboard>
                                        <Storyboard>
                                            <ThicknessAnimation Storyboard.TargetName="Knob"
                                                                Storyboard.TargetProperty="Margin"
                                                                To="19,0,0,0" Duration="0:0:0.18" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </Trigger.EnterActions>
                                <Trigger.ExitActions>
                                    <BeginStoryboard>
                                        <Storyboard>
                                            <ThicknessAnimation Storyboard.TargetName="Knob"
                                                                Storyboard.TargetProperty="Margin"
                                                                To="2,0,0,0" Duration="0:0:0.18" />
                                        </Storyboard>
                                    </BeginStoryboard>
                                </Trigger.ExitActions>
                                <Setter TargetName="Track" Property="Background" Value="{DynamicResource IcomSwOn}" />
                                <Setter TargetName="Track" Property="BorderBrush" Value="{DynamicResource IcomSwOn}" />
                                <Setter TargetName="Knob" Property="Background" Value="{DynamicResource IcomWin}" />
                                <Setter TargetName="Knob" Property="BorderBrush" Value="{DynamicResource IcomWin}" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Window chrome buttons -->
        <Style x:Key="IcomWinButton" TargetType="{x:Type Button}">
            <Setter Property="OverridesDefaultStyle" Value="True" />
            <Setter Property="Width" Value="34" />
            <Setter Property="Height" Value="34" />
            <Setter Property="WindowChrome.IsHitTestVisibleInChrome" Value="True" />
            <Setter Property="Foreground" Value="{DynamicResource IcomText3}" />
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderThickness" Value="0" />
            <Setter Property="FontSize" Value="11" />
            <Setter Property="FocusVisualStyle" Value="{x:Null}" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type Button}">
                        <Border x:Name="bd" Background="{TemplateBinding Background}" CornerRadius="8">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="bd" Property="Background" Value="{DynamicResource IcomHover}" />
                                <Setter Property="Foreground" Value="{DynamicResource IcomText2}" />
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter TargetName="bd" Property="Background" Value="{DynamicResource IcomHover}" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <Style x:Key="IcomWinButtonClose" BasedOn="{StaticResource IcomWinButton}" TargetType="{x:Type Button}">
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Foreground" Value="{DynamicResource IcomText}" />
                </Trigger>
            </Style.Triggers>
        </Style>

    </Window.Resources>

    <Border Background="{DynamicResource IcomWin}"
            BorderBrush="{DynamicResource IcomBorder}"
            BorderThickness="1">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="48" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>

            <!-- Title bar -->
            <Border Background="{DynamicResource IcomWin}"
                    BorderBrush="{DynamicResource IcomBorderSoft}"
                    BorderThickness="0,0,0,1">
                <Grid Margin="20,0,8,0">
                    <StackPanel HorizontalAlignment="Left" Orientation="Horizontal" VerticalAlignment="Center">
                        <TextBlock FontSize="15" FontWeight="SemiBold"
                                   Foreground="{DynamicResource IcomText}" Text="Audio-InOut" />
                        <Border Width="1" Height="16" Margin="11,0" VerticalAlignment="Center"
                                Background="{DynamicResource IcomBorder}" />
                        <TextBlock FontSize="15" Foreground="{DynamicResource IcomText3}" Text="Settings" />
                    </StackPanel>
                    <StackPanel HorizontalAlignment="Right" Orientation="Horizontal" VerticalAlignment="Center">
                        <Button x:Name="MinimizeButton" Style="{StaticResource IcomWinButton}" Content="─"
                                Click="MinimizeButton_Click" />
                        <Button x:Name="MaximizeRestoreButton" Style="{StaticResource IcomWinButton}" Content="▢"
                                Click="MaximizeRestoreButton_Click" />
                        <Button x:Name="CloseButton" Style="{StaticResource IcomWinButtonClose}" Content="✕"
                                Click="CloseButton_Click" />
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Body: sidebar + content -->
            <Grid Grid.Row="1">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="200" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>

                <!-- Sidebar -->
                <Grid Background="{DynamicResource IcomSide}">
                    <Border BorderBrush="{DynamicResource IcomBorderSoft}" BorderThickness="0,0,1,0" />
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="*" />
                            <RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>
                        <ListView x:Name="SectionList"
                                  Margin="12,14,12,0"
                                  SelectedIndex="{Binding SelectedSectionIndex, Mode=TwoWay}"
                                  SelectionChanged="SectionList_SelectionChanged"
                                  Style="{StaticResource SidebarListView}">
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="App behavior"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="Scroll behavior"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="Floating Mixer"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="Shortcuts"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="Appearance"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                            <ListViewItem>
                                <TextBlock FontSize="14" FontWeight="Medium" Text="About"
                                           Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListViewItem}}" />
                            </ListViewItem>
                        </ListView>
                        <!-- Search field -->
                        <Border Grid.Row="1" Height="36" Margin="12,0,12,14"
                                Background="{DynamicResource IcomField}"
                                BorderBrush="{DynamicResource IcomBorder}"
                                BorderThickness="1" CornerRadius="8">
                            <Grid Margin="13,0">
                                <TextBox x:Name="SearchBox"
                                         Background="Transparent" BorderThickness="0"
                                         FontSize="13" Foreground="{DynamicResource IcomText}"
                                         TextChanged="SearchBox_TextChanged" VerticalAlignment="Center" />
                                <TextBlock IsHitTestVisible="False" FontSize="13"
                                           Foreground="{DynamicResource IcomText3}"
                                           Text="⌕  Search settings" VerticalAlignment="Center">
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
                        </Border>
                    </Grid>
                </Grid>

                <!-- Content pane -->
                <ScrollViewer x:Name="ContentScrollViewer"
                              Grid.Column="1"
                              Background="{DynamicResource IcomWin}"
                              HorizontalScrollBarVisibility="Disabled"
                              ScrollChanged="ContentScrollViewer_ScrollChanged"
                              VerticalScrollBarVisibility="Auto">
                    <StackPanel Margin="30,0,30,32">

                        <!-- App behavior -->
                        <Border x:Name="SectionAppBehavior" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="App behavior" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12">
                                    <StackPanel>
                                        <Border BorderBrush="{DynamicResource IcomBorderSoft}"
                                                BorderThickness="0,0,0,1" MinHeight="46">
                                            <Grid Margin="16,9">
                                                <TextBlock HorizontalAlignment="Left" VerticalAlignment="Center"
                                                           FontSize="14" Foreground="{DynamicResource IcomText}"
                                                           Text="{x:Static resx:Resources.SettingsUseLogarithmicVolume}"
                                                           TextWrapping="Wrap" MaxWidth="360" />
                                                <CheckBox HorizontalAlignment="Right" VerticalAlignment="Center"
                                                          Style="{StaticResource IcomToggle}"
                                                          IsChecked="{Binding AppBehavior.UseLogarithmicVolume, Mode=TwoWay}" />
                                            </Grid>
                                        </Border>
                                        <Border MinHeight="46">
                                            <Grid Margin="16,9">
                                                <TextBlock HorizontalAlignment="Left" VerticalAlignment="Center"
                                                           FontSize="14" Foreground="{DynamicResource IcomText}"
                                                           Text="Start with Windows" />
                                                <CheckBox HorizontalAlignment="Right" VerticalAlignment="Center"
                                                          Style="{StaticResource IcomToggle}"
                                                          IsChecked="{Binding AppBehavior.StartWithWindows, Mode=TwoWay}" />
                                            </Grid>
                                        </Border>
                                    </StackPanel>
                                </Border>
                            </StackPanel>
                        </Border>

                        <!-- Scroll behavior -->
                        <Border x:Name="SectionScrollBehavior" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="Scroll behavior" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12">
                                    <StackPanel>
                                        <Border BorderBrush="{DynamicResource IcomBorderSoft}"
                                                BorderThickness="0,0,0,1" MinHeight="46">
                                            <Grid Margin="16,9">
                                                <TextBlock HorizontalAlignment="Left" VerticalAlignment="Center"
                                                           FontSize="14" Foreground="{DynamicResource IcomText}"
                                                           Text="{x:Static resx:Resources.SettingsUseScrollWheelInTray}"
                                                           TextWrapping="Wrap" MaxWidth="360" />
                                                <CheckBox HorizontalAlignment="Right" VerticalAlignment="Center"
                                                          Style="{StaticResource IcomToggle}"
                                                          IsChecked="{Binding ScrollBehavior.UseScrollWheelInTray, Mode=TwoWay}" />
                                            </Grid>
                                        </Border>
                                        <Border MinHeight="46">
                                            <Grid Margin="16,9">
                                                <TextBlock HorizontalAlignment="Left" VerticalAlignment="Center"
                                                           FontSize="14" Foreground="{DynamicResource IcomText}"
                                                           Text="{x:Static resx:Resources.SettingsUseGlobalMouseWheelHook}"
                                                           TextWrapping="Wrap" MaxWidth="360" />
                                                <CheckBox HorizontalAlignment="Right" VerticalAlignment="Center"
                                                          Style="{StaticResource IcomToggle}"
                                                          IsChecked="{Binding ScrollBehavior.UseGlobalMouseWheelHook, Mode=TwoWay}" />
                                            </Grid>
                                        </Border>
                                    </StackPanel>
                                </Border>
                            </StackPanel>
                        </Border>

                        <!-- Floating Mixer -->
                        <Border x:Name="SectionFloatingMixer" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="Floating Mixer" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12">
                                    <Border MinHeight="46">
                                        <Grid Margin="16,9">
                                            <StackPanel HorizontalAlignment="Left" VerticalAlignment="Center"
                                                        Orientation="Horizontal">
                                                <TextBlock FontSize="14" Foreground="{DynamicResource IcomText2}"
                                                           Text="Enable floating mixer" Margin="0,0,8,0" />
                                                <Border Background="{DynamicResource IcomChip}"
                                                        CornerRadius="4" Padding="9,3">
                                                    <TextBlock FontSize="11.5" FontWeight="Medium"
                                                               Foreground="{DynamicResource IcomText2}" Text="Soon" />
                                                </Border>
                                            </StackPanel>
                                            <CheckBox HorizontalAlignment="Right" VerticalAlignment="Center"
                                                      IsEnabled="False"
                                                      Style="{StaticResource IcomToggle}"
                                                      IsChecked="{Binding AppBehavior.FloatingMixerPlaceholderEnabled, Mode=TwoWay}" />
                                        </Grid>
                                    </Border>
                                </Border>
                            </StackPanel>
                        </Border>

                        <!-- Shortcuts -->
                        <Border x:Name="SectionShortcuts" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="Shortcuts" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12">
                                    <ContentControl Content="{Binding Shortcuts}"
                                                    FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                                </Border>
                            </StackPanel>
                        </Border>

                        <!-- Appearance -->
                        <Border x:Name="SectionAppearance" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="Appearance" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12">
                                    <Border MinHeight="46">
                                        <Grid Margin="16,9">
                                            <TextBlock HorizontalAlignment="Left" VerticalAlignment="Center"
                                                       FontSize="14" Foreground="{DynamicResource IcomText}"
                                                       Text="Theme" />
                                            <ComboBox Width="120" HorizontalAlignment="Right" VerticalAlignment="Center"
                                                      ItemsSource="{Binding Appearance.ThemeOptions}"
                                                      SelectedItem="{Binding Appearance.SelectedTheme, Mode=TwoWay}" />
                                        </Grid>
                                    </Border>
                                </Border>
                            </StackPanel>
                        </Border>

                        <!-- About -->
                        <Border x:Name="SectionAbout" Padding="0,18,0,4">
                            <StackPanel>
                                <TextBlock Margin="0,0,0,10" FontSize="19" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}" Text="About" />
                                <Border Background="{DynamicResource IcomCard}"
                                        BorderBrush="{DynamicResource IcomBorderSoft}"
                                        BorderThickness="1" CornerRadius="12" Padding="18">
                                    <ContentControl Content="{Binding About}"
                                                    FocusVisualStyle="{x:Null}" Focusable="False" IsTabStop="False" />
                                </Border>
                            </StackPanel>
                        </Border>

                    </StackPanel>
                </ScrollViewer>
            </Grid>

            <!-- Dialog overlay -->
            <Grid Grid.RowSpan="2">
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
                <Border MinWidth="300" HorizontalAlignment="Center" VerticalAlignment="Center"
                        BorderBrush="{DynamicResource IcomBorder}" BorderThickness="1" CornerRadius="12">
                    <Grid Background="{DynamicResource IcomWin}">
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
                                <TextBlock Margin="12,12,12,0" FontSize="16" FontWeight="SemiBold"
                                           Foreground="{DynamicResource IcomText}"
                                           Text="{Binding Dialog.Title}" />
                                <TextBlock Margin="12,12,12,24" FontSize="14"
                                           Foreground="{DynamicResource IcomText2}"
                                           Text="{Binding Dialog.Description}" TextWrapping="Wrap" />
                            </StackPanel>
                            <Button Grid.Row="1" Margin="12,12,2,12" HorizontalAlignment="Stretch"
                                    Command="{Binding Dialog.Button1Command}"
                                    Content="{Binding Dialog.Button1Text}" />
                            <Button Grid.Row="1" Grid.Column="1" Margin="2,12,12,12" HorizontalAlignment="Stretch"
                                    Command="{Binding Dialog.Button2Command}"
                                    Content="{Binding Dialog.Button2Text}" />
                        </Grid>
                    </Grid>
                </Border>
            </Grid>

        </Grid>
    </Border>

</Window>
```

---

## Task 9 — SettingsWindow.xaml.cs

**Files:** Modify `AudioInOut/UI/Views/SettingsWindow.xaml.cs`

- [ ] **Step 1: Update `GetSections()` to include `SectionAppearance`**

Replace:
```csharp
private FrameworkElement[] GetSections() =>
    new FrameworkElement[] { SectionAppBehavior, SectionScrollBehavior, SectionFloatingMixer, SectionShortcuts, SectionAbout };
```

With:
```csharp
private FrameworkElement[] GetSections() =>
    new FrameworkElement[] { SectionAppBehavior, SectionScrollBehavior, SectionFloatingMixer, SectionShortcuts, SectionAppearance, SectionAbout };
```

- [ ] **Step 2: Update `sectionNames` in `SearchBox_TextChanged`**

Replace:
```csharp
var sectionNames = new[] { "App behavior", "Scroll behavior", "Floating Mixer", "Shortcuts", "About" };
```

With:
```csharp
var sectionNames = new[] { "App behavior", "Scroll behavior", "Floating Mixer", "Shortcuts", "Appearance", "About" };
```

- [ ] **Step 3: Add window button click handlers**

Add these three methods inside the `SettingsWindow` class (after the existing methods):

```csharp
private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    => WindowState = System.Windows.WindowState.Minimized;

private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    => WindowState = WindowState == System.Windows.WindowState.Maximized
        ? System.Windows.WindowState.Normal
        : System.Windows.WindowState.Maximized;

private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
```

---

## Task 10 — App.xaml context menu

**Files:** Modify `AudioInOut/App.xaml`

- [ ] **Step 1: Replace the `ContextMenu` style (lines ~582–627)**

Find the block starting `<Style TargetType="{x:Type ContextMenu}">` and ending `</Style>` (before `<Style TargetType="{x:Type Separator}">`). Replace it entirely with:

```xml
<!--  ContextMenu styles & templates — Intercom  -->
<Style TargetType="{x:Type ContextMenu}">
    <Setter Property="SnapsToDevicePixels" Value="True" />
    <Setter Property="TextOptions.TextFormattingMode" Value="Display" />
    <Setter Property="UseLayoutRounding" Value="True" />
    <Setter Property="OverridesDefaultStyle" Value="True" />
    <Setter Property="UsesItemContainerTemplate" Value="True" />
    <Setter Property="ItemContainerTemplateSelector" Value="{StaticResource MenuSelector}" />
    <Setter Property="Placement" Value="Mouse" />
    <Setter Property="MinWidth" Value="280" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type ContextMenu}">
                <Border Background="Transparent" Padding="0,0,8,8">
                    <Border x:Name="Card"
                            Background="{DynamicResource IcomCard}"
                            BorderBrush="{DynamicResource IcomBorder}"
                            BorderThickness="1"
                            CornerRadius="12"
                            Padding="6">
                        <Border.Effect>
                            <DropShadowEffect BlurRadius="14" Direction="270"
                                              Opacity="0.22" ShadowDepth="4" />
                        </Border.Effect>
                        <ScrollViewer HorizontalScrollBarVisibility="Hidden"
                                      MaxHeight="600"
                                      VerticalScrollBarVisibility="Auto">
                            <StackPanel IsItemsHost="True"
                                        KeyboardNavigation.DirectionalNavigation="Cycle" />
                        </ScrollViewer>
                    </Border>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- [ ] **Step 2: Replace the `Separator` style (lines ~628–642)**

Find the block `<Style TargetType="{x:Type Separator}">` and replace it entirely with:

```xml
<Style TargetType="{x:Type Separator}">
    <Setter Property="Margin" Value="10,5,10,5" />
    <Setter Property="OverridesDefaultStyle" Value="True" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Separator}">
                <Rectangle Height="1" HorizontalAlignment="Stretch"
                           Fill="{DynamicResource IcomBorderSoft}" />
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- [ ] **Step 3: Replace the `MenuItem` style (lines ~643–743)**

Find the block `<Style x:Key="{x:Type MenuItem}" TargetType="{x:Type MenuItem}">` and replace it entirely with:

```xml
<Style x:Key="{x:Type MenuItem}" TargetType="{x:Type MenuItem}">
    <Setter Property="OverridesDefaultStyle" Value="True" />
    <Setter Property="Foreground" Value="{DynamicResource IcomText}" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type MenuItem}">
                <Border x:Name="Border"
                        MinHeight="36"
                        Background="Transparent"
                        BorderThickness="0"
                        CornerRadius="8">
                    <ContentPresenter x:Name="HeaderHost"
                                      Margin="8,0"
                                      ContentSource="Header"
                                      RecognizesAccessKey="False"
                                      VerticalAlignment="Center" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsHighlighted" Value="True">
                        <Setter TargetName="Border" Property="Background" Value="{DynamicResource IcomHover}" />
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter Property="Foreground" Value="{DynamicResource IcomText3}" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- [ ] **Step 4: Replace `ContextMenuItemTemplate` (lines ~744–759)**

Find `<DataTemplate x:Key="ContextMenuItemTemplate">` and replace it (through its closing `</DataTemplate>`) with:

```xml
<DataTemplate x:Key="ContextMenuItemTemplate">
    <MenuItem Command="{Binding Command}" IsEnabled="{Binding IsEnabled}">
        <MenuItem.Header>
            <Grid MinWidth="220">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="20" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                <Ellipse Width="7" Height="7" VerticalAlignment="Center">
                    <Ellipse.Style>
                        <Style TargetType="Ellipse">
                            <Setter Property="Fill" Value="Transparent" />
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding IsChecked}" Value="True">
                                    <Setter Property="Fill" Value="{DynamicResource IcomAccent}" />
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Ellipse.Style>
                </Ellipse>
                <TextBlock Grid.Column="1" Text="{Binding DisplayName}" FontSize="14"
                           Foreground="{DynamicResource IcomText}" VerticalAlignment="Center" />
            </Grid>
        </MenuItem.Header>
    </MenuItem>
</DataTemplate>
```

- [ ] **Step 5: Replace `ContextMenuSubItemTemplate` (lines ~760–766)**

Find `<HierarchicalDataTemplate x:Key="ContextMenuSubItemTemplate"` and replace through its closing tag with:

```xml
<HierarchicalDataTemplate x:Key="ContextMenuSubItemTemplate" ItemsSource="{Binding Children}">
    <MenuItem Command="{Binding Command}"
              Header="{Binding DisplayName}"
              ItemContainerTemplateSelector="{Binding RelativeSource={RelativeSource AncestorType={x:Type ContextMenu}}, Path=ItemContainerTemplateSelector}"
              ItemsSource="{Binding Children}"
              UsesItemContainerTemplate="True" />
</HierarchicalDataTemplate>
```

- [ ] **Step 6: Replace `ContextMenuSeparatorTemplate` (lines ~767–769)**

Find `<DataTemplate x:Key="ContextMenuSeparatorTemplate">` and replace through its closing tag with:

```xml
<DataTemplate x:Key="ContextMenuSeparatorTemplate">
    <Separator />
</DataTemplate>
```

- [ ] **Step 7: Replace `ContextMenuSectionTitleTemplate` (lines ~770–780)**

Find `<DataTemplate x:Key="ContextMenuSectionTitleTemplate">` and replace through its closing tag with:

```xml
<DataTemplate x:Key="ContextMenuSectionTitleTemplate">
    <MenuItem IsEnabled="False" IsHitTestVisible="False">
        <MenuItem.Header>
            <TextBlock Text="{Binding DisplayName}"
                       FontSize="13" FontWeight="Medium"
                       Foreground="{DynamicResource IcomText3}"
                       Margin="8,6,0,2" />
        </MenuItem.Header>
    </MenuItem>
</DataTemplate>
```

---

## Task 11 — Build verification

- [ ] **Step 1: Run dotnet restore**

```
dotnet restore AudioInOut.vs15.sln
```

- [ ] **Step 2: Build**

```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```

Expected: `Build succeeded.` with 0 errors. One known harmless warning about GitVersion/AssemblyInfo is acceptable.

- [ ] **Step 3: Commit all changes**

```
git add AudioInOut/UI/Themes/IcomLight.xaml AudioInOut/UI/Themes/IcomDark.xaml
git add AudioInOut/UI/ViewModels/AppearanceSettingsViewModel.cs
git add AudioInOut/AppSettings.cs AudioInOut/App.xaml.cs
git add AudioInOut/UI/ViewModels/SettingsWindowViewModel.cs
git add AudioInOut/UI/Views/SettingsWindow.xaml AudioInOut/UI/Views/SettingsWindow.xaml.cs
git add AudioInOut/App.xaml AudioInOut/AudioInOut.csproj
git commit -m "feat(ui): implement Intercom design for settings window and tray menu"
```
