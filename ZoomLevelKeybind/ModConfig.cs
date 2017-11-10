using Microsoft.Xna.Framework.Input;

namespace ZoomLevelKeybind
{
    internal class ModConfig
    {
        public string IncreaseZoom { get; set; } = Keys.OemPeriod.ToString();
        public string DecreaseZoon { get; set; } = Keys.OemComma.ToString();
        public bool UnlimitedZoom { get; set; } = false;
    }
}
