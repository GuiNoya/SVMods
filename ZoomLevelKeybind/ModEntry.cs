using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework.Input;

namespace ZoomLevelKeybind
{
    public class ModEntry : Mod
    {
        private ModConfig config;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            
            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            //if (!Context.IsWorldReady)
            //    return;

            if (e.Button.TryGetKeyboard(out Keys key))
            {
                if (e.Button == config.IncreaseZoomKey)
                    IncreaseZoom();
                else if (e.Button == config.DecreaseZoomKey)
                    DecreaseZoom();
                return;
            }

            if (e.Button.TryGetController(out Buttons button))
            {
                bool wasZoom = false;

                if (e.Button == config.IncreaseZoomButton)
                {
                    IncreaseZoom();
                    wasZoom = true;
                }
                else if (e.Button == config.DecreaseZoomButton)
                {
                    DecreaseZoom();
                    wasZoom = true;
                }

                if (config.SuppressControllerButton && wasZoom)
                    e.SuppressButton();
            }
        }

        private void IncreaseZoom()
        {
            if (config.UnlimitedZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel >= 0.05f ? 0.05f : (float)Math.Round((double)Game1.options.zoomLevel + 0.05, 2);
            else
                Game1.options.zoomLevel = Game1.options.zoomLevel >= 1.25f ? 1.25f : (float)Math.Round((double)Game1.options.zoomLevel + 0.05, 2);

            Program.gamePtr.refreshWindowSettings();
        }

        private void DecreaseZoom()
        {
            if (config.UnlimitedZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.05f ? 0.05f : (float)Math.Round((double)Game1.options.zoomLevel - 0.05, 2);
            else if (config.MoreZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.35f ? 0.35f : (float)Math.Round((double)Game1.options.zoomLevel - 0.05, 2);
            else
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.75f ? 0.75f : (float)Math.Round((double)Game1.options.zoomLevel - 0.05, 2);

            Program.gamePtr.refreshWindowSettings();
        }
    }
}
