using System.Text;
using DailyTasksReport.UI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;

namespace DailyTasksReport.Tasks
{
    public class PetTask : Task
    {
        private readonly ModConfig _config;
        private Pet _pet;
        private bool _petBowlFilled;
        private bool _petPetted;

        internal PetTask(ModConfig config)
        {
            _config = config;

            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }

        private void SettingsMenu_ReportConfigChanged(object sender, SettingsChangedEventArgs e)
        {
            var a = e.OptionChanged == OptionsEnum.UnpettedPet || e.OptionChanged == OptionsEnum.UnfilledPetBowl;
            var b = _config.UnpettedPet || _config.UnfilledPetBowl;
            Enabled = a && b;
        }

        public override void FirstScan()
        {
            var location = Game1.locations.Find(l => l is Farm);

            // Bowl
            if (location.getTileIndexAt(54, 7, "Buildings") == 1939)
                _petBowlFilled = true;

            // Pet

            CheckPetted(location);
            if (_pet == null)
                CheckPetted(Game1.locations.Find(l => l is FarmHouse));

            if (_pet == null || _petBowlFilled && _petPetted)
                Enabled = false;
        }

        private void UpdateInfo()
        {
            var wasPettedToday = ModEntry.ReflectionHelper.GetPrivateValue<bool>(_pet, "wasPetToday");
            if (wasPettedToday)
                _petPetted = true;

            if (!_petBowlFilled)
            {
                var f = Game1.locations.Find(l => l is Farm);
                if (f.getTileIndexAt(54, 7, "Buildings") == 1939)
                    _petBowlFilled = true;
            }

            if (_petBowlFilled && _petPetted)
                Enabled = false;
        }

        private void CheckPetted(GameLocation location)
        {
            foreach (var npc in location.characters)
            {
                if (!(npc is Pet pet)) continue;

                _pet = pet;
                var wasPettedToday = ModEntry.ReflectionHelper.GetPrivateValue<bool>(pet, "wasPetToday");
                if (wasPettedToday)
                    _petPetted = true;
                return;
            }
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;

            if (!Enabled)
                return "";

            UpdateInfo();

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

        public override string DetailedInfo(out int usedLines, out bool skipNextPage)
        {
            usedLines = 0;
            skipNextPage = true;
            return "";
        }

        public override void Clear()
        {
            Enabled = _config.UnpettedPet || _config.UnfilledPetBowl;
            _pet = null;
            _petBowlFilled = false;
            _petPetted = false;
        }
    }
}