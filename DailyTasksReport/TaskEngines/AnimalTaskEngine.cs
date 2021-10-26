using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewValley.Buildings;
using SObject = StardewValley.Object;
using StardewWeb;
using StardewWeb.StardewObjects;
using DailyTasksReport.Tasks;

namespace DailyTasksReport.TaskEngines
{
    class AnimalTaskEngine : TaskEngine
    {
        public static readonly int[] CollectableAnimalProducts = { 107, 174, 176, 180, 182, 440, 442, 444, 446 };

        private static readonly List<TaskItem<SObject>> AnimalProductsToCollect = new List<TaskItem<SObject>>();
        private static readonly List<TaskItem<SObject>> TrufflesToCollect = new List<TaskItem<SObject>>();
        private static readonly List<TaskItem<FarmAnimal>> AnimalProductsToHarvest = new List<TaskItem<FarmAnimal>>();
        private static readonly List<TaskItem<FarmAnimal>> UnpettedAnimals = new List<TaskItem<FarmAnimal>>();
        private static readonly List<Tuple<Building, int>> MissingHay = new List<Tuple<Building, int>>();
        private static readonly List<FishPond> PondsNeedingAttention = new List<FishPond>();
        private static readonly List<FishPond> PondsWithItem = new List<FishPond>();

        public readonly TaskReportConfig _config;
        public readonly AnimalsTaskId _id;
        public static AnimalsTaskId _who = AnimalsTaskId.None;
        private static Farm _farm;

        internal AnimalTaskEngine(TaskReportConfig config, AnimalsTaskId id)
        {
            _config = config;
            _id = id;
            TaskSubClass = _id.ToString();
            TaskClass = "Animal";

        }
        private static void ReScanUnpettedAnimals()
        {
            UnpettedAnimals.Clear();

            foreach (var animal in _farm.animals.Pairs)
                if (!animal.Value.wasPet.Value)
                    UnpettedAnimals.Add(new TaskItem<FarmAnimal>(_farm, animal.Value.Position, animal.Value));

            foreach (var building in _farm.buildings)
            {
                if (building.isUnderConstruction() || !(building.indoors.Value is AnimalHouse animalHouse))
                    continue;

                foreach (var animal in animalHouse.animals.Pairs)
                    if (!animal.Value.wasPet.Value)
                        UnpettedAnimals.Add(new TaskItem<FarmAnimal>(animalHouse, animal.Value.Position, animal.Value));
            }
        }

