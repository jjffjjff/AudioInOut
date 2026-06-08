# Backlog

## Cleanup

- [ ] **Nuke welcome/onboarding modal** — the dialog that appears on first launch (telemetry consent + welcome screen). Remove entirely.
- [ ] **Strip all telemetry** — remove Bugsnag error reporting, telemetry opt-in setting (`IsTelemetryEnabled`), and all related code paths. Delete Bugsnag NuGet package reference from `AudioInOut.csproj`.

Relevant files to target:
- `AudioInOut/App.xaml.cs` — startup logic that triggers the welcome flow
- `AudioInOut/UI/ViewModels/EarTrumpetAboutPageViewModel.cs` — `IsTelemetryEnabled` property
- `AudioInOut/AppSettings.cs` — `IsTelemetryEnabled` setting
- `AudioInOut/Diagnosis/ErrorReporter.cs` — Bugsnag integration
- `AudioInOut/AudioInOut.csproj` — Bugsnag NuGet reference
- Settings window XAML — telemetry toggle UI

## Sub-projects

- [ ] **Sub-project 2: Tray menu redesign** — section titles, bottom icon row, input/output sections (see `tasks/right-click-menu_research.md`)
- [ ] **Sub-project 3: Floating mixer rework** — pin to detach, horizontal/vertical toggle
- [ ] **Sub-project 4: Settings rework** — new options, remove legacy, Raycast/external trigger IPC stub
- [ ] **Sub-project 5: Extensibility stubs** — IPC interface design, window-pets-daemon hook
