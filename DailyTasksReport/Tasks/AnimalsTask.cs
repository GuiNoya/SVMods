using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DailyTasksReport.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Object = StardewValley.Object;

namespace DailyTasksReport.Tasks
{
    public class AnimalsTask : Task
    {
        private static readonly int[] Eggs = { 174, 176, 180, 182 };

        private readonly ModConfig _config;
        private bool _checkAnyProductDirectFromAnimal;
        private bool _checkAnyProductToCollectInCoop;

        private readonly List<TaskItem<FarmAnimal>> _unpettedAnimals = new List<TaskItem<FarmAnimal>>();
        private readonly List<TaskItem<FarmAnimal>> _animalProductsToHarvest = new List<TaskItem<FarmAnimal>>();
        private readonly List<TaskItem<Object>> _animalProductsToCollect = new List<TaskItem<Object>>();
        private readonly List<Tuple<Building, int>> _missingHay = new List<Tuple<Building, int>>();
        
        internal AnimalsTask(ModConfig config)
        {
            _config = config;
            SettingsMenu_ReportConfigChanged(null, null);

            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }

        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
            _checkAnyProductDirectFromAnimal = _config.AnimalProducts["Cow milk"] ||
                                               _config.AnimalProducts["Goat milk"] ||
                                               _config.AnimalProducts["Sheep wool"];
            _checkAnyProductToCollectInCoop = _config.AnimalProducts["Chicken egg"] ||
                                              _config.AnimalProducts["Dinosaur egg"] ||
                                              _config.AnimalProducts["Duck egg"] ||
                                              _config.AnimalProducts["Duck feather"] ||
                                              _config.AnimalProducts["Rabit's wool"] ||
                                              _config.AnimalProducts["Rabit's foot"];
        }

        public override string GeneralInfo(out int usedLines)
        {
            var stringBuilder = new StringBuilder();
            usedLines = 0;

            Clear();
            DoCheck();

            if (_config.UnpettedAnimals && _unpettedAnimals.Count > 0)
            {
                stringBuilder.Append($"Not petted animals: {_unpettedAnimals.Count}^");
                usedLines++;
            }

            if (_animalProductsToHarvest.Count + _animalProductsToCollect.Count > 0)
            {
                stringBuilder.Append($"Uncollected animal products: {_animalProductsToHarvest.Count + _animalProductsToCollect.Count}^");
                usedLines++;
            }

            if (_config.MissingHay && _missingHay.Count > 0)
            {
                stringBuilder.Append($"Empty hay spots on feeding benches: {_missingHay.Sum(t => t.Item2)}^");
                usedLines++;
            }

            return stringBuilder.ToString();
        }

