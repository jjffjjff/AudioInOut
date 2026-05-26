using System.Windows.Input;

namespace AudioInOut.UI.Helpers
{
    public class SystemSoundsHelper
    {
        public static ICommand PlayBeepSound { get; set; }

        static SystemSoundsHelper()
        {
            PlayBeepSound = new RelayCommand(() => System.Media.SystemSounds.Beep.Play());
        }
    }
}
