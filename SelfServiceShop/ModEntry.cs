using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;

namespace SelfServiceShop
{
    // ReSharper disable once UnusedMember.Global
    public class ModEntry : Mod
    {
        private ModConfig _config;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null ||
                !e.IsActionButton && e.Button != SButton.ControllerA)
                return;

            var property = Game1.currentLocation.doesTileHaveProperty((int) e.Cursor.GrabTile.X, (int) e.Cursor.GrabTile.Y, "Action", "Buildings");

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (property)
            {
                case "Buy General":
                    if (_config.Pierre && (_config.ShopsAlwaysOpen || IsNpcInLocation("Pierre")))
                    {
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                        Game1.activeClickableMenu =
                            new ShopMenu(((SeedShop) Game1.currentLocation).shopStock(), 0, "Pierre");
                    }
                    break;
                case "Carpenter":
                    if (_config.Carpenter)
                    {
                        NPC robin;
                        if (_config.ShopsAlwaysOpen)
                            robin = GetNpc("Robin");
                        else if (Game1.currentLocation.characters.Find(c => c.name == "Robin") is NPC npc)
                            robin = npc;
                        else
                            break;
                        var carpenters = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "carpenters");
                        var tileLocation = robin.getTileLocation();
                        carpenters.Invoke(new Location((int) tileLocation.X, (int) tileLocation.Y));
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
                case "AnimalShop":
                    if (_config.Ranch)
                    {
                        NPC marnie;
                        if (_config.ShopsAlwaysOpen)
                            marnie = GetNpc("Marnie");
                        else if (Game1.currentLocation.characters.Find(c => c.name == "Marnie") is NPC npc)
                            marnie = npc;
                        else
                            break;
                        var animalShop = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "animalShop");
                        var tileLocation = marnie.getTileLocation();
                        animalShop.Invoke(new Location((int) tileLocation.X, (int) tileLocation.Y + 1));
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
                case "Buy Fish":
                    if (_config.FishShop &&
                        (_config.ShopsAlwaysOpen || IsNpcInLocation("Willy") || IsNpcInLocation("Willy", "Beach")))
                    {
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                        Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, "Willy");
                    }
                    break;
                case "IceCreamStand":
                    if (_config.IceCreamStand &&
                        (_config.ShopsAlwaysOpen || _config.IceCreamInAllSeasons || SDate.Now().Season == "summer"))
                    {
                        var d = new Dictionary<Item, int[]>
                        {
                            {new Object(233, 1), new[] {250, int.MaxValue}}
                        };
                        Game1.activeClickableMenu = new ShopMenu(d);
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
            }
        }

        private static bool IsNpcInLocation(string name, string locationName = "")
        {
            return (locationName == "" ? Game1.currentLocation : Game1.locations.Find(l => l.name == locationName))
                .characters.Exists(c => c.name == name);
        }

        private static NPC GetNpc(string name)
        {
            var npc = Game1.currentLocation.characters.Find(c => c.name == name);
            if (npc != null)
                return npc;

            foreach (var location in Game1.locations)
            {
                npc = location.characters.Find(c => c.name == name);
                if (npc != null)
                    return npc;
            }
            return null;
        }

        /// <summary>
        ///     EventArgsInput.SuppressButton has a bug that does not suppress mouse buttons as of SMAPI 2.1.
        ///     So this method is a workaround to this bug.
        ///     Issue: https://github.com/Pathoschild/SMAPI/issues/384
        /// </summary>
        private static void SuppressRightMouseButton(string button)
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