# AudioInOut Handoff — 2026-06-09

## Latest Session — ship + repo cleanup

- **Committed** all pending rebrand changes (17 files: VM deletions, AppIcon.png, icons, manifest, settings window)
- **Fix:** `SettingsWindow.xaml.cs` — value-tuple syntax → `Tuple<>` for compiler compat
- **Fix:** `Package.appxmanifest` version → `2.3.0.132`
- **New GitHub repo:** https://github.com/jjffjjff/AudioInOut — private, standalone (not a fork)
- **Removed 106 EarTrumpet files:** `.azure-pipelines.yml`, `.chocolatey/`, `crowdin.yml`, `Graphics/`, `Resources/`, `Packaging/` (25-language Store PDPs), `CHANGELOG.md`, `CONTRIBUTING.md`, `PRIVACY.md`, `README.md`, `COMPILING.md`
- **GitVersionTask removed entirely** — csproj, packages.config, prebuild.ps1, GitVersion.yml
- **EarTrumpet.Package + EarTrumpet.ColorTool removed from .sln** (Package files remain on disk; ColorTool deleted)
- **Build: zero warnings**


## Previous Session — settings smoke test + fixes

### Analysis findings
- **Scroll-spy** (`SettingsWindow.xaml.cs:62–96`): ⚠️ `TransformToAncestor` called with no `IsLoaded` guard in both handlers; ⚠️ `_isScrollSpy` not in `try/finally` — exception between lines 92–95 would permanently lock sidebar nav. Cosmetic risk only in practice.
- **Sidebar nav**: ✅ 6 nav items match 6 `GetSections()` entries in correct order; `AudioInOutShortcutsPageViewModel` and `AudioInOutAboutPageViewModel` DataTemplates present; ⚠️ stale resx label `SettingsOpenEarTrumpetText` in Shortcuts section (still says "Open EarTrumpet").
- **Theme dropdown**: ✅ full chain — `Appearance.SelectedTheme` → `AppSettings.AppearanceTheme` → `ISettingsBag` → restart survival via `ContinueStartup():108`. Constructor loads saved value; `"System"` re-evaluates `IsLightTheme` at runtime.
- **StartWithWindows**: ⚠️→✅ fixed — `Process.MainModule.FileName` under MSIX resolves to version-stamped `WindowsApps\` path, goes stale after updates. Fixed: `App.HasIdentity` guard in getter + setter; `IsStartupSettingEnabled => !App.HasIdentity` property; XAML toggle binds `IsEnabled` to it.

### Fixes applied
- **SearchBox vertical clipping** — `Style="{x:Null}"` + `Margin=0 Padding=0 MinWidth=0 MinHeight=0` as local values bypasses global TextBox style (`Margin=12` was the culprit).
- **SearchBox caret invisible** — `CaretBrush="{DynamicResource IcomText}"` added as local value. Global style hard-codes `CaretBrush = ApplicationTextLightTheme` (near-black), invisible on dark `IcomField` with `Style="{x:Null}"`.
- **Search highlight** — replaced section opacity blink with inline `Run` replacement: matching TextBlock text split into pre/match/post Runs, match gets `Background = Color.FromArgb(200, 255, 165, 0)`. `ClearHighlights()` restores original `.Text` on each keystroke.
- **About section links removed** — Privacy Policy, Send Feedback, Troubleshoot hyperlinks deleted from About DataTemplate. Attribution line kept (MIT requirement).
- **StartWithWindows MSIX guard** — `AppBehaviorViewModel.cs`: getter + setter guard with `if (App.HasIdentity) return`; `IsStartupSettingEnabled` exposed for XAML `IsEnabled` binding.

### Files changed
- `AudioInOut/UI/Views/SettingsWindow.xaml` — SearchBox CaretBrush + layout fix; About links removed; StartWithWindows `IsEnabled` binding
- `AudioInOut/UI/Views/SettingsWindow.xaml.cs` — `ApplyHighlights` / `ClearHighlights`; removed opacity animation
- `AudioInOut/UI/ViewModels/AppBehaviorViewModel.cs` — `IsStartupSettingEnabled`; `HasIdentity` guard on `StartWithWindows`

**Build:** clean


## Previous Session — app icon

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
| Entire addon infrastructure deleted | Zero addons loaded; MEF + Newtonsoft.Json also removed as a result |
| `FocusedDeviceViewModel.IsApplicable` → `false` (hardcoded) | No addon content items remain; focused device dialog never opens |
| `Newtonsoft.Json` NuGet removed | Only used in deleted `AudioInOutAddon.cs` |
| `Serializer.cs` kept | Uses `System.Xml.XmlSerializer`, not Newtonsoft; still used by `WindowsStorageSettingsBag` and `RegistrySettingsBag` |
| Floating mixer kept as-is | Intentional — not a pruning target |
| Attribution string "Based on EarTrumpet by File-New-Project (MIT License)" kept in `SettingsWindow.xaml` | MIT License requires it |
| `EarTrumpet.Package/` kept on disk, removed from .sln | No MSIX packaging needed for solo use; can restore later |
| `EarTrumpet.ColorTool/` deleted entirely | Separate dev utility, no relation to AudioInOut |
| GitVersionTask removed entirely | Solo dev + Debug exe; no need for auto-versioning. Result: zero-warning build |
| New standalone GitHub repo (not fork) | Clean repo identity; upstream still reachable via `upstream` remote |
| `AdvertisedCategorySettingsViewModel`, `SettingsCategoryViewModel`, `SettingsAppItemViewModel` deleted | Confirmed dead — no callers |
| `ModalDialogViewModel`, `SettingsPageHeaderViewModel`, `SettingsPageViewModel` kept | Actively used |
| About page placeholder URLs: EarTrumpet GitHub issues + eartrumpet.app/privacy | No AudioInOut-specific URLs exist yet; replace when/if published |
| App icon: black squircle + white "I\O", single design for both light/dark tray themes | Squircle provides own contrast; user-specified design |
| Font crash URL → `ms-settings:fonts` | No AudioInOut help page; opens Windows font settings directly |
| `StartWithWindows` guarded by `App.HasIdentity` | Under MSIX, `Process.MainModule.FileName` goes stale after updates; correct MSIX mechanism is `StartupTask` manifest entry (not yet implemented) |
| About page links (Privacy Policy, Send Feedback, Troubleshoot) removed | No AudioInOut URLs exist; re-add when published |
| SearchBox `Style="{x:Null}"` + explicit `CaretBrush` | Global TextBox style hard-codes caret to `ApplicationTextLightTheme` (near-black), invisible on dark background without this override |

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
| About page links wired | ✅ Done (placeholder URLs; links in settings removed) |
| Crash-path EarTrumpet branding removed | ✅ Done |
| Dead legacy settings VMs deleted | ✅ Done |
| App icon (I\O squircle) | ✅ Done — tray, About page, exe |
| Input device switching | ✅ Done — RecordingCollectionViewModel + AudioDeviceKind.Recording |
| Settings smoke test | ✅ Done — findings documented above |
| Build clean (zero warnings) | ✅ Confirmed |
| Repo cleanup (EarTrumpet distribution files) | ✅ Done |
| GitVersionTask removed | ✅ Done |
| Standalone GitHub repo | ✅ github.com/jjffjjff/AudioInOut |

## Open / Low Priority

- Stale resx label `SettingsOpenEarTrumpetText` in Shortcuts section — cosmetic, low priority
- Scroll-spy `IsLoaded` guard — theoretical, not a user-visible bug
- Replace About page placeholder URLs — when/if project is published

## Settled Artifacts

### Build command (PowerShell only — bash strips `/` flags)
```powershell
cd "C:\Users\edtra\FilesMain\CLAUDE-SPACE\app_AudioInOut"
& "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```
Output: `Build\Debug\AudioInOut.exe`. **Zero warnings.**

## Repo

- **Branch:** `rebrand/audioinout`
- **GitHub:** https://github.com/jjffjjff/AudioInOut
- **Local:** `C:\Users\edtra\FilesMain\CLAUDE-SPACE\app_AudioInOut`
- **Backlog:** `tasks/backlog.md`
- **Architecture notes:** `CLAUDE.md`, `firstprinciples.md`