        public override string DetailedInfo()
        {
            var stringBuilder = new StringBuilder();
            var linesCount = 0;

            if (_config.UnpettedAnimals && _unpettedAnimals.Count > 0)
            {
                stringBuilder.Append("Not petted animals:^");
                linesCount++;

                foreach (var animal in _unpettedAnimals)
                {
                    stringBuilder.Append($"{animal.Object.type} {animal.Object.displayName} at {animal.Location.name}^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_animalProductsToHarvest.Count + _animalProductsToCollect.Count > 0)
            {
                stringBuilder.Append("Animal products:^");
                linesCount++;

                foreach (var animal in _animalProductsToHarvest)
                {
                    var produceName = Game1.objectInformation[animal.Object.currentProduce].Split("/".ToCharArray(), 2)[0];
                    stringBuilder.Append($"{animal.Object.type} {animal.Object.displayName} has {produceName} at {animal.Location.name}^");
                    linesCount++;
                }

                foreach (var product in _animalProductsToCollect)
                {

                    stringBuilder.Append($"{product.Name} at {product.Location.name} ({product.Position.X}, {product.Position.Y})^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_config.MissingHay && _missingHay.Count > 0)
            {
                stringBuilder.Append("Feedbenches not full of hay:^");
                linesCount++;

                foreach (var tuple in _missingHay)
                {
                    var s = "s";
                    if (tuple.Item2 == 1)
                        s = string.Empty;
                    stringBuilder.Append($"{tuple.Item2} hay{s} missing at {tuple.Item1.indoors.Name} ({tuple.Item1.tileX}, {tuple.Item1.tileY})^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            return stringBuilder.ToString();
        }

        public override void Clear()
        {
            _unpettedAnimals.Clear();
            _animalProductsToHarvest.Clear();
            _animalProductsToCollect.Clear();
            _missingHay.Clear();
        }

        private void DoCheck()
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            var farm = Game1.locations.Find(l => l is Farm) as Farm;
            
            // Checking animals
            CheckAnimals(farm, null);

            // ReSharper disable once PossibleNullReferenceException
            // Checking animal products that can be collected
            foreach (var building in farm.buildings)
            {
                if (building.isUnderConstruction()) continue;

                if (building.indoors is AnimalHouse animalHouse)
                {
                    // Check animals
                    CheckAnimals(null, animalHouse);

                    // Check for object in Coop
                    if (building is Coop && _checkAnyProductToCollectInCoop)
                    {
                        foreach (var pair in animalHouse.objects)
                        {
                            if (Array.BinarySearch(Eggs, pair.Value.parentSheetIndex) >= 0 && _config.AnimalProducts["Chicken egg"] ||
                                pair.Value.parentSheetIndex == 442 && _config.AnimalProducts["Duck egg"] ||
                                pair.Value.parentSheetIndex == 107 && _config.AnimalProducts["Dinosaur egg"] ||
                                pair.Value.parentSheetIndex == 444 && _config.AnimalProducts["Duck feather"] ||
                                pair.Value.parentSheetIndex == 446 && _config.AnimalProducts["Rabit's foot"] ||
                                pair.Value.parentSheetIndex == 440 && _config.AnimalProducts["Rabit's wool"])
                            {
                                _animalProductsToCollect.Add(new TaskItem<Object>(animalHouse, pair.Key, pair.Value.name, pair.Value));
                            }
                        }
                    }
                    
                    // Check for hay
                    if (_config.MissingHay)
                    {
                        var hays = animalHouse.numberOfObjectsWithName("Hay");
                        if (hays < animalHouse.animalLimit)
                            _missingHay.Add(Tuple.Create(building, animalHouse.animalLimit - hays));
                    }
                }
                // Check Slime hutch
                else if (building.indoors is SlimeHutch slimeHutch && _config.AnimalProducts["Slime ball"])
                {
                    foreach (var pair in building.indoors.objects)
                    {
                        if (pair.Value.parentSheetIndex >= 56 && pair.Value.parentSheetIndex <= 61)
                        {
                            _animalProductsToCollect.Add(new TaskItem<Object>(slimeHutch, pair.Key, pair.Value.name, pair.Value));
                        }
                    }
                }
            }
            
            CheckForTruffles(farm);
        }

        private void CheckAnimals(Farm farm, AnimalHouse animalHouse)
        {
            dynamic location;
            if (farm != null)
                location = farm;
            else if (animalHouse != null)
                location = animalHouse;
            else
                return;

            foreach (var animal in location.animals.Values)
            {
                TaskItem<FarmAnimal> item = null;
                if (_config.UnpettedAnimals && !animal.wasPet)
                {
                    item = new TaskItem<FarmAnimal>(location, animal.Position, animal.displayName, animal);
                    _unpettedAnimals.Add(item);
                }

                if (!_checkAnyProductDirectFromAnimal || animal.currentProduce <= 0) continue;

                if (animal.type.Contains("Cow") && _config.AnimalProducts["Cow milk"] ||
                    animal.type.Contains("Goat") && _config.AnimalProducts["Goat milk"] ||
                    animal.type.Contains("Sheep") && _config.AnimalProducts["Sheep wool"])
                    _animalProductsToHarvest.Add(item ?? new TaskItem<FarmAnimal>(location, animal.Position, animal.displayName,
                                                     animal));
            }
        }

        private void CheckForTruffles(GameLocation farm)
        {
            if (!_config.AnimalProducts["Truffle"]) return;

            foreach (var pair in farm.objects)
            {
                if (pair.Value.parentSheetIndex == 430)
                {
                    _animalProductsToCollect.Add(new TaskItem<Object>(farm, pair.Key, pair.Value.name, pair.Value));
                }
            }
        }
    }
}