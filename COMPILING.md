# Compiling AudioInOut

## Requirements
* [Visual Studio 2017](https://visualstudio.microsoft.com/vs/community/) (or newer)
* [Git for Windows](https://git-scm.com/download/win)
* [Windows 10 Anniversary Update](https://blogs.windows.com/windowsexperience/2016/08/02/how-to-get-the-windows-10-anniversary-update/) (or newer)
* [.NET Framework 4.6.2 Developer Pack](https://www.microsoft.com/net/download/thank-you/net462-developer-pack)
* [Windows 10 SDK (10.0.14393.0)](https://developer.microsoft.com/en-us/windows/downloads/sdk-archive)

## Step-by-step
1. Install Visual Studio 2017 with the `.NET desktop development` and `Universal Windows Platform development` workloads.
2. Install the `Windows 10 SDK (10.0.14393.0)` SDK.
3. Install the .NET Framework 4.6.2 Developer Pack.
4. Install Git for Windows.
5. Clone this repository.
6. Open `AudioInOut.vs15.sln` in Visual Studio.
7. Change the target platform to `x86` and build the `EarTrumpet.Package` project.
8. Output is at `Build\Debug\AudioInOut.exe`.
