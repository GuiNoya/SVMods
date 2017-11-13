using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace DailyTasksReport
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        internal ModConfig config;
        private ReportBuilder report;

        internal bool checkAnimalProducts;
        internal bool checkMachines;

        private static readonly int[] fruits = new int[] { 296, 396, 406, 410, 613, 634, 635, 636, 637, 638 };

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            checkAnimalProducts = config.AnimalProducts.ContainsValue(true);
            checkMachines = config.Machines.ContainsValue(true);

            report = new ReportBuilder(this);

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.Button == config.OpenReportKey)
            {
                report.Clear();

                foreach (GameLocation location in Game1.locations)
                {
                    if (location is Farm || location.name == "Greenhouse")
                    {
                        if (config.UnwateredCrops || config.UnharvestedCrops || config.DeadCrops)
                            CheckForCrops(location);
                    }
                    if (location is Farm || location is FarmHouse)
                    {
                        if (config.UnpettedPet)
                            CheckForUnpettedPet(location);
                    }
                    if (location is BuildableGameLocation buildableLocation)
                    {
                        if (config.MissingHay)
                            CheckForMissingHay(buildableLocation);
                    }
                    if (location is Farm farm)
                    {
                        if (config.UnpettedAnimals || checkAnimalProducts)
                            CheckAnimals(farm);
                        if (config.UnfilledPetBowl)
                            CheckForUnfilledPetBowl(farm);
                    }
                    if (location is FarmCave)
                    {
                        if (config.FarmCave)
                            CheckFarmCave(location as FarmCave);
                    }
                    if (location.waterTiles != null)
                    {
                        if (config.UncollectedCrabpots || config.NotBaitedCrabpots)
                            CheckForCrabpots(location);
                    }
                    CheckBigCraftables(location);

                }
                OpenReport();
                report.Clear();
            }
        }
        
        private void OpenReport()
        {
            if (Game1.activeClickableMenu != null)
            {
                Game1.exitActiveMenu();
            }
            Game1.activeClickableMenu = new ReportMenu(this, report.ToString());
        }

        private void CheckForCrops(GameLocation location)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures)
            {
                if (pair.Value is HoeDirt dirt && dirt.crop != null)
                {
                    if (config.DeadCrops && dirt.crop.dead)
                    {
                        report.AddDeadCrop(location.name, pair.Key, Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
                        continue;
                    }
                    if (config.UnwateredCrops && dirt.state == 0 && (dirt.crop.currentPhase < dirt.crop.phaseDays.Count - 1 || dirt.crop.dayOfCurrentPhase > 0))
                    {
                        report.AddUnwateredCrop(location.name, pair.Key, Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
                    }
                    if (config.UnharvestedCrops && dirt.crop.currentPhase >= dirt.crop.phaseDays.Count - 1 && dirt.crop.dayOfCurrentPhase == 0)
                    {
                        report.AddUnharvestedCrop(location.name, pair.Key, Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0]);
                    }
                }
            }
        }

        private void CheckForUnpettedPet(GameLocation location)
        {
            foreach (NPC npc in location.characters)
            {
                if (npc is Pet pet)
                {
                    bool wasPettedToday = Helper.Reflection.GetPrivateValue<bool>(pet, "wasPetToday");
                    if (!wasPettedToday)
                        report.PetWasNotPetted();
                    report.petExists = true;
                    return;
                }
            }
        }

        private void CheckForUnfilledPetBowl(Farm farm)
        {
            if (farm.getTileIndexAt(54, 7, "Buildings") == 1938)
            {
                report.PetBowlNotFilled();
            }
        }

        private void CheckAnimals(Farm farm)
        {
            foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals())
            {
                if (config.UnpettedAnimals && !farmAnimal.wasPet)
                {
                    report.AddUnpettedAnimal(farmAnimal);
                }
                if (checkAnimalProducts && farmAnimal.currentProduce > 0)
                {
                    if ((farmAnimal.type.Contains("Cow") && config.AnimalProducts["Cow milk"]) ||
                        (farmAnimal.type.Contains("Goat") && config.AnimalProducts["Goat milk"]) ||
                        (farmAnimal.type.Contains("Sheep") && config.AnimalProducts["Sheep wool"]))
                    {
                        report.AddUncollectedAnimalProduct(farmAnimal);
                    }
                }
            }
        }

        private void CheckForMissingHay(BuildableGameLocation location)
        {
            foreach (Building building in location.buildings)
            {
                if (!building.isUnderConstruction() && building.indoors is AnimalHouse animalHouse)
                {
                    int hays = animalHouse.numberOfObjectsWithName("Hay");
                    if (hays < animalHouse.animalLimit)
                    {
                        report.AddMissingHay(building, animalHouse.animalLimit - hays);
                    }
                }
            }
        }

        private void CheckFarmCave(FarmCave farmCave)
        {
            foreach (KeyValuePair<Vector2, StardewValley.Object> pair in farmCave.objects)
            {
                if (pair.Value.parentSheetIndex == 128 && pair.Value.heldObject != null && pair.Value.readyForHarvest)
                {
                    report.AddFarmCaveObject(pair.Value.heldObject);
                }
                else if (Array.BinarySearch(fruits, pair.Value.parentSheetIndex) >= 0)
                {
                    report.AddFarmCaveObject(pair.Value);
                }
            }
        }

        private void CheckForCrabpots(GameLocation location)
        {
            foreach (KeyValuePair<Vector2, StardewValley.Object> pair in location.objects)
            {
                if (pair.Value is CrabPot cb)
                {
                    if (config.UncollectedCrabpots && cb.heldObject != null)
                    {
                        report.AddUncollectedCrabpot(cb, location.name);
                    }
                    if (config.NotBaitedCrabpots && cb.bait == null)
                    {
                        report.AddNotBaitedCrabpot(cb, location.name);
                    }
                }
            }
        }

        private void CheckBigCraftables(GameLocation location)
        {
            foreach (KeyValuePair<Vector2, StardewValley.Object> pair in location.objects)
            {
                if (pair.Value.bigCraftable && checkMachines)
                {
                    if (config.Machines.ContainsKey(pair.Value.name) && config.Machines[pair.Value.name] && pair.Value.readyForHarvest)
                    {
                        report.AddMachine(pair.Value, location.name);
                    }
                }
            }
        }
    }
}
