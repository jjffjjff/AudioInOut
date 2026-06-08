# AudioInOut Handoff — 2026-06-08

## Latest Session — tray right-click menu placement

- **Symptom:** menu opened to LEFT of cursor regardless of screen position.
- **Root cause:** prior fixes (CustomPopupPlacementCallback + SetWindowPos override in `Opened`) both intentionally coded left-alignment (`x = point.X - menuWidth`) under the false belief that left = "standard tray behavior". Removing the override alone did not help — `PlacementMode.Top` with `PlacementTarget=null` falls back to the active window's bounds, so WPF still placed it leftward.
- **Fix:** kept `SetWindowPos` in `Opened` but inverted math to left-align (`x = point.X`), with right-edge flip via `System.Windows.Forms.Screen.FromPoint(...).WorkingArea` for multi-monitor correctness.
- **File:** `AudioInOut/UI/Helpers/ShellNotifyIcon.cs:320-348`
- **Status:** confirmed working by user.


## Original Goal

- Fork of EarTrumpet renamed to AudioInOut — add global default mic/capture endpoint switching
- Rebrand all remaining `EarTrumpet` C# type names and source files to `AudioInOut` prefix
- Remove dead/irrelevant upstream features to keep the codebase lean
- Build must stay clean throughout (MSBuild x86 Debug)

## Decisions

| Decision | Why |
|---|---|
| `EarTrumpetAddon` → `AudioInOutAddon`, all 5 `IEarTrumpet*` interfaces renamed | Completes type-level rebrand; file history preserved via `git mv` |
| `EarTrumpetActionsAddon`, `EarTrumpetAction`, `EarTrumpetEventKind` + 3 UI VMs renamed | Same — all type names now carry `AudioInOut` prefix |
| `EarTrumpetEventKind_Startup/Shutdown` resx keys renamed in 30 locale files + `Resources.Designer.cs` | User requested; 4 locale files had no entry for these keys and were left untouched |
| Entire `Addons/EarTrumpet.Actions/` subtree deleted | Trigger/condition/action automation engine — unrelated to fork goal; no user data at risk |
| `AddonManager.Load(bool)` → `Load()`, `LoadInternalAddons()` removed | No internal addons remain |
| Entire addon infrastructure deleted (`AddonHost`, `AddonManager`, `AddonResolver`, all 5 `IAudioInOut*` interfaces, `AudioInOutAddon.cs`, `AudioInOutAddonManifest.cs`, `AudioInOutAddonExtensions.cs`, `AddonAboutPageViewModel.cs`) | Zero addons loaded; MEF + Newtonsoft.Json also removed as a result |
| `FocusedDeviceViewModel.IsApplicable` → `false` (hardcoded) | No addon content items remain; focused device dialog never opens |
| `Newtonsoft.Json` NuGet removed from packages.config / csproj / App.config | Only used in deleted `AudioInOutAddon.cs` |
| `Serializer.cs` kept | Uses `System.Xml.XmlSerializer`, not Newtonsoft; still used by `WindowsStorageSettingsBag` and `RegistrySettingsBag` |
| Floating mixer kept as-is | Intentional — not a pruning target |
| Attribution string "Based on EarTrumpet by File-New-Project (MIT License)" kept in `SettingsWindow.xaml` | MIT License requires it |
| `EarTrumpet.Package/` and `EarTrumpet.ColorTool/` NOT touched | Store package identity; renaming breaks build chain |

## Settled Artifacts

### Build command
```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```
Output: `Build\Debug\AudioInOut.exe`. Known harmless warning: GitVersion conflicts with AssemblyInfo.cs.

### Renamed files (all via `git mv`)

