using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyTasksReport
{
    internal class ReportBuilder
    {
        ModEntry parent;
        IList<Tuple<Vector2, string>> unwateredCropsInFarm = new List<Tuple<Vector2, string>>();
        IList<Tuple<Vector2, string>> unwateredCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        IList<Tuple<Vector2, string>> unharvestedCropsInFarm = new List<Tuple<Vector2, string>>();
        IList<Tuple<Vector2, string>> unharvestedCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        IList<Tuple<Vector2, string>> deadCropsInFarm = new List<Tuple<Vector2, string>>();
        IList<Tuple<Vector2, string>> deadCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        internal bool petExists = false;
        bool petWasPetted = true;
        bool petBowlFilled = true;
        IList<FarmAnimal> unpettedAnimals = new List<FarmAnimal>();
        IList<FarmAnimal> uncollectedAnimalProductFromAnimal = new List<FarmAnimal>();
        IList<Tuple<Building, int>> missingHay = new List<Tuple<Building, int>>();
        int totalHay = 0;
        string farmCaveChoice;
        Dictionary<string, int> objectsInFarmCave = new Dictionary<string, int>();
        int caveObjectsCount = 0;
        IList<Tuple<CrabPot, string>> uncollectedCrabpots = new List<Tuple<CrabPot, string>>();
        IList<Tuple<CrabPot, string>> notBaitedCrabpots = new List<Tuple<CrabPot, string>>();
        IList<Tuple<StardewValley.Object, string>> uncollectedMachines = new List<Tuple<StardewValley.Object, string>>();


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
            if (uncollectedAnimalProductFromAnimal.Count > 0 && (++count > 0))
                stringBuilder.Append($"Uncollected animal products: {uncollectedAnimalProductFromAnimal.Count}^");
            if (totalHay > 0 && (++count > 0))
                stringBuilder.Append($"Empty hay spots on feeding benches: {totalHay}^");
            if (objectsInFarmCave.Count > 0 && (++count > 0))
                stringBuilder.Append($"{farmCaveChoice} in the farm cave: {caveObjectsCount}^");
            if (uncollectedCrabpots.Count > 0 && (++count > 0))
                stringBuilder.Append($"Crabpots ready to collect: {uncollectedCrabpots.Count}^");
            if (notBaitedCrabpots.Count > 0 && (++count > 0))
                stringBuilder.Append($"Crabpots not baited: {notBaitedCrabpots.Count}^");
            if (uncollectedMachines.Count > 0 && (++count > 0))
                stringBuilder.Append($"Uncollected machines: {uncollectedMachines.Count}^");

            if (count == 0)
            {
                stringBuilder.Append("All done!");
                return stringBuilder.ToString();
            }

            if (parent.config.ShowDetailedInfo)
            {
                NextPage(ref stringBuilder, ref count);

                if (unwateredCropsInFarm.Count > 0 || unwateredCropsInGreenhouse.Count > 0)
                {
                    stringBuilder.Append("Unwatered crops:^");
                    count++;

                    EchoForCrops(ref stringBuilder, unwateredCropsInFarm, ref count, "Farm");
                    EchoForCrops(ref stringBuilder, unwateredCropsInGreenhouse, ref count, "Greenhouse");

                    NextPage(ref stringBuilder, ref count);
                }

                if (unharvestedCropsInFarm.Count > 0 || unharvestedCropsInGreenhouse.Count > 0)
                {
                    stringBuilder.Append("Ready to harvest crops:^");
                    count++;

                    EchoForCrops(ref stringBuilder, unharvestedCropsInFarm, ref count, "Farm");
                    EchoForCrops(ref stringBuilder, unharvestedCropsInGreenhouse, ref count, "Greenhouse");

                    NextPage(ref stringBuilder, ref count);
                }

                if (deadCropsInFarm.Count > 0 || deadCropsInGreenhouse.Count > 0)
                {
                    stringBuilder.Append("Dead Crops:^");
                    count++;

                    EchoForCrops(ref stringBuilder, deadCropsInFarm, ref count, "Farm");
                    EchoForCrops(ref stringBuilder, deadCropsInGreenhouse, ref count, "Greenhouse");

                    NextPage(ref stringBuilder, ref count);
                }

                if (unpettedAnimals.Count > 0)
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

                if (uncollectedAnimalProductFromAnimal.Count > 0)
                {
                    stringBuilder.Append("Animal products:^");
                    ++count;

                    foreach (FarmAnimal animal in uncollectedAnimalProductFromAnimal)
                    {
                        string produceName = Game1.objectInformation[animal.currentProduce].Split("/".ToCharArray(), 2)[0];
                        stringBuilder.Append($"{animal.type} {animal.displayName} has {produceName}^");
                        ++count;
                    }

                    NextPage(ref stringBuilder, ref count);
                }

                if (totalHay > 0)
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

                if (objectsInFarmCave.Count > 0)
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

                if (uncollectedCrabpots.Count > 0)
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

                if (notBaitedCrabpots.Count > 0)
                {
                    stringBuilder.Append("Crabpots still not baited:^");
                    ++count;

                    foreach (Tuple<CrabPot, string> t in notBaitedCrabpots)
                    {
                        stringBuilder.Append($"{t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                        count++;
                    }

                    NextPage(ref stringBuilder, ref count);
                }

                if (uncollectedMachines.Count > 0)
                {
                    stringBuilder.Append("Machines ready to collect:^");
                    ++count;

                    foreach (Tuple<StardewValley.Object, string> t in uncollectedMachines)
                    {
                        stringBuilder.Append($"{t.Item1.name} with {t.Item1.heldObject.name} at {t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                        ++count;
                    }
                }
            }

            return stringBuilder.ToString();
        }

        internal void AddUncollectedAnimalProduct(FarmAnimal farmAnimal)
        {
            uncollectedAnimalProductFromAnimal.Add(farmAnimal);
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

        internal void AddMachine(StardewValley.Object machine, string locationName)
        {
            uncollectedMachines.Add(Tuple.Create(machine, locationName));
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
            uncollectedAnimalProductFromAnimal.Clear();
            petExists = false;
            petBowlFilled = true;
            petWasPetted = true;
            missingHay.Clear();
            totalHay = 0;
            uncollectedCrabpots.Clear();
            notBaitedCrabpots.Clear();
            objectsInFarmCave.Clear();
            caveObjectsCount = 0;
            uncollectedMachines.Clear();
        }
    }
}
