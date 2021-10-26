using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTasksReport
{

    [Serializable]
 public   class TaskReportConfig
    {
        public bool DrawBubbleUnpettedAnimals { get; set; }
        public bool DrawBubbleTruffles { get; set; }
        public bool DrawBubbleAnimalsWithProduce { get; set; }
        public bool DrawBubbleBuildingsMissingHay { get; set; }
        public bool DrawBubbleBuildingsWithProduce { get; set; }
        public bool DrawBubbleDeadCrops { get; set; }
        public bool DrawBubbleUnharvestedCrops { get; set; }
        public bool DrawBubbleUnwateredCrops { get; set; }
        public bool DrawBubbleCask { get; set; }
        public bool DrawBubbleCrabpotsNotBaited { get; set; }
        public bool DrawBubbleUnpettedPet { get; set; }
        public bool SkipFlowersInHarvest { get; set; } = true;
        public bool FlowerReportLastDay { get; set; } = true;
        public bool ShowDetailedInfo { get; set; } = true;
        public bool Birthdays { get; set; } = true;
        public bool NewRecipeOnTv { get; set; } = true;
        public bool TravelingMerchant { get; set; } = true;
        public bool UnpettedAnimals { get; set; } = true;
        public bool UnwateredCrops { get; set; } = true;
        public bool UnharvestedCrops { get; set; } = true;
        public bool DeadCrops { get; set; } = true;
        public int FruitTrees { get; set; } = 3;
        public bool MissingHay { get; set; } = true;
        public bool UnpettedPet { get; set; } = true;
        public bool UnfilledPetBowl { get; set; } = true;
        public bool FarmCave { get; set; } = true;
        public bool UncollectedCrabpots { get; set; } = true;
        public bool NotBaitedCrabpots { get; set; } = true;
        public bool BrokenFences { get; set; } = true;
        public bool PondsNeedingAttention { get; set; } = true;
        public bool PondsWithItems { get; set; } = true;
        public int SiloThreshold { get; set; } = 100;
        // <summary> Product quality to check in casks. </summary>
        public int Cask { get; set; } = 3;
        internal bool ProductToCollect(int objectIndex)
        {
            return AnimalProducts[LookupProductToCollect[objectIndex]];
        }
        internal bool ProductFromAnimal(int produceIndex)
        {
            if (LookupProductFromAnimal.ContainsKey(produceIndex))
            {
                return produceIndex > 0 && AnimalProducts[LookupProductFromAnimal[produceIndex]];
            }
            else
            {
                return false;
            }
        }


        private static readonly Dictionary<int, string> LookupProductFromAnimal = new Dictionary<int, string>
        {
            {184, "Cow milk"},
            {186, "Cow milk"},
            {436, "Goat milk"},
            {438, "Goat milk"},
            {440, "Sheep wool"}
        };
        private static readonly Dictionary<int, string> LookupProductToCollect = new Dictionary<int, string>
        {
            {174, "Chicken egg"},
            {176, "Chicken egg"},
            {180, "Chicken egg"},
            {182, "Chicken egg"},
            {107, "Dinosaur egg"},
            {442, "Duck egg"},
            {444, "Duck feather"},
            {440, "Rabbit's wool"},
            {446, "Rabbit's foot"},
            {430, "Truffle"},
            {56, "Slime ball"},
            {57, "Slime ball"},
            {58, "Slime ball"},
            {59, "Slime ball"},
            {60, "Slime ball"},
            {61, "Slime ball"},
            {289, "Ostrich Egg" }
        };
        public Dictionary<string, bool> AnimalProducts { get; set; } = new Dictionary<string, bool>
        {
            {"Cow milk", true},
            {"Goat milk", true},
            {"Sheep wool", true},
            {"Chicken egg", true},
            {"Dinosaur egg", true},
            {"Duck egg", true},
            {"Duck feather", true},
            {"Rabbit's wool", true},
            {"Rabbit's foot", true},
            {"Truffle", true},
            {"Slime ball", true},
            {"Ostrich Egg",true }
        };
        public Dictionary<string, bool> Machines { get; set; } = new Dictionary<string, bool>
        {
            {"Bee House", true},
            {"Charcoal Kiln", true},
            {"Cheese Press", true},
            {"Crystalarium", true},
            {"Furnace", true},
            {"Keg", true},
            {"Lightning Rod", true},
            {"Loom", true},
            {"Mayonnaise Machine", true},
            {"Oil Maker", true},
            {"Preserves Jar", true},
            {"Recycling Machine", true},
            {"Seed Maker", true},
            {"Slime Egg-Press", true},
            {"Soda Machine", true},
            {"Statue Of Endless Fortune", true},
            {"Statue Of Perfection", true},
            {"Tapper", true},
            {"Worm Bin", true}
        };

    }
}