        private void UpdateList()
        {
            switch (_id)
            {
                case AnimalsTaskId.UnpettedAnimals:
                    UnpettedAnimals.RemoveAll(a => a.Object.wasPet.Value || a.Object.health.Value <= 0);
                    foreach (var animal in UnpettedAnimals)
                        animal.Location = _farm.animals.FieldDict.ContainsKey(animal.Object.myID.Value)
                            ? _farm
                            : animal.Object.home.indoors.Value;
                    break;

                case AnimalsTaskId.AnimalProducts:
                    AnimalProductsToCollect.RemoveAll(i =>
                        !(i.Location.objects.TryGetValue(i.Position, out var obj) &&
                          obj.ParentSheetIndex == i.Object.ParentSheetIndex));
                    TrufflesToCollect.Clear();
                    CheckForTruffles(_farm);
                    AnimalProductsToHarvest.RemoveAll(animal =>
                        animal.Object.currentProduce.Value <= 0 || animal.Object.health.Value <= 0);
                    foreach (var animal in AnimalProductsToHarvest)
                        animal.Location = _farm.animals.ContainsKey(animal.Object.myID.Value)
                            ? _farm
                            : animal.Object.home.indoors.Value;
                    break;

                case AnimalsTaskId.MissingHay:
                    MissingHay.Clear();
                    foreach (var building in _farm.buildings)
                    {
                        if (building.isUnderConstruction() || !(building.indoors.Value is AnimalHouse animalHouse)) continue;
                        var count = animalHouse.numberOfObjectsWithName("Hay");
                        var animalLimit = animalHouse.animalLimit.Value;
                        if (count < animalLimit)
                            MissingHay.Add(new Tuple<Building, int>(building, animalLimit - count));
                    }
                    break;
                case AnimalsTaskId.PondsNeedingAttention:
                    PondsNeedingAttention.Clear();
                    foreach (var building in _farm.buildings)
                    {
                        if (building.isUnderConstruction()) continue;

                        if (building is FishPond fishpond)
                        {
                            if (fishpond.HasUnresolvedNeeds())
                            {
                                PondsNeedingAttention.Add(fishpond);
                            }
                        }
                    }
                    break;
                case AnimalsTaskId.PondsWithItems:
                    PondsWithItem.Clear();
                    foreach (var building in _farm.buildings)
                    {
                        if (building.isUnderConstruction()) continue;

                        if (building is FishPond fishpond)
                        {
                            if (fishpond.output.Value!=null)
                            {
                                PondsWithItem.Add(fishpond);
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown animal task");
            }
        }

        private static void CheckAnimals(Farm farm)
        {
            foreach (var animal in farm.animals.Pairs)
            {
                if (!animal.Value.wasPet.Value)
                    UnpettedAnimals.Add(new TaskItem<FarmAnimal>(farm, animal.Value.Position, animal.Value));

                var currentProduce = animal.Value.currentProduce.Value;
                if (currentProduce > 0 && currentProduce != 430)
                    AnimalProductsToHarvest.Add(new TaskItem<FarmAnimal>(farm, animal.Value.Position, animal.Value));
            }
        }

        private static void CheckAnimals(AnimalHouse location)
        {
            foreach (var animal in location.animals.Pairs)
            {
                if (!animal.Value.wasPet.Value)
                    UnpettedAnimals.Add(new TaskItem<FarmAnimal>(location, animal.Value.position, animal.Value));

                var currentProduce = animal.Value.currentProduce.Value;
                if (currentProduce > 0 && currentProduce != 430)
                    AnimalProductsToHarvest.Add(new TaskItem<FarmAnimal>(location, animal.Value.position,
                        animal.Value));
            }
        }

        private static void CheckAnimalProductsInCoop(GameLocation coop)
        {
            foreach (var pair in coop.objects.Pairs)
                if (Array.BinarySearch(CollectableAnimalProducts, pair.Value.ParentSheetIndex) >= 0 &&
                    (int)pair.Key.X <= coop.map.DisplayWidth / Game1.tileSize &&
                    (int)pair.Key.Y <= coop.map.DisplayHeight / Game1.tileSize)
                    AnimalProductsToCollect.Add(new TaskItem<SObject>(coop, pair.Key, pair.Value));
        }

        private static void CheckForTruffles(GameLocation farm)
        {
            foreach (var pair in farm.objects.Pairs)
                if (pair.Value.ParentSheetIndex == 430)
                    TrufflesToCollect.Add(new TaskItem<SObject>(farm, pair.Key, pair.Value));
        }

        public override void Clear()
        {
            switch (_id)
            {
                case AnimalsTaskId.UnpettedAnimals:
                    Enabled = _config.UnpettedAnimals;
                    break;

                case AnimalsTaskId.AnimalProducts:
                    Enabled = _config.AnimalProducts.ContainsValue(true);
                    break;
                case AnimalsTaskId.MissingHay:
                    Enabled = _config.MissingHay;
                    break;
                case AnimalsTaskId.PondsNeedingAttention:
                    Enabled = _config.PondsNeedingAttention;
                    break;
                case AnimalsTaskId.PondsWithItems:
                    Enabled = _config.PondsWithItems;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Animal task not implemented");
            }

            if (_id != _who) return;

            UnpettedAnimals.Clear();
            AnimalProductsToHarvest.Clear();
            AnimalProductsToCollect.Clear();
            TrufflesToCollect.Clear();
            MissingHay.Clear();
            PondsNeedingAttention.Clear();
            PondsWithItem.Clear();
        }

        public override List<ReportReturnItem> DetailedInfo()
        {
            List<ReportReturnItem> rpItem = new List<ReportReturnItem> { };

            if (!Enabled) return rpItem;

            var stringBuilder = new StringBuilder();

            switch (_id)
            {
                case AnimalsTaskId.UnpettedAnimals:
                    if (UnpettedAnimals.Count == 0) return rpItem;


                    foreach (var animal in UnpettedAnimals)
                    {
                        rpItem.Add(new ReportReturnItem
                        {
                            Label = animal.Object.displayType + " " + animal.Object.displayName + I18n.Tasks_At() + animal.Location.name + " (" + animal.Object.getTileX().ToString() + ", " + animal.Object.getTileY().ToString() + ")",
                            WarpTo = new StardewWebWarp { Location = animal.Location.Name, X = animal.Object.getTileX(), Y = animal.Object.getTileY() }
                        });

                    }
                    break;
                case AnimalsTaskId.PondsNeedingAttention:
                    if (PondsNeedingAttention.Count == 0) return rpItem;

                    foreach (FishPond fpond in PondsNeedingAttention)
                    {
                        rpItem.Add(new ReportReturnItem
                        {
                            Label = "Fish Pond" + I18n.Tasks_At() + "Farm (" + fpond.tileX.ToString() + ", " + fpond.tileY.ToString() + ") wants something.",
                            WarpTo = new StardewWebWarp { Location = "Farm", X = fpond.tileX+fpond.tilesWide-1, Y = fpond.tileY+fpond.tilesHigh }
                        });
                    }

                    break;
                case AnimalsTaskId.PondsWithItems:
                    if (PondsWithItem.Count == 0) return rpItem;

                    foreach (FishPond fpond in PondsWithItem)
                    {
                        rpItem.Add(new ReportReturnItem
                        {
                            Label = "Fish Pond" + I18n.Tasks_At() + "Farm (" + fpond.tileX.ToString() + ", " + fpond.tileY.ToString() + ") have items to collect.",
                            WarpTo = new StardewWebWarp { Location = "Farm", X = fpond.tileX + fpond.tilesWide-1, Y = fpond.tileY + fpond.tilesHigh }
                        });
                    }

                    break;
                case AnimalsTaskId.AnimalProducts:
                    if (AnimalProductsToCollect.Count + TrufflesToCollect.Count + AnimalProductsToHarvest.Count == 0)
                        return rpItem;


                    foreach (var animal in AnimalProductsToHarvest)
                    {
                        var currentProduce = animal.Object.currentProduce.Value;
                        if (!_config.ProductFromAnimal(currentProduce)) continue;
                        StardewObject sd = Library.GetObject(currentProduce);

                        rpItem.Add(new ReportReturnItem
                        {
                            Label = $"{animal.Object.displayType} {animal.Object.displayName}" + I18n.Tasks_Animal_Has() + $"{sd.DisplayName}" + I18n.Tasks_At() + $"{ animal.Location.Name} ({animal.Object.getTileX()}, {animal.Object.getTileY()})",
                            WarpTo = new StardewWebWarp { Location = animal.Location.Name, X = animal.Object.getTileX(), Y = animal.Object.getTileY() }
                        });
                    }

                    foreach (var product in AnimalProductsToCollect)
                    {
                        if (!_config.ProductToCollect(product.Object.ParentSheetIndex)) continue;

                        rpItem.Add(new ReportReturnItem
                        {
                            Label = $"{product.Object.DisplayName}" + I18n.Tasks_At() + $"{product.Location.Name} ({product.Position.X}, {product.Position.Y})",
                            WarpTo = new StardewWebWarp { Location = product.Location.Name, X = (int)product.Position.X, Y = (int)product.Position.Y }
                        });
                    }

                    if (!_config.AnimalProducts["Truffle"]) break;
                    foreach (var product in TrufflesToCollect)
                    {

                        rpItem.Add(new ReportReturnItem
                        {
                            Label = $"{product.Object.DisplayName}" + I18n.Tasks_At() + $"{ product.Location.Name} ({product.Position.X}, {product.Position.Y})",
                            WarpTo = new StardewWebWarp { Location = product.Location.Name, X = (int)product.Position.X, Y = (int)product.Position.Y }
                        });
                    }
                    break;

                case AnimalsTaskId.MissingHay:
                    if (MissingHay.Count == 0) return rpItem;

                    //stringBuilder.Append("Feedbenches not full of hay:^");

                    foreach (var tuple in MissingHay)
                    {
                        var s = tuple.Item2 == 1 ? I18n.Tasks_Animal_MissingHay() : I18n.Tasks_Animal_MissingHays();

                        rpItem.Add(new ReportReturnItem
                        {
                            Label = $"{tuple.Item2}" + s + $"{tuple.Item1.indoors.Value.name} ({tuple.Item1.tileX}, {tuple.Item1.tileY})",
                            WarpTo = new StardewWebWarp
                            {
                                Location = tuple.Item1.indoors.Value.NameOrUniqueName,
                                X = (int)tuple.Item1.tileX,
                                Y = (int)tuple.Item1.tileY
                            }
                        });
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Animal task not implemented");
            }

            return rpItem;
        }

        public override List<ReportReturnItem> GeneralInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return null;

            var count = 0;

            UpdateList();

            switch (_id)
            {
                case AnimalsTaskId.UnpettedAnimals:
                    if (UnpettedAnimals.Count > 0)
                    {
                        prItem.Add(new ReportReturnItem() { Label = I18n.Tasks_Animal_NotPetted(), Details = UnpettedAnimals.Count.ToString() });
                    }
                    break;
                case AnimalsTaskId.PondsNeedingAttention:
                    if (PondsNeedingAttention.Count > 0)
                    {
                        prItem.Add(new ReportReturnItem() { Label ="Ponds needing attention: ", Details = PondsNeedingAttention.Count.ToString() });
                    }
                    break;
                case AnimalsTaskId.PondsWithItems:
                    if (PondsWithItem.Count > 0)
                    {
                        prItem.Add(new ReportReturnItem() { Label = "Ponds with items to collect: ", Details = PondsWithItem.Count.ToString() });
                    }
                    break;
                case AnimalsTaskId.AnimalProducts:
                    if (_config.AnimalProducts["Truffle"])
                        count = TrufflesToCollect.Count;
                    count += AnimalProductsToCollect.Count(p => _config.ProductToCollect(p.Object.ParentSheetIndex));
                    count += AnimalProductsToHarvest.Count(p => _config.ProductFromAnimal(p.Object.currentProduce.Value));
                    if (count > 0)
                    {
                        prItem.Add(new ReportReturnItem() { Label = I18n.Tasks_Animal_Uncollected(), Details = count.ToString() });
                    }
                    break;

                case AnimalsTaskId.MissingHay:
                    count = MissingHay.Sum(t => t.Item2);
                    if (count > 0)
                    {
                        prItem.Add(new ReportReturnItem() { Label = I18n.Tasks_Animal_EmptyHay(), Details = count.ToString() });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown animal task");
            }

            return prItem;
        }

        internal override void FirstScan()
        {
            if (_who == AnimalsTaskId.None)
                _who = _id;
            else if (_who != _id)
                return;

            if (ObjectsNames.Count == 0)
                PopulateObjectsNames();

            _farm = Game1.locations.First(l => l is Farm) as Farm;

            // Checking animals left outside
            CheckAnimals(_farm);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var building in _farm.buildings)
            {
                if (building.isUnderConstruction()) continue;

                switch (building.indoors.Value)
                {
                    case AnimalHouse animalHouse:
                        // Check animals
                        CheckAnimals(animalHouse);

                        // Check for object in Coop
                        if (building is Coop)
                            CheckAnimalProductsInCoop(animalHouse);

                        // Check for hay
                        var count = animalHouse.numberOfObjectsWithName("Hay");
                        var animalLimit = animalHouse.animalLimit.Value;
                        if (count < animalLimit)
                            MissingHay.Add(new Tuple<Building, int>(building, animalLimit - count));
                        break;

                    case SlimeHutch slimeHutch:
                        // Check slime balls
                        foreach (var pair in building.indoors.Value.objects.Pairs)
                            if (pair.Value.ParentSheetIndex >= 56 && pair.Value.ParentSheetIndex <= 61)
                                AnimalProductsToCollect.Add(new TaskItem<SObject>(slimeHutch, pair.Key, pair.Value));
                        break;
                    default:
                        break;
                }
                if (building is FishPond fishpond)
                {
                    if (fishpond.HasUnresolvedNeeds())
                    {
                        PondsNeedingAttention.Add(fishpond);
                    }
                    if (fishpond.output.Value != null)
                    {
                        PondsWithItem.Add(fishpond);
                    }
                }
            }

            CheckForTruffles(_farm);
        }
    }
}
