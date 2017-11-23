using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DailyTasksReport.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace DailyTasksReport.Tasks
{
    public class AnimalsTask : Task
    {
        private readonly ModConfig _config;
        private bool _checkAnyProductDirectFromAnimal;

        private readonly List<TaskItem<FarmAnimal>> _unpettedAnimals = new List<TaskItem<FarmAnimal>>();
        private readonly List<TaskItem<FarmAnimal>> _animalProducts = new List<TaskItem<FarmAnimal>>();
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

            if (_checkAnyProductDirectFromAnimal && _animalProducts.Count > 0)
            {
                stringBuilder.Append($"Uncollected animal products: {_animalProducts.Count}^");
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

            if (_checkAnyProductDirectFromAnimal && _animalProducts.Count > 0)
            {
                stringBuilder.Append("Animal products:^");
                linesCount++;

                foreach (var animal in _animalProducts)
                {
                    var produceName = Game1.objectInformation[animal.Object.currentProduce].Split("/".ToCharArray(), 2)[0];
                    stringBuilder.Append($"{animal.Object.type} {animal.Object.displayName} has {produceName} at {animal.Location.name}^");
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
            _animalProducts.Clear();
            _missingHay.Clear();
        }

        private void DoCheck()
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            var farm = Game1.locations.Find(l => l is Farm) as Farm;

            // ReSharper disable once PossibleNullReferenceException
            foreach (var animal in farm.animals.Values)
            {
                TaskItem<FarmAnimal> item = null;
                if (_config.UnpettedAnimals && !animal.wasPet)
                {
                    item = new TaskItem<FarmAnimal>(farm, animal.Position, animal.displayName, animal);
                    _unpettedAnimals.Add(item);
                }

                if (!_checkAnyProductDirectFromAnimal || animal.currentProduce <= 0) continue;

                if (animal.type.Contains("Cow") && _config.AnimalProducts["Cow milk"] ||
                    animal.type.Contains("Goat") && _config.AnimalProducts["Goat milk"] ||
                    animal.type.Contains("Sheep") && _config.AnimalProducts["Sheep wool"])
                    _animalProducts.Add(item ?? new TaskItem<FarmAnimal>(farm, animal.Position, animal.displayName,
                                            animal));
            }

            foreach (var building in farm.buildings)
            {
                if (!(building.indoors is AnimalHouse animalHouse) || building.isUnderConstruction()) continue;

                foreach (var animal in animalHouse.animals.Values)
                {
                    TaskItem<FarmAnimal> item = null;
                    if (!animal.wasPet)
                    {
                        item = new TaskItem<FarmAnimal>(animalHouse, animal.Position, animal.displayName, animal);
                        _unpettedAnimals.Add(item);
                    }

                    if (!_checkAnyProductDirectFromAnimal || animal.currentProduce <= 0) continue;

                    if (animal.type.Contains("Cow") && _config.AnimalProducts["Cow milk"] ||
                        animal.type.Contains("Goat") && _config.AnimalProducts["Goat milk"] ||
                        animal.type.Contains("Sheep") && _config.AnimalProducts["Sheep wool"])
                        _animalProducts.Add(item ?? new TaskItem<FarmAnimal>(farm, animal.Position, animal.displayName,
                                                animal));
                }

                if (_config.MissingHay)
                {
                    var hays = animalHouse.numberOfObjectsWithName("Hay");
                    if (hays < animalHouse.animalLimit)
                        _missingHay.Add(Tuple.Create(building, animalHouse.animalLimit - hays));
                }
            }
        }
    }
}