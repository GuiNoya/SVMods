using System.Text;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;

namespace DailyTasksReport.Tasks
{
    public class PetTask : Task
    {
        private readonly ModConfig _config;
        private bool _petBowlFilled;
        private bool _petFound;
        private bool _petPetted;

        internal PetTask(ModConfig config)
        {
            _config = config;
        }

        private void DoCheck()
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            var location = Game1.locations.Find(l => l is Farm);

            // Bowl
            if (location.getTileIndexAt(54, 7, "Buildings") == 1939)
                _petBowlFilled = true;

            // Pet

            CheckPetted(location);

            if (!_petFound)
                CheckPetted(Game1.locations.Find(l => l is FarmHouse));
        }

        private void CheckPetted(GameLocation location)
        {
            foreach (var npc in location.characters)
            {
                if (!(npc is Pet pet)) continue;

                var wasPettedToday = ModEntry.ReflectionHelper.GetPrivateValue<bool>(pet, "wasPetToday");
                if (wasPettedToday)
                    _petPetted = true;
                _petFound = true;
                return;
            }
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;

            if (!_config.UnpettedPet && !_config.UnfilledPetBowl)
                return "";

            Clear();
            DoCheck();

            if (!_petFound)
                return "";

            var stringBuilder = new StringBuilder();

            if (_config.UnpettedPet && !_petPetted)
            {
                stringBuilder.Append("You did not pet your pet today.^");
                usedLines++;
            }
            if (_config.UnfilledPetBowl && !_petBowlFilled)
            {
                stringBuilder.Append("You did not fill your pet's bowl.^");
                usedLines++;
            }
            return stringBuilder.ToString();
        }

        public override string DetailedInfo()
        {
            return "";
        }

        public override void Clear()
        {
            _petFound = false;
            _petBowlFilled = false;
            _petPetted = false;
        }
    }
}