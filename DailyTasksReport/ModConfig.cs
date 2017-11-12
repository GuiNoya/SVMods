using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace DailyTasksReport
{
    internal class ModConfig
    {
        /// <summary> The keyboard key to open the report. </summary>
        public SButton OpenReportKey { get; set; } = SButton.Y;
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
        /// <summary> Check or not if the feeding benches are not full. </summary>
        public bool MissingHay { get; set; } = true;
        /// <summary> Check or not if there is something in the farm cave. </summary>
        public bool FarmCave { get; set; } = true;
        /// <summary> Check or not if you have uncollected crabpots. </summary>
        public bool UncollectedCrabpots { get; set; } = true;
        /// <summary> Check or not if you have not baited crabpots. </summary>
        public bool NotBaitedCrabpots { get; set; } = true;
    }
}
