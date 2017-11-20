using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace DailyTasksReport.UI
{
    internal class Checkbox : OptionsElement
    {
        private readonly ModConfig _config;
        private readonly OptionsEnum _option;
        private bool _isChecked;

        public Checkbox(string label, OptionsEnum whichOption, ModConfig config, int itemLevel = 0) :
            base(label, -1, -1, Game1.pixelZoom * 9, Game1.pixelZoom * 9)
        {
            _option = whichOption;
            _config = config;
            bounds.X += itemLevel * Game1.pixelZoom * 7;

            if (whichOption == OptionsEnum.AllAnimalProducts || whichOption == OptionsEnum.AllMachines)
                this.whichOption = -1;
            else
                this.whichOption = (int) whichOption;

            // Load options
            RefreshStatus();
        }

        internal void RefreshStatus()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_option)
            {
                case OptionsEnum.ShowDetailedInfo:
                    _isChecked = _config.ShowDetailedInfo;
                    break;

                case OptionsEnum.UnwateredCrops:
                    _isChecked = _config.UnwateredCrops;
                    break;
                case OptionsEnum.UnharvestedCrops:
                    _isChecked = _config.UnharvestedCrops;
                    break;
                case OptionsEnum.DeadCrops:
                    _isChecked = _config.DeadCrops;
                    break;
                case OptionsEnum.UnpettedPet:
                    _isChecked = _config.UnpettedPet;
                    break;
                case OptionsEnum.UnfilledPetBowl:
                    _isChecked = _config.UnfilledPetBowl;
                    break;
                case OptionsEnum.UnpettedAnimals:
                    _isChecked = _config.UnpettedAnimals;
                    break;
                case OptionsEnum.MissingHay:
                    _isChecked = _config.MissingHay;
                    break;
                case OptionsEnum.FarmCave:
                    _isChecked = _config.FarmCave;
                    break;
                case OptionsEnum.UncollectedCrabpots:
                    _isChecked = _config.UncollectedCrabpots;
                    break;
                case OptionsEnum.NotBaitedCrabpots:
                    _isChecked = _config.NotBaitedCrabpots;
                    break;

                // Animal products
                case OptionsEnum.AllAnimalProducts:
                    _isChecked = _config.AnimalProducts["Cow milk"] &&
                                 _config.AnimalProducts["Goat milk"] &&
                                 _config.AnimalProducts["Sheep wool"];
                    break;
                case OptionsEnum.CowMilk:
                    _isChecked = _config.AnimalProducts["Cow milk"];
                    break;
                case OptionsEnum.GoatMilk:
                    _isChecked = _config.AnimalProducts["Goat milk"];
                    break;
                case OptionsEnum.SheepWool:
                    _isChecked = _config.AnimalProducts["Sheep wool"];
                    break;

                // Machines
                case OptionsEnum.AllMachines:
                    _isChecked = !_config.Machines.ContainsValue(false);
                    break;
                case OptionsEnum.BeeHouse:
                    _isChecked = _config.Machines["Bee House"];
                    break;
                case OptionsEnum.CharcoalKiln:
                    _isChecked = _config.Machines["Charcoal Kiln"];
                    break;
                case OptionsEnum.CheesePress:
                    _isChecked = _config.Machines["Cheese Press"];
                    break;
                case OptionsEnum.Crystalarium:
                    _isChecked = _config.Machines["Crystalarium"];
                    break;
                case OptionsEnum.Furnace:
                    _isChecked = _config.Machines["Furnace"];
                    break;
                case OptionsEnum.Keg:
                    _isChecked = _config.Machines["Keg"];
                    break;
                case OptionsEnum.LightningRod:
                    _isChecked = _config.Machines["Lightning Rod"];
                    break;
                case OptionsEnum.Loom:
                    _isChecked = _config.Machines["Loom"];
                    break;
                case OptionsEnum.MayonnaiseMachine:
                    _isChecked = _config.Machines["Mayonnaise Machine"];
                    break;
                case OptionsEnum.OilMaker:
                    _isChecked = _config.Machines["Oil Maker"];
                    break;
                case OptionsEnum.PreservesJar:
                    _isChecked = _config.Machines["Preserves Jar"];
                    break;
                case OptionsEnum.RecyclingMachine:
                    _isChecked = _config.Machines["Recycling Machine"];
                    break;
                case OptionsEnum.SeedMaker:
                    _isChecked = _config.Machines["Seed Maker"];
                    break;
                case OptionsEnum.SlimeEggPress:
                    _isChecked = _config.Machines["Slime Egg-Press"];
                    break;
                case OptionsEnum.SodaMachine:
                    _isChecked = _config.Machines["Soda Machine"];
                    break;
                case OptionsEnum.StatueOfEndlessFortune:
                    _isChecked = _config.Machines["Statue Of Endless Fortune"];
                    break;
                case OptionsEnum.StatueOfPerfection:
                    _isChecked = _config.Machines["Statue Of Perfection"];
                    break;
                case OptionsEnum.Tapper:
                    _isChecked = _config.Machines["Tapper"];
                    break;
                case OptionsEnum.WormBin:
                    _isChecked = _config.Machines["Worm Bin"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Option {_option} is not possible on a checkbox.");
            }
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (whichOption == -1)
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y + Game1.pixelZoom),
                    _isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                    Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None,
                    0.4f);
                base.draw(b, slotX + bounds.X + Game1.pixelZoom * 4, slotY - Game1.pixelZoom * 3);
            }
            else
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y),
                    _isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                    Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None,
                    0.4f);
                base.draw(b, slotX, slotY);
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (greyedOut)
                return;

            Game1.playSound("drumkit6");
            _isChecked = !_isChecked;

            // Change options
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_option)
            {
                case OptionsEnum.ShowDetailedInfo:
                    _config.ShowDetailedInfo = _isChecked;
                    break;

                case OptionsEnum.UnwateredCrops:
                    _config.UnwateredCrops = _isChecked;
                    break;
                case OptionsEnum.UnharvestedCrops:
                    _config.UnharvestedCrops = _isChecked;
                    break;
                case OptionsEnum.DeadCrops:
                    _config.DeadCrops = _isChecked;
                    break;
                case OptionsEnum.UnpettedPet:
                    _config.UnpettedPet = _isChecked;
                    break;
                case OptionsEnum.UnfilledPetBowl:
                    _config.UnfilledPetBowl = _isChecked;
                    break;
                case OptionsEnum.UnpettedAnimals:
                    _config.UnpettedAnimals = _isChecked;
                    break;
                case OptionsEnum.MissingHay:
                    _config.MissingHay = _isChecked;
                    break;
                case OptionsEnum.FarmCave:
                    _config.FarmCave = _isChecked;
                    break;
                case OptionsEnum.UncollectedCrabpots:
                    _config.UncollectedCrabpots = _isChecked;
                    break;
                case OptionsEnum.NotBaitedCrabpots:
                    _config.NotBaitedCrabpots = _isChecked;
                    break;

                // Animal products
                case OptionsEnum.AllAnimalProducts:
                    _config.AnimalProducts["Cow milk"] = _isChecked;
                    _config.AnimalProducts["Goat milk"] = _isChecked;
                    _config.AnimalProducts["Sheep wool"] = _isChecked;
                    break;
                case OptionsEnum.CowMilk:
                    _config.AnimalProducts["Cow milk"] = _isChecked;
                    break;
                case OptionsEnum.GoatMilk:
                    _config.AnimalProducts["Goat milk"] = _isChecked;
                    break;
                case OptionsEnum.SheepWool:
                    _config.AnimalProducts["Sheep wool"] = _isChecked;
                    break;

                // Machines
                case OptionsEnum.AllMachines:
                    foreach (var key in _config.Machines.Keys.ToList())
                        _config.Machines[key] = _isChecked;
                    break;
                case OptionsEnum.BeeHouse:
                    _config.Machines["Bee House"] = _isChecked;
                    break;
                case OptionsEnum.CharcoalKiln:
                    _config.Machines["Charcoal Kiln"] = _isChecked;
                    break;
                case OptionsEnum.CheesePress:
                    _config.Machines["Cheese Press"] = _isChecked;
                    break;
                case OptionsEnum.Crystalarium:
                    _config.Machines["Crystalarium"] = _isChecked;
                    break;
                case OptionsEnum.Furnace:
                    _config.Machines["Furnace"] = _isChecked;
                    break;
                case OptionsEnum.Keg:
                    _config.Machines["Keg"] = _isChecked;
                    break;
                case OptionsEnum.LightningRod:
                    _config.Machines["Lightning Rod"] = _isChecked;
                    break;
                case OptionsEnum.Loom:
                    _config.Machines["Loom"] = _isChecked;
                    break;
                case OptionsEnum.MayonnaiseMachine:
                    _config.Machines["Mayonnaise Machine"] = _isChecked;
                    break;
                case OptionsEnum.OilMaker:
                    _config.Machines["Oil Maker"] = _isChecked;
                    break;
                case OptionsEnum.PreservesJar:
                    _config.Machines["Preserves Jar"] = _isChecked;
                    break;
                case OptionsEnum.RecyclingMachine:
                    _config.Machines["Recycling Machine"] = _isChecked;
                    break;
                case OptionsEnum.SeedMaker:
                    _config.Machines["Seed Maker"] = _isChecked;
                    break;
                case OptionsEnum.SlimeEggPress:
                    _config.Machines["Slime Egg-Press"] = _isChecked;
                    break;
                case OptionsEnum.SodaMachine:
                    _config.Machines["Soda Machine"] = _isChecked;
                    break;
                case OptionsEnum.StatueOfEndlessFortune:
                    _config.Machines["Statue Of Endless Fortune"] = _isChecked;
                    break;
                case OptionsEnum.StatueOfPerfection:
                    _config.Machines["Statue Of Perfection"] = _isChecked;
                    break;
                case OptionsEnum.Tapper:
                    _config.Machines["Tapper"] = _isChecked;
                    break;
                case OptionsEnum.WormBin:
                    _config.Machines["Worm Bin"] = _isChecked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Option {_option} is not possible on a checkbox.");
            }

            SettingsMenu.ConfigChanged = true;
        }
    }
}