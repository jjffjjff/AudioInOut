using System.Runtime.InteropServices;

namespace AudioInOut.Interop
{
    static class Ole32
    {
        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern void PropVariantClear(ref PropVariant pvar);
    }
}