**Extensibility/**
- `EarTrumpetAddon.cs` → `AudioInOutAddon.cs` (then deleted in pruning)
- `EarTrumpetAddonManifest.cs` → `AudioInOutAddonManifest.cs` (then deleted)
- `IEarTrumpetAddonAppContent.cs` → `IAudioInOutAddonAppContent.cs` (then deleted)
- `IEarTrumpetAddonDeviceContent.cs` → `IAudioInOutAddonDeviceContent.cs` (then deleted)
- `IEarTrumpetAddonEvents.cs` → `IAudioInOutAddonEvents.cs` (then deleted)
- `IEarTrumpetAddonNotificationAreaContextMenu.cs` → `IAudioInOutAddonNotificationAreaContextMenu.cs` (then deleted)
- `IEarTrumpetAddonSettingsPage.cs` → `IAudioInOutAddonSettingsPage.cs` (then deleted)

**Extensions/**
- `EarTrumpetAddonExtensions.cs` → `AudioInOutAddonExtensions.cs` (then deleted)

**UI/ViewModels/**
- `EarTrumpetAboutPageViewModel.cs` → `AudioInOutAboutPageViewModel.cs`
- `EarTrumpetShortcutsPageViewModel.cs` → `AudioInOutShortcutsPageViewModel.cs`
- `EarTrumpetMouseSettingsPageViewModel.cs` → `AudioInOutMouseSettingsPageViewModel.cs`

**Deleted:** `Addons/EarTrumpet.Actions/` entire subtree (66 csproj entries removed)

## Current State

| Area | Status |
|---|---|
| All `EarTrumpet*` C# type names rebranded | ✅ Done |
| All `EarTrumpet*` source files renamed | ✅ Done |
| Resx keys `EarTrumpetEventKind_*` renamed | ✅ Done |
| Actions addon deleted | ✅ Done |
| Addon infrastructure fully deleted | ✅ Done |
| Newtonsoft.Json removed | ✅ Done |
| Build clean | ✅ Confirmed |
| Floating mixer | ✅ Kept intentionally |
| Input device switching (original feature goal) | ✅ Done — already implemented via RecordingCollectionViewModel + AudioDeviceKind.Recording in tray menu |
| Settings smoke test | ⚠️ Not formally done |

## Open Questions

- **Settings smoke test:** theme dropdown, hotkey conflict warning, reset button, scroll-spy — never formally validated after redesign.

## Next Steps (priority order)

1. **🔴 Fix tray icon crash** — `TaskbarIconSource.cs:122,124` looks up `"EarTrumpetIconDark"` / `"EarTrumpetIconLight"` but `App.xaml` only defines `AudioInOutIconLight` / `AudioInOutIconDark`. Null → tray icon dead → whole app broken. Rename the two resource keys.

2. **Settings smoke test** — scroll-spy, sidebar nav, theme dropdown, hotkey conflict warning, reset button. Check: theme persistence, scroll-spy race in `SettingsWindow.xaml.cs:62-96`, `StartWithWindows` writes `Process.MainModule.FileName` (may be non-launchable under MSIX).

3. **Wire About page links** — `AudioInOutAboutPageViewModel.cs:45-47`: `OpenGitHubIssueChooser()`, `OpenAbout()`, `OpenPrivacyPolicy()` are empty stubs; three visible hyperlinks in Settings → About do nothing.

4. **Fix crash-path EarTrumpet branding** — `App.xaml.cs:186` links to `eartrumpet.app/jmp/fixfonts`; `Resources.Designer.cs:487,496` still says "EarTrumpet couldn't start". Replace with neutral URL + resx update.

5. **Delete legacy settings VM family** — `AdvertisedCategorySettingsViewModel`, `SettingsCategoryViewModel`, `SettingsPageHeaderViewModel`, `SettingsAppItemViewModel`, `ModalDialogViewModel` still on disk from old architecture. Commit `bbf5149` claimed these were deleted — they weren't. Also: `AudioInOutAboutPageViewModel` inherits `SettingsPageViewModel` vestigially.

## Repo

- **Branch:** `rebrand/audioinout`
- **Repo:** `C:\Users\edtra\FilesMain\CLAUDE-SPACE\app_AudioInOut`
- **Backlog:** `tasks/backlog.md`
- **Architecture notes:** `CLAUDE.md`, `firstprinciples.md`
