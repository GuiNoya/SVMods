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
        private readonly ModEntry _parent;
        private readonly IList<Tuple<Vector2, string>> _unwateredCropsInFarm = new List<Tuple<Vector2, string>>();
        private readonly IList<Tuple<Vector2, string>> _unwateredCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        private readonly IList<Tuple<Vector2, string>> _unharvestedCropsInFarm = new List<Tuple<Vector2, string>>();
        private readonly IList<Tuple<Vector2, string>> _unharvestedCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        private readonly IList<Tuple<Vector2, string>> _deadCropsInFarm = new List<Tuple<Vector2, string>>();
        private readonly IList<Tuple<Vector2, string>> _deadCropsInGreenhouse = new List<Tuple<Vector2, string>>();
        internal bool PetExists;
        private bool _petWasPetted = true;
        private bool _petBowlFilled = true;
        private readonly IList<FarmAnimal> _unpettedAnimals = new List<FarmAnimal>();
        private readonly IList<FarmAnimal> _uncollectedAnimalProductFromAnimal = new List<FarmAnimal>();
        private readonly IList<Tuple<Building, int>> _missingHay = new List<Tuple<Building, int>>();
        private int _totalHay;
        private string _farmCaveChoice;
        private readonly Dictionary<string, int> _objectsInFarmCave = new Dictionary<string, int>();
        private int _caveObjectsCount;
        private readonly IList<Tuple<CrabPot, string>> _uncollectedCrabpots = new List<Tuple<CrabPot, string>>();
        private readonly IList<Tuple<CrabPot, string>> _notBaitedCrabpots = new List<Tuple<CrabPot, string>>();
        private readonly IList<Tuple<StardewValley.Object, string>> _uncollectedMachines = new List<Tuple<StardewValley.Object, string>>();


        public ReportBuilder(ModEntry parent)
        {
            _parent = parent;
        }

        public override string ToString()
        {
            var count = 0;
            var stringBuilder = new StringBuilder();

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (Game1.player.caveChoice == 1)
                _farmCaveChoice = "Fruits";
            else if (Game1.player.caveChoice == 2)
                _farmCaveChoice = "Mushrooms";

            if (_unwateredCropsInFarm.Count > 0 && ++count > 0)
                stringBuilder.Append($"Farm crops not watered: {_unwateredCropsInFarm.Count}^");
            if (_unwateredCropsInGreenhouse.Count > 0 && ++count > 0)
                stringBuilder.Append($"Greenhouse crops not watered: {_unwateredCropsInGreenhouse.Count}^");
            if (_unharvestedCropsInFarm.Count > 0 && ++count > 0)
                stringBuilder.Append($"Farm crops ready to harvest: {_unharvestedCropsInFarm.Count}^");
            if (_unharvestedCropsInGreenhouse.Count > 0 && ++count > 0)
                stringBuilder.Append($"Greenhouse crops ready to harvested {_unharvestedCropsInGreenhouse.Count}^");
            if (_deadCropsInFarm.Count > 0 && ++count > 0)
                stringBuilder.Append($"Dead crops in the farm: {_deadCropsInFarm.Count}^");
            if (_deadCropsInGreenhouse.Count > 0 && ++count > 0)
                stringBuilder.Append($"Dead crops in the greenhouse {_deadCropsInGreenhouse.Count}^");
            if (PetExists && !_petBowlFilled && ++count > 0)
                stringBuilder.Append("You did not fill your pet's bowl.^");
            if (PetExists && !_petWasPetted && ++count > 0)
                stringBuilder.Append("You did not pet your pet today.^");
            if (_unpettedAnimals.Count > 0 && ++count > 0)
                stringBuilder.Append($"Not petted animals: {_unpettedAnimals.Count}^");
            if (_uncollectedAnimalProductFromAnimal.Count > 0 && ++count > 0)
                stringBuilder.Append($"Uncollected animal products: {_uncollectedAnimalProductFromAnimal.Count}^");
            if (_totalHay > 0 && ++count > 0)
                stringBuilder.Append($"Empty hay spots on feeding benches: {_totalHay}^");
            if (_objectsInFarmCave.Count > 0 && ++count > 0)
                stringBuilder.Append($"{_farmCaveChoice} in the farm cave: {_caveObjectsCount}^");
            if (_uncollectedCrabpots.Count > 0 && ++count > 0)
                stringBuilder.Append($"Crabpots ready to collect: {_uncollectedCrabpots.Count}^");
            if (_notBaitedCrabpots.Count > 0 && ++count > 0)
                stringBuilder.Append($"Crabpots not baited: {_notBaitedCrabpots.Count}^");
            if (_uncollectedMachines.Count > 0 && ++count > 0)
                stringBuilder.Append($"Uncollected machines: {_uncollectedMachines.Count}^");

            if (count == 0)
            {
                stringBuilder.Append("All done!");
                return stringBuilder.ToString();
            }

            if (!_parent.Config.ShowDetailedInfo)
                return stringBuilder.ToString();

            NextPage(ref stringBuilder, ref count);

            if (_unwateredCropsInFarm.Count > 0 || _unwateredCropsInGreenhouse.Count > 0)
            {
                stringBuilder.Append("Unwatered crops:^");
                count++;

                EchoForCrops(ref stringBuilder, _unwateredCropsInFarm, ref count, "Farm");
                EchoForCrops(ref stringBuilder, _unwateredCropsInGreenhouse, ref count, "Greenhouse");

                NextPage(ref stringBuilder, ref count);
            }

            if (_unharvestedCropsInFarm.Count > 0 || _unharvestedCropsInGreenhouse.Count > 0)
            {
                stringBuilder.Append("Ready to harvest crops:^");
                count++;

                EchoForCrops(ref stringBuilder, _unharvestedCropsInFarm, ref count, "Farm");
                EchoForCrops(ref stringBuilder, _unharvestedCropsInGreenhouse, ref count, "Greenhouse");

                NextPage(ref stringBuilder, ref count);
            }

            if (_deadCropsInFarm.Count > 0 || _deadCropsInGreenhouse.Count > 0)
            {
                stringBuilder.Append("Dead Crops:^");
                count++;

                EchoForCrops(ref stringBuilder, _deadCropsInFarm, ref count, "Farm");
                EchoForCrops(ref stringBuilder, _deadCropsInGreenhouse, ref count, "Greenhouse");

                NextPage(ref stringBuilder, ref count);
            }

            if (_unpettedAnimals.Count > 0)
            {
                stringBuilder.Append("Unpetted animals:^");
                count++;

                foreach (var animal in _unpettedAnimals)
                {
                    stringBuilder.Append($"{animal.type} {animal.displayName}^");
                    count++;
                }

                NextPage(ref stringBuilder, ref count);
            }

            if (_uncollectedAnimalProductFromAnimal.Count > 0)
            {
                stringBuilder.Append("Animal products:^");
                ++count;

                foreach (var animal in _uncollectedAnimalProductFromAnimal)
                {
                    var produceName = Game1.objectInformation[animal.currentProduce].Split("/".ToCharArray(), 2)[0];
                    stringBuilder.Append($"{animal.type} {animal.displayName} has {produceName}^");
                    ++count;
                }

                NextPage(ref stringBuilder, ref count);
            }

            if (_totalHay > 0)
            {
                stringBuilder.Append("Feedbenches not full of hay:^");
                count++;

                foreach (var tuple in _missingHay)
                {
                    var s = "s";
                    if (tuple.Item2 == 1)
                        s = string.Empty;
                    stringBuilder.Append($"{tuple.Item2} hay{s} missing at {tuple.Item1.indoors.Name} ({tuple.Item1.tileX}, {tuple.Item1.tileY})^");
                    count++;
                }

                NextPage(ref stringBuilder, ref count);
            }

            if (_objectsInFarmCave.Count > 0)
            {
                stringBuilder.Append($"{_farmCaveChoice} in the farm cave:^");
                count++;

                foreach (var pair in _objectsInFarmCave)
                {
                    var name = Pluralize(pair.Key, pair.Value);
                    stringBuilder.Append($"{pair.Value} {name}^");
                    count++;
                }

                NextPage(ref stringBuilder, ref count);
            }

            if (_uncollectedCrabpots.Count > 0)
            {
                stringBuilder.Append("Uncollected crabpots:^");
                count++;

                foreach (var t in _uncollectedCrabpots)
                {
                    stringBuilder.Append($"{t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                    count++;
                }

                NextPage(ref stringBuilder, ref count);
            }

            if (_notBaitedCrabpots.Count > 0)
            {
                stringBuilder.Append("Crabpots still not baited:^");
                ++count;

                foreach (var t in _notBaitedCrabpots)
                {
                    stringBuilder.Append($"{t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                    count++;
                }

                NextPage(ref stringBuilder, ref count);
            }

            // ReSharper disable once InvertIf
            if (_uncollectedMachines.Count > 0)
            {
                stringBuilder.Append("Machines ready to collect:^");
                ++count;

                foreach (var t in _uncollectedMachines)
                {
                    stringBuilder.Append($"{t.Item1.name} with {t.Item1.heldObject.name} at {t.Item2} ({t.Item1.tileLocation.X}, {t.Item1.tileLocation.Y})^");
                    ++count;
                }
            }

            return stringBuilder.ToString();
        }

        internal void AddUncollectedAnimalProduct(FarmAnimal farmAnimal)
        {
            _uncollectedAnimalProductFromAnimal.Add(farmAnimal);
        }

        private static string Pluralize(string name, int number)
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

        private static void EchoForCrops(ref StringBuilder stringBuilder, IEnumerable<Tuple<Vector2, string>> list, ref int count, string name)
        {
            foreach (var tuple in list)
            {
                stringBuilder.Append($"{tuple.Item2} at {name} ({tuple.Item1.X}, {tuple.Item1.Y})^");
                count++;
            }
        }

        internal void AddUnwateredCrop(string locationName, Vector2 pos, string cropName)
        {
            switch (locationName)
            {
                case "Farm":
                    _unwateredCropsInFarm.Add(Tuple.Create(pos, cropName));
                    break;
                case "Greenhouse":
                    _unwateredCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                    break;
                default:
                    _parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
                    break;
            }
        }

        internal void AddUnharvestedCrop(string locationName, Vector2 pos, string cropName)
        {
            switch (locationName)
            {
                case "Farm":
                    _unharvestedCropsInFarm.Add(Tuple.Create(pos, cropName));
                    break;
                case "Greenhouse":
                    _unharvestedCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                    break;
                default:
                    _parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
                    break;
            }
        }

        internal void AddDeadCrop(string locationName, Vector2 pos, string cropName)
        {
            switch (locationName)
            {
                case "Farm":
                    _deadCropsInFarm.Add(Tuple.Create(pos, cropName));
                    break;
                case "Greenhouse":
                    _deadCropsInGreenhouse.Add(Tuple.Create(pos, cropName));
                    break;
                default:
                    _parent.Monitor.Log("Crop location is not Farm nor Greenhouse.", LogLevel.Error);
                    break;
            }
        }

        internal void PetWasNotPetted()
        {
            _petWasPetted = false;
        }

        internal void PetBowlNotFilled()
        {
            _petBowlFilled = false;
        }

        internal void AddUnpettedAnimal(FarmAnimal animal)
        {
            _unpettedAnimals.Add(animal);
        }

        internal void AddMissingHay(Building building, int missing)
        {
            _missingHay.Add(Tuple.Create(building, missing));
            _totalHay += missing;
        }

        internal void AddFarmCaveObject(StardewValley.Object o)
        {
            if (_objectsInFarmCave.ContainsKey(o.name))
                _objectsInFarmCave[o.name] += 1;
            else
                _objectsInFarmCave.Add(o.name, 1);
            _caveObjectsCount++;
        }

        internal void AddUncollectedCrabpot(CrabPot cb, string locationName)
        {
            _uncollectedCrabpots.Add(Tuple.Create(cb, locationName));
        }

        internal void AddNotBaitedCrabpot(CrabPot cb, string locationName)
        {
            _notBaitedCrabpots.Add(Tuple.Create(cb, locationName));
        }

        internal void AddMachine(StardewValley.Object machine, string locationName)
        {
            _uncollectedMachines.Add(Tuple.Create(machine, locationName));
        }

        internal void Clear()
        {
            _unwateredCropsInFarm.Clear();
            _unwateredCropsInGreenhouse.Clear();
            _unharvestedCropsInFarm.Clear();
            _unharvestedCropsInGreenhouse.Clear();
            _deadCropsInFarm.Clear();
            _deadCropsInGreenhouse.Clear();
            _unpettedAnimals.Clear();
            _uncollectedAnimalProductFromAnimal.Clear();
            PetExists = false;
            _petBowlFilled = true;
            _petWasPetted = true;
            _missingHay.Clear();
            _totalHay = 0;
            _uncollectedCrabpots.Clear();
            _notBaitedCrabpots.Clear();
            _objectsInFarmCave.Clear();
            _caveObjectsCount = 0;
            _uncollectedMachines.Clear();
        }
    }
}
