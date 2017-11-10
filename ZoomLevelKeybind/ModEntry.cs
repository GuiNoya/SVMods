using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ZoomLevelKeybind
{
    public class ModEntry : Mod
    {
        private ModConfig config;
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            //if (!Context.IsWorldReady)
            //    return;

            if (e.KeyPressed.ToString() == config.IncreaseZoom)
            {
                if (!config.UnlimitedZoom && Game1.options.zoomLevel >= 1.25f)
                {
                    Game1.options.zoomLevel = 1.25f;
                    return;
                }
                Game1.options.zoomLevel += 0.05f;
            }
            else if (e.KeyPressed.ToString() == config.DecreaseZoon)
            {
                if (!config.UnlimitedZoom && Game1.options.zoomLevel <= 0.75f)
                {
                    Game1.options.zoomLevel = 0.75f;
                    return;
                }
                Game1.options.zoomLevel -= 0.05f;
            }
        }
    }
}
