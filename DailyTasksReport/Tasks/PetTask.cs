using System.Text;
using DailyTasksReport.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;

namespace DailyTasksReport.Tasks
{
    public class PetTask : Task
    {
        private readonly ModConfig _config;
        private Farm _farm;
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

        protected override void FirstScan()
        {
            _farm = Game1.locations.Find(l => l is Farm) as Farm;

            _pet = _farm?.characters.Find(npc => npc is Pet) as Pet;

            if (_pet == null)
            {
                var location = Game1.locations.Find(l => l is FarmHouse);
                _pet = location.characters.Find(npc => npc is Pet) as Pet;
            }

            Enabled = _pet != null;
        }

        private void UpdateInfo()
        {
            _petPetted = ModEntry.ReflectionHelper.GetPrivateValue<bool>(_pet, "wasPetToday");
            _petBowlFilled = _farm.getTileIndexAt(54, 7, "Buildings") == 1939;

            if (_petBowlFilled && _petPetted)
                Enabled = false;
        }

        public override void Draw(SpriteBatch b)
        {
            if (!_config.DrawBubbleUnpettedPet ||
                !(Game1.currentLocation is Farm) && !(Game1.currentLocation is FarmHouse)) return;

            _petPetted = ModEntry.ReflectionHelper.GetPrivateValue<bool>(_pet, "wasPetToday");
            if (_petPetted) return;

            var v = new Vector2(_pet.getStandingX() - Game1.viewport.X - Game1.tileSize * 0.3f,
                _pet.getStandingY() - Game1.viewport.Y - Game1.tileSize * (_pet is Cat ? 1.5f : 1.9f));
            DrawBubble(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(117, 7, 9, 8), v);
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