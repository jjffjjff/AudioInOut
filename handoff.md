# AudioInOut Handoff — 2026-06-08

## Latest Session — app icon

- **New icon:** black squircle with white "I\O" — generated via Python/Pillow
- **Files created/replaced:**
  - `AudioInOut/Assets/Icon-Dark.ico` — multi-size (16/24/32/48/64/128/256px)
  - `AudioInOut/Assets/Icon-Light.ico` — same design (both themes use black squircle)
  - `AudioInOut/Assets/AppIcon.png` — 256px PNG for About section
- **Wired into:**
  - `AudioInOut.csproj` — `ApplicationIcon` already pointed to `Icon-Light.ico`; `AppIcon.png` added as `<Resource>`
  - `SettingsWindow.xaml` About section — replaced `Border`+`TextBlock "A·IO"` placeholder with `<Image Source="...AppIcon.png">`
  - Tray icon already references `Icon-Dark.ico` / `Icon-Light.ico` via `TaskbarIconSource.cs`
- **Build:** clean


## Previous Session — cleanup pass (#1 #3 #4 #5)

- **#1 Tray icon resource keys** — `TaskbarIconSource.cs:122,124` used `"EarTrumpetIconDark/Light"`; renamed to `"AudioInOutIconDark/Light"` to match `App.xaml` definitions.
- **#3 About page links** — `OpenGitHubIssueChooser()` and `OpenPrivacyPolicy()` wired via `ProcessHelper.StartNoThrow`; EarTrumpet GitHub issues + eartrumpet.app/privacy as placeholders. `OpenAbout()` left as stub (not bound in XAML).
- **#4 Crash-path branding** — `App.xaml.cs:186` URL replaced with `ms-settings:fonts`; `Resources.resx` `CriticalFailureFontLookupHelpText` body updated to match (no eartrumpet.app URL remains).
- **#5 Dead VM deletion** — deleted `AdvertisedCategorySettingsViewModel.cs`, `SettingsCategoryViewModel.cs`, `SettingsAppItemViewModel.cs`; removed their csproj entries; removed dead `SettingsAppItemViewModel` DataTemplate from `App.xaml`. `ModalDialogViewModel` kept (used by FlyoutViewModel/FullWindowViewModel). `SettingsPageHeaderViewModel` + `SettingsPageViewModel` kept (active base classes).
- **Build:** clean (known GitVersion warnings only).


## Previous Session — tray right-click menu placement

- **Symptom:** menu opened to LEFT of cursor regardless of screen position.
- **Root cause:** prior fixes both coded left-alignment under the false belief that left = "standard tray behavior". `PlacementMode.Top` with `PlacementTarget=null` falls back to the active window's bounds — removing the override alone still opened leftward.
- **Fix:** kept `SetWindowPos` in `Opened`, inverted math to `x = point.X`, right-edge flip via `System.Windows.Forms.Screen.FromPoint(...).WorkingArea` for multi-monitor correctness.
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
| `AdvertisedCategorySettingsViewModel`, `SettingsCategoryViewModel`, `SettingsAppItemViewModel` deleted | Confirmed dead — no callers; `SettingsAppItemViewModel` DataTemplate also removed from App.xaml |
| `ModalDialogViewModel`, `SettingsPageHeaderViewModel`, `SettingsPageViewModel` kept | Actively used — not dead despite old architecture label |
| About page placeholder URLs: EarTrumpet GitHub issues + eartrumpet.app/privacy | No AudioInOut-specific URLs exist yet; replace when/if published |
| App icon: black squircle + white "I\O", single design for both light/dark tray themes | Squircle provides own contrast; user-specified design |
| Font crash URL → `ms-settings:fonts` | No AudioInOut help page; opens Windows font settings directly |

## Settled Artifacts

### Build command
```
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln -p:Configuration=Debug -p:Platform=x86 -t:AudioInOut -v:minimal
```
Note: use `-p:` not `/p:` — MSBuild.rsp in that directory strips `/` flags.  
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
| Tray icon resource keys fixed | ✅ Done |
| About page links wired | ✅ Done (placeholder URLs) |
| Crash-path EarTrumpet branding removed | ✅ Done |
| Dead legacy settings VMs deleted | ✅ Done (3 files + DataTemplate) |
| App icon (I\O squircle) | ✅ Done — tray, About page, exe |
| Build clean | ✅ Confirmed |
| Floating mixer | ✅ Kept intentionally |
| Input device switching (original feature goal) | ✅ Done — already implemented via RecordingCollectionViewModel + AudioDeviceKind.Recording in tray menu |
| Settings smoke test | ⚠️ Not formally done |

## Open Questions

- **Settings smoke test:** theme dropdown, hotkey conflict warning, reset button, scroll-spy — never formally validated after redesign.

## Next Steps (priority order)

1. **Settings smoke test** — scroll-spy, sidebar nav, theme dropdown, hotkey conflict warning, reset button. Check: theme persistence, scroll-spy race in `SettingsWindow.xaml.cs:62-96`, `StartWithWindows` writes `Process.MainModule.FileName` (may be non-launchable under MSIX).

2. **Replace About page placeholder URLs** — once/if the fork has a published home, replace EarTrumpet URLs in `AudioInOutAboutPageViewModel.cs`.

## Repo

- **Branch:** `rebrand/audioinout`
- **Repo:** `C:\Users\edtra\FilesMain\CLAUDE-SPACE\app_AudioInOut`
- **Backlog:** `tasks/backlog.md`
- **Architecture notes:** `CLAUDE.md`, `firstprinciples.md`
