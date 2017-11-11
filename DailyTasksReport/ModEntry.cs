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
using System.Text;

namespace DailyTasksReport

{
    /// <summary>The mod entry point.</summary>
    class ModEntry : Mod
    {
        internal ModConfig config;
        private ReportBuilder report;

        static readonly int[] fruits = new int[] { 296, 396, 406, 410, 613, 634, 635, 636, 637, 638 };

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            report = new ReportBuilder(this);

            ControlEvents.KeyPressed += this.ControlEvents_KeyPress;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ControlEvents_KeyPress(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.KeyPressed.ToString() == config.OpenReportKey)
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
                        if (config.UnpettedAnimals)
                            CheckForUnpettedAnimals(farm);
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
                    report.FoundPet();
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

        private void CheckForUnpettedAnimals(Farm farm)
        {
            foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals())
            {
                if (!farmAnimal.wasPet)
                {
                    report.AddUnpettedAnimal(farmAnimal);
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
        
        private class ReportBuilder
        {
            ModEntry parent;
            IList<Tuple<Vector2, string>> unwateredCropsInFarm = new List<Tuple<Vector2, string>>();
            IList<Tuple<Vector2, string>> unwateredCropsInGreenhouse = new List<Tuple<Vector2, string>>();
            IList<Tuple<Vector2, string>> unharvestedCropsInFarm = new List<Tuple<Vector2, string>>();
            IList<Tuple<Vector2, string>> unharvestedCropsInGreenhouse = new List<Tuple<Vector2, string>>();
            IList<Tuple<Vector2, string>> deadCropsInFarm = new List<Tuple<Vector2, string>>();
            IList<Tuple<Vector2, string>> deadCropsInGreenhouse = new List<Tuple<Vector2, string>>();
            bool petExists = false;
            bool petWasPetted = true;
            bool petBowlFilled = true;
            IList<FarmAnimal> unpettedAnimals = new List<FarmAnimal>();
            IList<Tuple<Building, int>> missingHay = new List<Tuple<Building, int>>();
            int totalHay = 0;
            string farmCaveChoice;
            Dictionary<string, int> objectsInFarmCave = new Dictionary<string, int>();
            int caveObjectsCount = 0;
            IList<Tuple<CrabPot, string>> uncollectedCrabpots = new List<Tuple<CrabPot, string>>();
            IList<Tuple<CrabPot, string>> notBaitedCrabpots = new List<Tuple<CrabPot, string>>();

            public ReportBuilder(ModEntry parent)
            {
                this.parent = parent;
            }

            public override string ToString()
            {
                int count = 0;
                StringBuilder stringBuilder = new StringBuilder();
                if (Game1.player.caveChoice == 1)
                    farmCaveChoice = "Fruits";
                else if (Game1.player.caveChoice == 2)
                    farmCaveChoice = "Mushrooms";

                if (unwateredCropsInFarm.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Farm crops not watered: {unwateredCropsInFarm.Count}^");
                if (unwateredCropsInGreenhouse.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Greenhouse crops not watered: {unwateredCropsInGreenhouse.Count}^");
                if (unharvestedCropsInFarm.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Farm crops ready to harvest: {unharvestedCropsInFarm.Count}^");
                if (unharvestedCropsInGreenhouse.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Greenhouse crops ready to harvested {unharvestedCropsInGreenhouse.Count}^");
                if (deadCropsInFarm.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Dead crops in the farm: {deadCropsInFarm.Count}^");
                if (deadCropsInGreenhouse.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Dead crops in the greenhouse {deadCropsInGreenhouse.Count}^");
                if (petExists && !petBowlFilled && (++count > 0))
                    stringBuilder.Append($"You did not fill your pet's bowl.^");
                if (petExists && !petWasPetted && (++count > 0))
                    stringBuilder.Append($"You did not pet your pet today.^");
                if (unpettedAnimals.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Not petted animals: {unpettedAnimals.Count}^");
                if (totalHay > 0 && (++count > 0))
                    stringBuilder.Append($"Empty hay spots on feeding benches: {totalHay}^");
                if (objectsInFarmCave.Count > 0 && (++count > 0))
                    stringBuilder.Append($"{farmCaveChoice} in the farm cave: {caveObjectsCount}^");
                if (uncollectedCrabpots.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Crabpots ready to collect: {uncollectedCrabpots.Count}^");
                if (notBaitedCrabpots.Count > 0 && (++count > 0))
                    stringBuilder.Append($"Crabpots not baited: {notBaitedCrabpots.Count}^");

                if (count == 0)
                {
                    stringBuilder.Append("All done!");
                    return stringBuilder.ToString();
                }

                if (parent.config.ShowDetailedInfo)
                {
                    NextPage(ref stringBuilder, ref count);

                    if (parent.config.UnwateredCrops && (unwateredCropsInFarm.Count > 0 || unwateredCropsInGreenhouse.Count > 0))
                    {
                        stringBuilder.Append("Unwatered crops:^");
                        count++;

                        EchoForCrops(ref stringBuilder, unwateredCropsInFarm, ref count, "Farm");
                        EchoForCrops(ref stringBuilder, unwateredCropsInGreenhouse, ref count, "Greenhouse");

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.UnharvestedCrops && (unharvestedCropsInFarm.Count > 0 || unharvestedCropsInGreenhouse.Count > 0))
                    {
                        stringBuilder.Append("Ready to harvest crops:^");
                        count++;

                        EchoForCrops(ref stringBuilder, unharvestedCropsInFarm, ref count, "Farm");
                        EchoForCrops(ref stringBuilder, unharvestedCropsInGreenhouse, ref count, "Greenhouse");

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.DeadCrops && deadCropsInFarm.Count > 0 || deadCropsInGreenhouse.Count > 0)
                    {
                        stringBuilder.Append("Dead Crops:^");
                        count++;

                        EchoForCrops(ref stringBuilder, deadCropsInFarm, ref count, "Farm");
                        EchoForCrops(ref stringBuilder, deadCropsInGreenhouse, ref count, "Greenhouse");

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.UnpettedAnimals && unpettedAnimals.Count > 0)
                    {
                        stringBuilder.Append("Unpetted animals:^");
                        count++;

                        foreach (FarmAnimal animal in unpettedAnimals)
                        {
                            stringBuilder.Append($"{animal.type} {animal.displayName}^");
                            count++;
                        }

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.MissingHay && totalHay > 0)
                    {
                        stringBuilder.Append("Feedbenches not full of hay:^");
                        count++;

                        foreach (Tuple<Building, int> tuple in missingHay)
                        {
                            string s = "s";
                            if (tuple.Item2 == 1)
                                s = String.Empty;
                            stringBuilder.Append($"{tuple.Item2} hay{s} missing at {tuple.Item1.indoors.Name} ({tuple.Item1.tileX}, {tuple.Item1.tileY})^");
                            count++;
                        }

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.FarmCave && objectsInFarmCave.Count > 0)
                    {
                        stringBuilder.Append($"{farmCaveChoice} in the farm cave:^");
                        count++;
                        
                        foreach (KeyValuePair<string, int> pair in objectsInFarmCave)
                        {
                            string name = Pluralize(pair.Key, pair.Value);
                            stringBuilder.Append($"{pair.Value} {name}^");
                            count++;
                        }

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.UncollectedCrabpots && uncollectedCrabpots.Count > 0)
                    {
                        stringBuilder.Append("Uncollected crabpots:^");
                        count++;

                        foreach (Tuple<CrabPot, string> t in uncollectedCrabpots)
                        {
                            stringBuilder.Append($"{t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                            count++;
                        }

                        NextPage(ref stringBuilder, ref count);
                    }

                    if (parent.config.NotBaitedCrabpots && notBaitedCrabpots.Count > 0)
                    {
                        stringBuilder.Append("Crabpots still not baited:^");
                        ++count;

                        foreach (Tuple<CrabPot, string> t in notBaitedCrabpots)
                        {
                            stringBuilder.Append($"{t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                            count++;
                        }
                    }
                }

                return stringBuilder.ToString();
            }

            private string Pluralize(string name, int number)
            {
                if (number < 2)
                    return name;
                if (name.EndsWith("y"))
                    return name.Substring(0, name.Length - 1) + "ies";
                if (name == "Peach")
                    return name + "es";
                return name + "s";
            }
            
            private static void NextPage(ref StringBuilder stringBuilder, ref int count)
            {
                for (; count % 11 != 0; count++)
                {
                    stringBuilder.Append("^");
                }
            }

            private static void EchoForCrops(ref StringBuilder stringBuilder, IList<Tuple<Vector2, string>> list, ref int count, string name)
            {
                foreach (Tuple<Vector2, string> tuple in list)
                {
                    stringBuilder.Append($"{tuple.Item2} at {name} ({tuple.Item1.X}, {tuple.Item1.Y})^");
                    count++;
                }
            }

            internal void AddUnwateredCrop(string locationName, Vector2 pos, string cropName)
            {
                if (locationName == "Farm")
                    unwateredCropsInFarm.Add(Tuple.Create(pos, cropName));
                else if (locationName == "Greenhouse")
                    unwateredCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                else
                    parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
            }

            internal void AddUnharvestedCrop(string locationName, Vector2 pos, string cropName)
            {
                if (locationName == "Farm")
                    unharvestedCropsInFarm.Add(Tuple.Create(pos, cropName));
                else if (locationName == "Greenhouse")
                    unharvestedCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                else
                    parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
            }

            internal void AddDeadCrop(string locationName, Vector2 pos, string cropName)
            {
                if (locationName == "Farm")
                    deadCropsInFarm.Add(Tuple.Create(pos, cropName));
                else if (locationName == "Greenhouse")
                    deadCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                else
                    parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
            }

            internal void FoundPet()
            {
                petExists = true;
            }

            internal void PetWasNotPetted()
            {
                petWasPetted = false;
            }

            internal void PetBowlNotFilled()
            {
                petBowlFilled = false;
            }

            internal void AddUnpettedAnimal(FarmAnimal animal)
            {
                unpettedAnimals.Add(animal);
            }

            internal void AddMissingHay(Building building, int missing)
            {
                missingHay.Add(Tuple.Create(building, missing));
                totalHay += missing;
            }

            internal void AddFarmCaveObject(StardewValley.Object o)
            {
                if (objectsInFarmCave.ContainsKey(o.name))
                    objectsInFarmCave[o.name] += 1;
                else
                    objectsInFarmCave.Add(o.name, 1);
                caveObjectsCount++;
            }

            internal void AddUncollectedCrabpot(CrabPot cb, string locationName)
            {
                uncollectedCrabpots.Add(Tuple.Create(cb, locationName));
            }

            internal void AddNotBaitedCrabpot(CrabPot cb, string locationName)
            {
                notBaitedCrabpots.Add(Tuple.Create(cb, locationName));
            }

            internal void Clear()
            {
                unwateredCropsInFarm.Clear();
                unwateredCropsInGreenhouse.Clear();
                unharvestedCropsInFarm.Clear();
                unharvestedCropsInGreenhouse.Clear();
                deadCropsInFarm.Clear();
                deadCropsInGreenhouse.Clear();
                unpettedAnimals.Clear();
                petExists = false;
                petBowlFilled = true;
                petWasPetted = true;
                missingHay.Clear();
                totalHay = 0;
                uncollectedCrabpots.Clear();
                notBaitedCrabpots.Clear();
                objectsInFarmCave.Clear();
                caveObjectsCount = 0;
            }
            
        }
    }
}
