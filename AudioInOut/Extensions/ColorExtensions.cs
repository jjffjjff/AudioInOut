using System.Windows.Media;

namespace AudioInOut.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToABGRColor(this uint abgrValue)
            => Color.FromArgb((byte)(abgrValue >> 24), (byte)abgrValue, (byte)(abgrValue >> 8), (byte)(abgrValue >> 16));

        public static uint ToABGR(this Color abgrValue)
        {
            return (uint)(
                abgrValue.A << 24 |
                abgrValue.B << 16 |
                abgrValue.G << 8 |
                abgrValue.R);
        }
    }
}
