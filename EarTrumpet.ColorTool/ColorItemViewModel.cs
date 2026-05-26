using System.Windows.Media;

namespace AudioInOut.ColorTool
{
    public class ColorItemViewModel
    {
        public ColorItemViewModel() { }

        public string Name { get; set; }
        public string Opacity { get; set; }
        public SolidColorBrush Color { get; set; }
    }
}
