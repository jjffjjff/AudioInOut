Use this as the handoff note.

## Target
Add **input-device switching** to AudioInOut (fork of EarTrumpet) in a way that reuses existing output-device concepts, without locking the app into this exact UX. The upstream EarTrumpet supports default audio device control and is open-source here: [github.com/File-New-Project/EarTrumpet](https://github.com/File-New-Project/EarTrumpet). [github](https://github.com/File-New-Project/EarTrumpet)

## Current understanding
The existing tray/device behavior is built around Windows audio endpoints. Windows explicitly models endpoints by:
- data flow: `eRender` vs `eCapture`
- role: `eConsole`, `eCommunications`, etc. [learn.microsoft](https://learn.microsoft.com/fr-fr/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint)

That means “switch default microphone” is the same category of operation as “switch default speaker,” but applied to **capture** endpoints instead of render endpoints. [learn.microsoft](https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint)

## Recommended scope
Treat this as **global input switching**, not per-app mic routing, for v1. The research supports default endpoint selection much more clearly than universal per-app input reassignment. [learn.microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection)

Candidate UX directions:
- Tray submenu: `Recording devices`
- Unified `Devices` surface with Output and Input sections
- Optional role selector: Default / Communications / Both
- Keep architecture extensible so future work can add per-app input behavior if Windows/API reality supports it cleanly [learn.microsoft](https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint)

## Architecture guidance
Prefer a generalized device model instead of adding microphone logic as a one-off branch. The abstraction should be able to represent:
- flow: render/capture
- role: console/communications/multimedia if relevant
- endpoint metadata: id, name, state, default flags
- actions: enumerate, get current default, set default [maul-esel.github](https://maul-esel.github.io/COM-Classes/AHK_Lv1.1/MMDeviceEnumerator)

Good direction:
- Refactor output-specific code toward endpoint-flow-agnostic services.
- Add UI composition that can render either output or input device lists.
- Keep the policy/action layer separated from menu/view code so later refactors can swap UX without rewriting endpoint logic. [learn.microsoft](https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint)

## Constraints / caveats
Do not assume “set default mic” means every running app will immediately follow. Some apps cache devices, and communications-role behavior may differ from general default-device behavior. [learn.microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection)

Also account for endpoints that should not be user-selectable as defaults. Windows documents `PKEY_AudioDevice_NeverSetAsDefaultEndpoint`, including flow/role masks, which may matter when filtering device choices or explaining unavailable devices. [learn.microsoft](https://learn.microsoft.com/is-is/windows-hardware/drivers/audio/pkey-audiodevice-neversetasdefaultendpoint)

## Useful resources
- EarTrumpet repo: [github.com/File-New-Project/EarTrumpet](https://github.com/File-New-Project/EarTrumpet) [github](https://github.com/File-New-Project/EarTrumpet)
- EarTrumpet site: [eartrumpet.app](https://eartrumpet.app) [eartrumpet](https://eartrumpet.app)
- Microsoft `IMMDeviceEnumerator::GetDefaultAudioEndpoint`: [learn.microsoft.com/.../immdeviceenumerator-getdefaultaudioendpoint](https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint) [learn.microsoft](https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint)
- Windows default audio endpoint selection: [learn.microsoft.com/.../default-audio-endpoint-selection](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection) [learn.microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection)
- `PKEY_AudioDevice_NeverSetAsDefaultEndpoint`: [learn.microsoft.com/.../pkey-audiodevice-neversetasdefaultendpoint](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/pkey-audiodevice-neversetasdefaultendpoint) [learn.microsoft](https://learn.microsoft.com/is-is/windows-hardware/drivers/audio/pkey-audiodevice-neversetasdefaultendpoint)

## Non-goals for now
- No commitment to per-app microphone routing.
- No assumption that tray submenu is the final UX.
- No assumption that the existing output implementation should be copied verbatim rather than generalized. [learn.microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/audio/default-audio-endpoint-selection)

## Suggested handoff prompt
Refactor AudioInOut’s device management so output-specific logic becomes endpoint-flow-agnostic, then add support for capture endpoints and default input-device switching. Keep UI flexible: tray submenu is acceptable for v1, but architecture should support future migration to a unified Devices/settings surface. Respect endpoint roles and avoid baking in assumptions about per-app microphone routing.
