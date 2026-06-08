# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Fork of [EarTrumpet](https://github.com/File-New-Project/EarTrumpet), renamed **AudioInOut** — a Windows audio tray app (C#/WPF). The goal is to add **input-device switching** alongside the existing output-device infrastructure, and to clean up telemetry/onboarding cruft from the upstream.

**Repo:** `C:\Users\edtra\FilesMain\CLAUDE-SPACE\app_AudioInOut`  
**Active branch:** `rebrand/audioinout`

## The Feature

Add global default **microphone/capture endpoint switching** to the tray UI, reusing and generalizing the existing output-device infrastructure.

Windows audio endpoints have two orthogonal axes:
- **Data flow**: `eRender` (output/speakers) vs. `eCapture` (input/microphone)
- **Role**: `eConsole`, `eCommunications`, `eMultimedia`

"Switch default mic" is the same category of operation as "switch default speaker" — same API (`IMMDeviceEnumerator::GetDefaultAudioEndpoint`/`SetDefaultAudioEndpoint`), different flow parameter. The existing output code should become flow-agnostic rather than copying it for input.

## Architecture Guidance

### Generalize, don't branch
Refactor output-specific services toward a shared endpoint model:
- `flow` (render/capture)
- `role` (console/communications/multimedia)
- endpoint metadata: id, name, state, default flags
- actions: enumerate, get default, set default

Policy/action layer must stay separated from menu/view code so UX can change without rewriting endpoint logic.

### UI
v1 acceptable: tray submenu `Recording devices`. Architecture should also support a unified `Devices` surface with Output and Input sections, and an optional role selector (Default / Communications / Both).

### Constraints
- Do **not** assume per-app mic routing is achievable — `IAudioClient` session routing for capture is not universally supported; scope v1 to global default switching only.
- Some apps cache devices; changing the default does not guarantee immediate effect for all running apps.
- Respect `PKEY_AudioDevice_NeverSetAsDefaultEndpoint` when enumerating selectable devices — filter or explain unavailable endpoints rather than surfacing them.
- Communications-role behavior may differ from console default behavior.

## Build

```
dotnet restore AudioInOut.vs15.sln
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```
Output: `Build\Debug\AudioInOut.exe`. One known harmless warning: GitVersion conflicts with AssemblyInfo.cs version attributes.

## Key References

| Topic | URL |
|-------|-----|
| EarTrumpet repo | https://github.com/File-New-Project/EarTrumpet |
| `IMMDeviceEnumerator::GetDefaultAudioEndpoint` | https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint |
| Windows default audio endpoint selection | https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection |
| `PKEY_AudioDevice_NeverSetAsDefaultEndpoint` | https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/pkey-audiodevice-neversetasdefaultendpoint |

## Non-Goals (v1)

- Per-app microphone routing
- Committing to tray submenu as final UX
- Copying existing output implementation verbatim rather than generalizing it

## Session Notes

- `handoff.md` — last session state and next steps
- `firstprinciples.md` — original design brief
- `tasks/backlog.md` — backlog (inside fork tasks dir if exists)
