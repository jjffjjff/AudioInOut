# Settings Window Redesign

**Date:** 2026-06-02  
**Branch:** rebrand/audioinout

## Goal

Replace the two-level category-tile → pages navigation with a single scrollable settings page, a flat left sidebar, and a search box anchored at the bottom of the sidebar.

## Layout

```
┌─────────────────────┬──────────────────────────────────────┐
│  Left sidebar 220px │  Right: ScrollViewer                 │
│                     │                                       │
│  App behavior       │  ── App behavior ──────────────────  │
│  Scroll behavior    │  [log scale toggle]                   │
│  Floating Mixer     │  [start with windows toggle]          │
│  Shortcuts          │                                       │
│  About              │  ── Scroll behavior ────────────────  │
│                     │  [scroll wheel in tray]               │
│                     │  [global mouse wheel hook]            │
│  ┌───────────────┐  │                                       │
│  │  Search...    │  │  ── Floating Mixer ─────────────────  │
│  └───────────────┘  │  [placeholder toggle]                 │
└─────────────────────┴──────────────────────────────────────┘
```

- Window shell unchanged: title bar, acrylic background, dialog overlay, resize behavior.
- Left sidebar: `ListView` of 5 section names. Clicking scrolls right panel to that section. Highlighted item tracks current visible section (scroll-spy).
- Bottom of left sidebar: `TextBox` for search, always visible, docked.
- Right: single `ScrollViewer` > `StackPanel` > 5 named section `Border` anchors stacked vertically.
- No back button, no home screen, no category tiles — permanent single view.

## Sections

| Section | Settings |
|---------|----------|
| App behavior | Log scale toggle, Start with Windows toggle |
| Scroll behavior | Use scroll wheel in tray, Use global mouse wheel hook |
| Floating Mixer | Placeholder toggle (no-op, persisted for future use) |
| Shortcuts | Hotkey bindings (existing UI unchanged) |
| About | Version info, links (existing UI unchanged) |

## ViewModel Changes

**Delete:**
- `BackstackViewModel`
- `SettingsCategoryViewModel`
- `SettingsPageHeaderViewModel`
- `EarTrumpetCommunitySettingsPageViewModel`
- `EarTrumpetLegacySettingsPageViewModel`
- All navigation-related properties/commands from `SettingsWindowViewModel` (GoHome, Selected, Categories, Backstack, etc.)

**Keep (as section VMs):**
- `EarTrumpetMouseSettingsPageViewModel` — gains `UseLogarithmicVolume` (moved from deleted Community VM)
- `EarTrumpetShortcutsPageViewModel` — unchanged
- `EarTrumpetAboutPageViewModel` — unchanged

**New `AppBehaviorViewModel`:**
- `UseLogarithmicVolume` — moved from `EarTrumpetCommunitySettingsPageViewModel`
- `StartWithWindows` — bool, reads/writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- `FloatingMixerPlaceholderEnabled` — bool, no-op, persisted in `AppSettings`

`EarTrumpetMouseSettingsPageViewModel` is unchanged — covers Scroll behavior only.

**New `SettingsWindowViewModel` exposes:**
- `AppBehavior` (`AppBehaviorViewModel`)
- `ScrollBehavior` (`EarTrumpetMouseSettingsPageViewModel`)
- `Shortcuts` (Shortcuts VM)
- `About` (About VM)
- `SearchText` — string, two-way bound to search TextBox
- `SelectedSectionIndex` — int, drives left ListView selection

## Scroll-spy

Pure view concern, implemented in code-behind (`SettingsWindow.xaml.cs`):

1. Each section gets a named `Border`: `SectionAppBehavior`, `SectionScrollBehavior`, `SectionFloatingMixer`, `SectionShortcuts`, `SectionAbout`.
2. `ScrollViewer.ScrollChanged` handler: for each named border, call `border.TransformToAncestor(scrollViewer)` to get its Y offset relative to the viewport. The section with the smallest non-negative Y offset is "current". Update `SelectedSectionIndex` on the VM.
3. Left `ListView.SelectionChanged` (user click): call `ScrollViewer.ScrollToVerticalOffset` with the target border's offset.

## Search

- `TextBox` at bottom of left panel, `Text` two-way bound to `SettingsWindowViewModel.SearchText`.
- On `SearchText` changed: walk the right `StackPanel`'s children looking for `TextBlock.Text` containing the query (case-insensitive, trim). Scroll `ScrollViewer` to the first containing section border. Update `SelectedSectionIndex` to match.
- No filtering or hiding of sections — navigate-to only.
- Empty search: no action.

## Files Affected

| File | Action |
|------|--------|
| `UI/Views/SettingsWindow.xaml` | Major rewrite of content area |
| `UI/Views/SettingsWindow.xaml.cs` | Add scroll-spy + search code-behind |
| `UI/ViewModels/SettingsWindowViewModel.cs` | Strip nav, add section VM properties |
| `UI/ViewModels/AppBehaviorViewModel.cs` | New — StartWithWindows, FloatingMixerPlaceholder, UseLogarithmicVolume |
| `UI/ViewModels/EarTrumpetMouseSettingsPageViewModel.cs` | Unchanged (Scroll behavior) |
| `UI/ViewModels/BackstackViewModel.cs` | Delete |
| `UI/ViewModels/SettingsCategoryViewModel.cs` | Delete |
| `UI/ViewModels/SettingsPageHeaderViewModel.cs` | Delete |
| `UI/ViewModels/EarTrumpetCommunitySettingsPageViewModel.cs` | Delete |
| `UI/ViewModels/EarTrumpetLegacySettingsPageViewModel.cs` | Delete |
| `AppSettings.cs` | Add `FloatingMixerPlaceholderEnabled` |
| `UI/Controls/MenuItemTemplateSelector.cs` | Remove refs to deleted VMs if any |

## Non-Goals

- Per-section collapse/expand
- Fuzzy search or search result highlighting
- Settings import/export
- Any floating mixer actual functionality
