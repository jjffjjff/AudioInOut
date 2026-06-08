# AudioInOut

A fork of [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) by File-New-Project (MIT License), extended to support **input device switching** alongside the existing output device infrastructure.

## What's different from EarTrumpet

- Input (recording) device switching from the tray menu
- Telemetry and onboarding removed
- Redesigned tray context menu and settings window
- Renamed throughout to AudioInOut

## Build

```
dotnet restore AudioInOut.vs15.sln
"C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\msbuild.exe" AudioInOut.vs15.sln /p:Configuration=Debug /p:Platform=x86 /t:AudioInOut /v:minimal
```

Output: `Build\Debug\AudioInOut.exe`

## Upstream

Based on EarTrumpet by Rafael Rivera, David Golden, and Dave Amenta.
Original source: https://github.com/File-New-Project/EarTrumpet
