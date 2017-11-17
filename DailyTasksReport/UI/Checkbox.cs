using StardewValley.Menus;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DailyTasksReport
{
    internal class Checkbox : OptionsElement
    {
        private ModConfig config;

        public OptionsEnum option;
        public readonly int itemLevel;
        public bool isChecked;

        public Checkbox(string label, OptionsEnum whichOption, ModConfig config, int itemLevel = 0) : base(label, -1, -1, Game1.pixelZoom * 9, Game1.pixelZoom * 9, -1)
        {
            this.option = whichOption;
            this.config = config;
            this.itemLevel = itemLevel;
            this.bounds.X += itemLevel * Game1.pixelZoom * 7;

            if (whichOption == OptionsEnum.AllAnimalProducts || whichOption == OptionsEnum.AllMachines)
                this.whichOption = -1;
            else
                this.whichOption = (int)whichOption;

            // Load options
            switch (option)
            {
                case OptionsEnum.ShowDetailedInfo:
                    isChecked = config.ShowDetailedInfo;
                    break;

                case OptionsEnum.UnwateredCrops:
                    isChecked = config.UnwateredCrops;
                    break;
                case OptionsEnum.unharvestedCrops:
                    isChecked = config.UnharvestedCrops;
                    break;
                case OptionsEnum.DeadCrops:
                    isChecked = config.DeadCrops;
                    break;
                case OptionsEnum.unpettedPet:
                    isChecked = config.UnpettedPet;
                    break;
                case OptionsEnum.UnfilledPetBowl:
                    isChecked = config.UnfilledPetBowl;
                    break;
                case OptionsEnum.UnpettedAnimals:
                    isChecked = config.UnpettedAnimals;
                    break;
                case OptionsEnum.MissingHay:
                    isChecked = config.MissingHay;
                    break;
                case OptionsEnum.FarmCave:
                    isChecked = config.FarmCave;
                    break;
                case OptionsEnum.UncollectedCrabpots:
                    isChecked = config.UncollectedCrabpots;
                    break;
                case OptionsEnum.NotBaitedCrabpots:
                    isChecked = config.NotBaitedCrabpots;
                    break;

                // Animal products
                case OptionsEnum.AllAnimalProducts:
                    isChecked = config.AnimalProducts["Cow milk"] && config.AnimalProducts["Goat milk"] && config.AnimalProducts["Sheep wool"];
                    break;
                case OptionsEnum.CowMilk:
                    isChecked = config.AnimalProducts["Cow milk"];
                    break;
                case OptionsEnum.GoatMilk:
                    isChecked = config.AnimalProducts["Goat milk"];
                    break;
                case OptionsEnum.SheepWool:
                    isChecked = config.AnimalProducts["Sheep wool"];
                    break;

                // Machines
                case OptionsEnum.AllMachines:
                    isChecked = !config.Machines.ContainsValue(false);
                    break;
                case OptionsEnum.BeeHouse:
                    isChecked = config.Machines["Bee House"];
                    break;
                case OptionsEnum.CharcoalKiln:
                    isChecked = config.Machines["Charcoal Kiln"];
                    break;
                case OptionsEnum.CheesePress:
                    isChecked = config.Machines["Cheese Press"];
                    break;
                case OptionsEnum.Crystalarium:
                    isChecked = config.Machines["Crystalarium"];
                    break;
                case OptionsEnum.Furnace:
                    isChecked = config.Machines["Furnace"];
                    break;
                case OptionsEnum.Keg:
                    isChecked = config.Machines["Keg"];
                    break;
                case OptionsEnum.LightningRod:
                    isChecked = config.Machines["Lightning Rod"];
                    break;
                case OptionsEnum.Loom:
                    isChecked = config.Machines["Loom"];
                    break;
                case OptionsEnum.MayonnaiseMachine:
                    isChecked = config.Machines["Mayonnaise Machine"];
                    break;
                case OptionsEnum.OilMaker:
                    isChecked = config.Machines["Oil Maker"];
                    break;
                case OptionsEnum.PreservesJar:
                    isChecked = config.Machines["Preserves Jar"];
                    break;
                case OptionsEnum.RecyclingMachine:
                    isChecked = config.Machines["Recycling Machine"];
                    break;
                case OptionsEnum.SeedMaker:
                    isChecked = config.Machines["Seed Maker"];
                    break;
                case OptionsEnum.SlimeEggPress:
                    isChecked = config.Machines["Slime Egg-Press"];
                    break;
                case OptionsEnum.SodaMachine:
                    isChecked = config.Machines["Soda Machine"];
                    break;
                case OptionsEnum.StatueOfEndlessFortune:
                    isChecked = config.Machines["Statue Of Endless Fortune"];
                    break;
                case OptionsEnum.StatueOfPerfection:
                    isChecked = config.Machines["Statue Of Perfection"];
                    break;
                case OptionsEnum.Tapper:
                    isChecked = config.Machines["Tapper"];
                    break;
                case OptionsEnum.WormBin:
                    isChecked = config.Machines["Worm Bin"];
                    break;
            }
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (whichOption == -1)
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y + Game1.pixelZoom), isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
                base.draw(b, slotX + bounds.X + Game1.pixelZoom * 4, slotY - Game1.pixelZoom * 3);
            }
            else
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y), isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
                base.draw(b, slotX, slotY);
            }
        }
        

        public override void receiveLeftClick(int x, int y)
        {
            if (greyedOut)
                return;

            Game1.playSound("drumkit6");
            isChecked = !isChecked;

            // Change options
            switch (option)
            {
                case OptionsEnum.ShowDetailedInfo:
                    config.ShowDetailedInfo = isChecked;
                    break;

                case OptionsEnum.UnwateredCrops:
                    config.UnwateredCrops = isChecked;
                    break;
                case OptionsEnum.unharvestedCrops:
                    config.UnharvestedCrops = isChecked;
                    break;
                case OptionsEnum.DeadCrops:
                    config.DeadCrops = isChecked;
                    break;
                case OptionsEnum.unpettedPet:
                    config.UnpettedPet = isChecked;
                    break;
                case OptionsEnum.UnfilledPetBowl:
                    config.UnfilledPetBowl = isChecked;
                    break;
                case OptionsEnum.UnpettedAnimals:
                    config.UnpettedAnimals = isChecked;
                    break;
                case OptionsEnum.MissingHay:
                    config.MissingHay = isChecked;
                    break;
                case OptionsEnum.FarmCave:
                    config.FarmCave = isChecked;
                    break;
                case OptionsEnum.UncollectedCrabpots:
                    config.UncollectedCrabpots = isChecked;
                    break;
                case OptionsEnum.NotBaitedCrabpots:
                    config.NotBaitedCrabpots = isChecked;
                    break;

                // Animal products
                case OptionsEnum.AllAnimalProducts:
                    config.AnimalProducts["Cow milk"] = isChecked;
                    config.AnimalProducts["Goat milk"] = isChecked;
                    config.AnimalProducts["Sheep wool"] = isChecked;
                    SettingsMenu.groupClicked = OptionsEnum.AllAnimalProducts;
                    break;
                case OptionsEnum.CowMilk:
                    config.AnimalProducts["Cow milk"] = isChecked;
                    break;
                case OptionsEnum.GoatMilk:
                    config.AnimalProducts["Goat milk"] = isChecked;
                    break;
                case OptionsEnum.SheepWool:
                    config.AnimalProducts["Sheep wool"] = isChecked;
                    break;

                // Machines
                case OptionsEnum.AllMachines:
                    foreach (var key in config.Machines.Keys.ToList())
                        config.Machines[key] = isChecked;
                    SettingsMenu.groupClicked = OptionsEnum.AllMachines;
                    break;
                case OptionsEnum.BeeHouse:
                    config.Machines["Bee House"] = isChecked;
                    break;
                case OptionsEnum.CharcoalKiln:
                    config.Machines["Charcoal Kiln"] = isChecked;
                    break;
                case OptionsEnum.CheesePress:
                    config.Machines["Cheese Press"] = isChecked;
                    break;
                case OptionsEnum.Crystalarium:
                    config.Machines["Crystalarium"] = isChecked;
                    break;
                case OptionsEnum.Furnace:
                    config.Machines["Furnace"] = isChecked;
                    break;
                case OptionsEnum.Keg:
                    config.Machines["Keg"] = isChecked;
                    break;
                case OptionsEnum.LightningRod:
                    config.Machines["Lightning Rod"] = isChecked;
                    break;
                case OptionsEnum.Loom:
                    config.Machines["Loom"] = isChecked;
                    break;
                case OptionsEnum.MayonnaiseMachine:
                    config.Machines["Mayonnaise Machine"] = isChecked;
                    break;
                case OptionsEnum.OilMaker:
                    config.Machines["Oil Maker"] = isChecked;
                    break;
                case OptionsEnum.PreservesJar:
                    config.Machines["Preserves Jar"] = isChecked;
                    break;
                case OptionsEnum.RecyclingMachine:
                    config.Machines["Recycling Machine"] = isChecked;
                    break;
                case OptionsEnum.SeedMaker:
                    config.Machines["Seed Maker"] = isChecked;
                    break;
                case OptionsEnum.SlimeEggPress:
                    config.Machines["Slime Egg-Press"] = isChecked;
                    break;
                case OptionsEnum.SodaMachine:
                    config.Machines["Soda Machine"] = isChecked;
                    break;
                case OptionsEnum.StatueOfEndlessFortune:
                    config.Machines["Statue Of Endless Fortune"] = isChecked;
                    break;
                case OptionsEnum.StatueOfPerfection:
                    config.Machines["Statue Of Perfection"] = isChecked;
                    break;
                case OptionsEnum.Tapper:
                    config.Machines["Tapper"] = isChecked;
                    break;
                case OptionsEnum.WormBin:
                    config.Machines["Worm Bin"] = isChecked;
                    break;
            }

            SettingsMenu.configChanged = true;
        }
    }
}
