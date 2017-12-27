using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private static readonly NPC Ghost = new NPC
        {
            sprite = new AnimatedSprite(new Texture2D(Game1.graphics.GraphicsDevice, 1, 1))
        };

        private static readonly Texture2D PortraitRobin = Game1.content.Load<Texture2D>("Portraits\\Robin");
        private static readonly Texture2D PortraitMarnie = Game1.content.Load<Texture2D>("Portraits\\Marnie");
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

            var property = Game1.currentLocation.doesTileHaveProperty((int) e.Cursor.GrabTile.X,
                (int) e.Cursor.GrabTile.Y, "Action", "Buildings");

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
                        if (_config.ShopsAlwaysOpen)
                        {
                            Carpenters();
                        }
                        else
                        {
                            NPC robin;
                            if (Game1.currentLocation.characters.Find(c => c.name == "Robin") is NPC npc)
                                robin = npc;
                            else
                                break;
                            var carpenters = Helper.Reflection.GetMethod(Game1.currentLocation, "carpenters");
                            var tileLocation = robin.getTileLocation();
                            carpenters.Invoke(new Location((int) tileLocation.X, (int) tileLocation.Y));
                        }
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
                case "AnimalShop":
                    if (_config.Ranch)
                    {
                        if (_config.ShopsAlwaysOpen)
                        {
                            AnimalShop();
                        }
                        else
                        {
                            NPC marnie;
                            if (Game1.currentLocation.characters.Find(c => c.name == "Marnie") is NPC npc)
                                marnie = npc;
                            else
                                break;
                            var animalShop = Helper.Reflection.GetMethod(Game1.currentLocation, "animalShop");
                            var tileLocation = marnie.getTileLocation();
                            animalShop.Invoke(new Location((int) tileLocation.X, (int) tileLocation.Y + 1));
                        }
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
                case "Blacksmith":
                    if (_config.Blacksmith)
                    {
                        if (_config.ShopsAlwaysOpen)
                        {
                            Blacksmith(Game1.getCharacterFromName("Clint"));
                        }
                        else
                        {
                            NPC clint;
                            if (Game1.currentLocation.characters.Find(c => c.name == "Clint") is NPC npc)
                                clint = npc;
                            else
                                break;
                            Blacksmith(clint);
                        }
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
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
                // FarmExpansion mod compatibility (versions >= 3.0 and < 3.1)
                // Not the way I want, but it's the way I found
                case "FECarpenter":
                    if (_config.Carpenter &&
                        (_config.ShopsAlwaysOpen || Game1.currentLocation.characters.Exists(n => n.name == "Robin")))
                    {
                        var robin = Game1.currentLocation.characters.Find(n => n.name == "Robin");
                        if (robin == null || Vector2.Distance(robin.getTileLocation(), e.Cursor.GrabTile) > 3f)
                        {
                            Ghost.name = "Robin";
                            Ghost.setTilePosition((int) e.Cursor.GrabTile.X, (int) e.Cursor.GrabTile.Y - 1);
                            Ghost.Portrait = PortraitRobin;
                            Game1.currentLocation.characters.Insert(0, Ghost);
                            MenuEvents.MenuChanged += MenuEvents_MenuChangedGhost;
                        }

                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
                case "FEAnimalShop":
                    if (_config.Ranch &&
                        (_config.ShopsAlwaysOpen || Game1.currentLocation.characters.Exists(n => n.name == "Marnie")))
                    {
                        var marnie = Game1.currentLocation.characters.Find(n => n.name == "Marnie");
                        if (marnie == null ||
                            !marnie.getTileLocation().Equals(new Vector2(e.Cursor.GrabTile.X, e.Cursor.GrabTile.Y - 1)))
                        {
                            Ghost.name = "Marnie";
                            Ghost.setTilePosition((int) e.Cursor.GrabTile.X, (int) e.Cursor.GrabTile.Y - 1);
                            Ghost.Portrait = PortraitMarnie;
                            Game1.currentLocation.characters.Insert(0, Ghost);
                            MenuEvents.MenuChanged += MenuEvents_MenuChangedGhost;
                        }
                        e.SuppressButton();
                        SuppressRightMouseButton(e.Button.ToString());
                    }
                    break;
            }
        }

        private static void MenuEvents_MenuChangedGhost(object sender, EventArgsClickableMenuChanged e)
        {
            Game1.currentLocation.characters.Remove(Ghost);
            MenuEvents.MenuChanged -= MenuEvents_MenuChangedGhost;
        }

        private static bool IsNpcInLocation(string name, string locationName = "")
        {
            return (locationName == "" ? Game1.currentLocation : Game1.locations.Find(l => l.name == locationName))
                .characters.Exists(c => c.name == name);
        }

        private static void Carpenters()
        {
            if (Game1.player.daysUntilHouseUpgrade < 0 && !Game1.getFarm().isThereABuildingUnderConstruction() &&
                Game1.player.currentUpgrade == null)
            {
                Response[] answerChoices;
                if (Game1.player.houseUpgradeLevel < 3)
                    answerChoices = new[]
                    {
                        new Response("Shop",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
                        new Response("Upgrade",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")),
                        new Response("Construct",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")),
                        new Response("Leave",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
                    };
                else
                    answerChoices = new[]
                    {
                        new Response("Shop",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
                        new Response("Construct",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")),
                        new Response("Leave",
                            Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
                    };
                Game1.currentLocation.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), answerChoices,
                    "carpenter");
                return;
            }
            Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
        }

        private static void AnimalShop()
        {
            Game1.currentLocation.createQuestionDialogue("", new[]
            {
                new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
                new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
                new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
            }, "Marnie");
        }

        private static void Blacksmith(NPC clint)
        {
            if (Game1.player.toolBeingUpgraded == null)
            {
                Response[] answerChoices;
                if (Game1.player.hasItemInInventory(535, 1) || Game1.player.hasItemInInventory(536, 1) ||
                    Game1.player.hasItemInInventory(537, 1) || Game1.player.hasItemInInventory(749, 1))
                    answerChoices = new[]
                    {
                        new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
                        new Response("Upgrade",
                            Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                        new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
                        new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                    };
                else
                    answerChoices = new[]
                    {
                        new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
                        new Response("Upgrade",
                            Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                        new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                    };
                Game1.currentLocation.createQuestionDialogue("", answerChoices, "Blacksmith");
                return;
            }

            if (Game1.player.daysLeftForToolUpgrade <= 0)
            {
                if (Game1.player.freeSpotsInInventory() > 0)
                {
                    Game1.player.holdUpItemThenMessage(Game1.player.toolBeingUpgraded);
                    Game1.player.addItemToInventoryBool(Game1.player.toolBeingUpgraded);
                    Game1.player.toolBeingUpgraded = null;
                    return;
                }
                Game1.drawDialogue(clint, Game1.content.LoadString("Data\\ExtraDialogue:Clint_NoInventorySpace"));
                return;
            }
            Game1.drawDialogue(clint,
                Game1.content.LoadString("Data\\ExtraDialogue:Clint_StillWorking",
                    (object) Game1.player.toolBeingUpgraded.DisplayName));
            MenuEvents.MenuClosed += MenuEvents_MenuClosedBlacksmith;
        }

        private static void MenuEvents_MenuClosedBlacksmith(object sender, EventArgsClickableMenuClosed e)
        {
            if (Game1.player.hasItemInInventory(535, 1) || Game1.player.hasItemInInventory(536, 1) ||
                Game1.player.hasItemInInventory(537, 1) || Game1.player.hasItemInInventory(749, 1))
            {
                var answerChoices = new[]
                {
                    new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
                    new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                };
                Game1.currentLocation.createQuestionDialogue("", answerChoices, "Blacksmith");
            }
            else
            {
                Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0, "Clint");
            }
            MenuEvents.MenuClosed -= MenuEvents_MenuClosedBlacksmith;
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