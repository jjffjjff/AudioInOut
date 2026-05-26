using System.Runtime.InteropServices;

namespace AudioInOut.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}
