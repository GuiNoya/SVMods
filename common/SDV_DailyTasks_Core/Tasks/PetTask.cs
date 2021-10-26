using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using DailyTasksReport.TaskEngines;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Characters;
using DailyTasksReport.UI;

namespace DailyTasksReport.Tasks
{
    public class PetTask : Task
    {
        private PetTaskEngine _PetEngine;
        internal PetTask(TaskReportConfig config)
        {
            _config = config;
            _PetEngine= new PetTaskEngine(config);
            _Engine = _PetEngine;
            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;


            if (!Enabled || _Engine.GeneralInfo().Count() == 0)
                return "";

            _Engine.UpdateList();
            usedLines = _Engine.GeneralInfo().Count();
            return string.Join("^", _Engine.GeneralInfo()) + "^";
        }

        public override string DetailedInfo(out int usedLines, out bool skipNextPage)
        {
            usedLines = 0;
            skipNextPage = true;
            return "";
        }
        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
           _Engine.SetEnabled();
        }
        public override void Draw(SpriteBatch b)
        {

            if (!_config.DrawBubbleUnpettedPet ||_PetEngine._pet == null || _PetEngine._pet.currentLocation != Game1.currentLocation ||
                !(Game1.currentLocation is Farm) && !(Game1.currentLocation is FarmHouse)) return;

              if (_PetEngine._petPetted) return;

            var v = new Vector2(_PetEngine._pet.getStandingX() - Game1.viewport.X - Game1.tileSize * 0.3f,
                _PetEngine._pet.getStandingY() - Game1.viewport.Y - Game1.tileSize * (_PetEngine._pet is Cat ? 1.5f : 1.9f));
            DrawBubble(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(117, 7, 9, 8), v);
        }
    }
}