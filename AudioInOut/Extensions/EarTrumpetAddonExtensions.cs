using AudioInOut.Actions;
using AudioInOut.Extensibility;

namespace AudioInOut.Extensions
{
    public static class EarTrumpetAddonExtensions
    {
        public static bool IsInternal(this EarTrumpetAddon addon)
        {
            return addon is EarTrumpetActionsAddon;
        }
    }
}
