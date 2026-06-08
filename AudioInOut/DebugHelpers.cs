using AudioInOut.DataModel.AppInformation;
using AudioInOut.DataModel.Audio;
using AudioInOut.DataModel.WindowsAudio;
using AudioInOut.Interop.Helpers;
using AudioInOut.UI.ViewModels;
using System;
using System.Linq;

#if DEBUG
namespace AudioInOut
{
    class DebugHelpers
    {
        private static void DebugRemoveAllDevices()
        {
            var devManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            var devManagerNotify = (Interop.MMDeviceAPI.IMMNotificationClient)devManager;
            foreach (var dev in devManager.Devices.ToArray())
            {
                devManagerNotify.OnDeviceRemoved(dev.Id);
            }
            devManagerNotify.OnDefaultDeviceChanged(Interop.MMDeviceAPI.EDataFlow.eRender, Interop.MMDeviceAPI.ERole.eMultimedia, null);
            devManagerNotify.OnDefaultDeviceChanged(Interop.MMDeviceAPI.EDataFlow.eRender, Interop.MMDeviceAPI.ERole.eConsole, null);
        }

        private static void AddMockApp(IAudioDevice mockDevice, string displayName, string appId, string iconPath)
        {
            var mockApp = MakeMockApp(mockDevice, displayName, appId, iconPath);
            var mockApp2 = MakeMockApp(mockDevice, displayName, appId, iconPath);
            var mockApp3 = MakeMockApp(mockDevice, displayName, appId, iconPath);

            var group = new DataModel.WindowsAudio.Internal.AudioDeviceSessionGroup(mockDevice, mockApp);
            group.AddSession(mockApp2);
            group.AddSession(mockApp3);
            mockDevice.Groups.Add(group);
        }

        private static DataModel.Audio.Mocks.AudioDeviceSession MakeMockApp(IAudioDevice mockDevice, string displayName, string appId, string iconPath)
        {
            return new DataModel.Audio.Mocks.AudioDeviceSession(
                mockDevice,
                Guid.NewGuid().ToString(),
                displayName,
                appId,
                Environment.ExpandEnvironmentVariables(iconPath));
        }

        private static void DebugAddMockDevice()
        {
            var id = Guid.NewGuid().ToString();
            var devManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            var devManagerNotify = (Interop.MMDeviceAPI.IMMNotificationClient)devManager;

            var mockDevice = new DataModel.Audio.Mocks.AudioDevice(id, devManager);

            AddMockApp(mockDevice,
                "System Sounds",
                "*SystemSounds",
                AppInformationFactory.CreateForProcess(0).SmallLogoPath);
            AddMockApp(mockDevice,
                "Firefaux",
                "Firefaux",
                @"%ProgramFiles%\Mozilla Firefox\firefox.exe");
            AddMockApp(mockDevice,
                "Chr0me",
                "Chr0me",
                @"%ProgramFilesx86%\Google\Chrome\Application\chrome.exe");

            var addInfo = devManager.GetType().GetMethod("Add", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            addInfo.Invoke(devManager, new object[] { mockDevice });
        }
    }
}
#endif