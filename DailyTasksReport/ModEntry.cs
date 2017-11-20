using System;
using DailyTasksReport.UI;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace DailyTasksReport
{
    /// <summary>The mod entry point.</summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModEntry : Mod
    {
        private static readonly int[] Fruits = {296, 396, 406, 410, 613, 634, 635, 636, 637, 638};

        private bool _checkAnimalProducts;
        private bool _checkMachines;
        private ReportBuilder _report;

        internal ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            _checkAnimalProducts = Config.AnimalProducts.ContainsValue(true);
            _checkMachines = Config.Machines.ContainsValue(true);

            _report = new ReportBuilder(this);

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || e.KeyPressed == Keys.None)
                return;

            if (e.KeyPressed.ToString() == Config.OpenReportKey)
            {
                _report.Clear();

                foreach (var location in Game1.locations)
                {
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (location is Farm || location.name == "Greenhouse")
                        if (Config.UnwateredCrops || Config.UnharvestedCrops || Config.DeadCrops)
                            CheckForCrops(location);
                    if (location is Farm || location is FarmHouse)
                        if (Config.UnpettedPet)
                            CheckForUnpettedPet(location);
                    if (location is BuildableGameLocation buildableLocation)
                        if (Config.MissingHay)
                            CheckForMissingHay(buildableLocation);
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (location is Farm farm)
                    {
                        if (Config.UnpettedAnimals || _checkAnimalProducts)
                            CheckAnimals(farm);
                        if (Config.UnfilledPetBowl)
                            CheckForUnfilledPetBowl(farm);
                    }
                    if (location is FarmCave cave)
                        if (Config.FarmCave)
                            CheckFarmCave(cave);
                    if (location.waterTiles != null)
                        if (Config.UncollectedCrabpots || Config.NotBaitedCrabpots)
                            CheckForCrabpots(location);
                    CheckBigCraftables(location);
                }
                OpenReport();
                _report.Clear();
            }
            else if (e.KeyPressed.ToString() == Config.OpenSettings)
            {
                SettingsMenu.OpenMenu(this);
            }
        }

        private void OpenReport()
        {
            if (Game1.activeClickableMenu != null)
                Game1.exitActiveMenu();
            Game1.activeClickableMenu = new ReportMenu(this, _report.ToString());
        }

        private void CheckForCrops(GameLocation location)
        {
            foreach (var pair in location.terrainFeatures)
            {
                if (!(pair.Value is HoeDirt dirt) || dirt.crop == null) continue;

                if (Config.DeadCrops && dirt.crop.dead)
                {
                    _report.AddDeadCrop(location.name, pair.Key,
                        Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
                    continue;
                }
                if (Config.UnwateredCrops && dirt.state == 0 &&
                    (dirt.crop.currentPhase < dirt.crop.phaseDays.Count - 1 || dirt.crop.dayOfCurrentPhase > 0))
                    _report.AddUnwateredCrop(location.name, pair.Key,
                        Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
                if (Config.UnharvestedCrops && dirt.crop.currentPhase >= dirt.crop.phaseDays.Count - 1 &&
                    dirt.crop.dayOfCurrentPhase == 0)
                    _report.AddUnharvestedCrop(location.name, pair.Key,
                        Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
            }
        }

        private void CheckForUnpettedPet(GameLocation location)
        {
            foreach (var npc in location.characters)
            {
                if (!(npc is Pet pet)) continue;

                var wasPettedToday = Helper.Reflection.GetPrivateValue<bool>(pet, "wasPetToday");
                if (!wasPettedToday)
                    _report.PetWasNotPetted();
                _report.PetExists = true;
                return;
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void CheckForUnfilledPetBowl(Farm farm)
        {
            if (farm.getTileIndexAt(54, 7, "Buildings") == 1938)
                _report.PetBowlNotFilled();
        }

        private void CheckAnimals(Farm farm)
        {
            foreach (var farmAnimal in farm.getAllFarmAnimals())
            {
                if (Config.UnpettedAnimals && !farmAnimal.wasPet)
                    _report.AddUnpettedAnimal(farmAnimal);

                if (!_checkAnimalProducts || farmAnimal.currentProduce <= 0) continue;

                if (farmAnimal.type.Contains("Cow") && Config.AnimalProducts["Cow milk"] ||
                    farmAnimal.type.Contains("Goat") && Config.AnimalProducts["Goat milk"] ||
                    farmAnimal.type.Contains("Sheep") && Config.AnimalProducts["Sheep wool"])
                    _report.AddUncollectedAnimalProduct(farmAnimal);
            }
        }

        private void CheckForMissingHay(BuildableGameLocation location)
        {
            foreach (var building in location.buildings)
            {
                if (building.isUnderConstruction() || !(building.indoors is AnimalHouse animalHouse)) continue;

                var hays = animalHouse.numberOfObjectsWithName("Hay");
                if (hays < animalHouse.animalLimit)
                    _report.AddMissingHay(building, animalHouse.animalLimit - hays);
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void CheckFarmCave(FarmCave farmCave)
        {
            foreach (var pair in farmCave.objects)
                if (pair.Value.parentSheetIndex == 128 && pair.Value.heldObject != null && pair.Value.readyForHarvest)
                    _report.AddFarmCaveObject(pair.Value.heldObject);
                else if (Array.BinarySearch(Fruits, pair.Value.parentSheetIndex) >= 0)
                    _report.AddFarmCaveObject(pair.Value);
        }

        private void CheckForCrabpots(GameLocation location)
        {
            foreach (var pair in location.objects)
            {
                if (!(pair.Value is CrabPot cb)) continue;

                if (Config.UncollectedCrabpots && cb.heldObject != null)
                    _report.AddUncollectedCrabpot(cb, location.name);
                if (Config.NotBaitedCrabpots && cb.bait == null)
                    _report.AddNotBaitedCrabpot(cb, location.name);
            }
        }

        private void CheckBigCraftables(GameLocation location)
        {
            foreach (var pair in location.objects)
            {
                if (!pair.Value.bigCraftable || !_checkMachines) continue;

                if (Config.Machines.ContainsKey(pair.Value.name) && Config.Machines[pair.Value.name] && pair.Value.readyForHarvest)
                    _report.AddMachine(pair.Value, location.name);
            }
        }
    }
}