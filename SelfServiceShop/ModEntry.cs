using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using xTile.Dimensions;

namespace SelfServiceShop
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
            if (Context.IsWorldReady && Context.IsPlayerFree && Game1.activeClickableMenu == null && (e.IsActionButton || e.Button == SButton.ControllerA))
            {
                string property = Game1.currentLocation.doesTileHaveProperty((int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y, "Action", "Buildings");
                
                switch (property)
                {
                    case "Buy General":
                        if (config.Pierre && IsNpcInLocation("Pierre"))
                        {
                            e.SuppressButton();
                            SuppressRightMouseButton(e.Button.ToString());
                            Game1.activeClickableMenu = new ShopMenu((Game1.currentLocation as SeedShop).shopStock(), 0, "Pierre");
                        }
                        break;
                    case "Carpenter":
                        if (config.Carpenter && Game1.currentLocation.characters.Find(c => c.name == "Robin") is NPC robin && robin != null)
                        {
                            IPrivateMethod carpenters = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "carpenters");
                            Vector2 tileLocation = robin.getTileLocation();
                            carpenters.Invoke(new Location((int)tileLocation.X, (int)tileLocation.Y));
                            e.SuppressButton();
                            SuppressRightMouseButton(e.Button.ToString());
                        }
                        break;
                    case "AnimalShop":
                        if (config.Ranch && Game1.currentLocation.characters.Find(c => c.name == "Marnie") is NPC marnie && marnie != null)
                        {
                            IPrivateMethod animalShop = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "animalShop");
                            Vector2 tileLocation = marnie.getTileLocation();
                            animalShop.Invoke(new Location((int)tileLocation.X, (int)tileLocation.Y + 1));
                            e.SuppressButton();
                            SuppressRightMouseButton(e.Button.ToString());
                        }
                        break;
                    case "Buy Fish":
                        if (config.FishShop && (IsNpcInLocation("Willy") || IsNpcInLocation("Willy", "Beach")))
                        {
                            e.SuppressButton();
                            SuppressRightMouseButton(e.Button.ToString());
                            Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, "Willy");
                        }
                        break;
                    case "IceCreamStand":
                        if (!config.IceCreamStand || (!config.IceCreamInAllSeasons && SDate.Now().Season != "summer"))
                            break;
                        Dictionary<Item, int[]> d = new Dictionary<Item, int[]>
                        {
                            { new Object(233, 1), new int[2] { 250, int.MaxValue } }
                        };
                        Game1.activeClickableMenu = new ShopMenu(d);
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                        break;
                }
            }
        }
        
        private bool IsNpcInLocation(string name, string locationName = "")
        {
            return (locationName == "" ? Game1.currentLocation : Game1.locations.Find(l => l.name == locationName)).characters.Exists(c => c.name == name);
        }

        /// <summary>
        /// EventArgsInput.SuppressButton has a bug that does not suppress mouse buttons as of SMAPI 2.1.
        /// So this method is a workaround to this bug.
        /// Issue: https://github.com/Pathoschild/SMAPI/issues/384
        /// </summary>
        private void SuppressRightMouseButton(string button)
        {
            if (button != "MouseRight")
                return;
            Game1.oldMouseState = new MouseState(
                Game1.oldMouseState.X,
                Game1.oldMouseState.Y,
                Game1.oldMouseState.ScrollWheelValue,
                Game1.oldMouseState.LeftButton,
                Game1.oldMouseState.MiddleButton,
                ButtonState.Pressed,
                Game1.oldMouseState.XButton1,
                Game1.oldMouseState.XButton2
            );
        }
    }
}
