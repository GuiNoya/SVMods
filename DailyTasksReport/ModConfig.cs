using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace DailyTasksReport
{
    internal class ModConfig
    {
        /// <summary> The input to open the report. </summary>
        public string OpenReportKey { get; set; } = Keys.Y.ToString();

        /// <summary> The input to open the settings menu. </summary>
        public string OpenSettings { get; set; } = Keys.None.ToString();

        /// <summary> Show detailed info on the next pages. </summary>
        public bool ShowDetailedInfo { get; set; } = true;

        /// <summary> Check or not for unwatered crops in farm and greenhouse. </summary>
        public bool UnwateredCrops { get; set; } = true;

        /// <summary> Check or not for unharvested crops in farm and greenhouse. </summary>
        public bool UnharvestedCrops { get; set; } = true;

        /// <summary> Check or not if there are dead crops. </summary>
        public bool DeadCrops { get; set; } = true;

        /// <summary> Check or not for if you petted your pet. </summary>
        public bool UnpettedPet { get; set; } = true;

        /// <summary> Check or not if your pet's bowl is filled with water. </summary>
        public bool UnfilledPetBowl { get; set; } = true;

        /// <summary> Check or not for unpetted animals in your farm. </summary>
        public bool UnpettedAnimals { get; set; } = true;

        /// <summary> Check or not if that are animal products ready to collect. </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
        public Dictionary<string, bool> AnimalProducts { get; set; } = new Dictionary<string, bool>
        {
            {"Cow milk", true},
            {"Goat milk", true},
            {"Sheep wool", true},
            {"Chicken egg", true},
            {"Dinosaur egg", true},
            {"Duck egg", true},
            {"Duck feather", true},
            {"Rabit's wool", true},
            {"Rabit's foot", true},
            {"Truffle", true},
            {"Slime ball", true}
        };

        /// <summary> Check or not if the feeding benches are not full. </summary>
        public bool MissingHay { get; set; } = true;

        /// <summary> Check or not if there is something in the farm cave. </summary>
        public bool FarmCave { get; set; } = true;

        /// <summary> Check or not if you have uncollected crabpots. </summary>
        public bool UncollectedCrabpots { get; set; } = true;

        /// <summary> Check or not if you have not baited crabpots. </summary>
        public bool NotBaitedCrabpots { get; set; } = true;

        /// <summary> Check or not if there are BigCraftables (tapper, machines, ...) ready to collect. </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
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

        // <summary> Product quality to check in casks. </summary>
        public int Cask { get; set; } = 4;
    }
}