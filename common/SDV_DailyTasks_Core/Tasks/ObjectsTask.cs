using DailyTasksReport.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System.Linq;
using DailyTasksReport.TaskEngines;
using System;

namespace DailyTasksReport.Tasks
{
    public class ObjectsTask : Task
    {
        internal ObjectsTask(TaskReportConfig config, ObjectsTaskId id)
        {
            _config = config;
            _Engine = new ObjectTaskEngine(config, id);
            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;

        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;

            if (!Enabled) return "";

            _Engine.UpdateList();
            usedLines = _Engine.GeneralInfo().Count();
            return string.Join("^", _Engine.GeneralInfo()) + (_Engine.GeneralInfo().Count > 0 ? "^" : "");
        }

        public override string DetailedInfo(out int usedLines, out bool skipNextPage)
        {
            skipNextPage = false;
            usedLines = 0;

            if (!Enabled || _Engine.DetailedInfo().Count() == 0) return "";


            string sHeader = (ObjectsTaskId)_Engine.TaskId switch
            {
                ObjectsTaskId.NotBaitedCrabpots => I18n.Tasks_Object_Crab(),
                ObjectsTaskId.UncollectedCrabpots => I18n.Tasks_Object_CrabPot(),
                ObjectsTaskId.UncollectedMachines => I18n.Tasks_Object_Machine(),
                _ => ""
            };

            return sHeader + ":^" + string.Join("^", _Engine.DetailedInfo()) + "^";
        }

        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
            _Engine.SetEnabled();
        }
        public override void Draw(SpriteBatch b)
        {
            if (ObjectTaskEngine._who != (ObjectsTaskId)_Engine.TaskId) return;

            var x = Game1.viewport.X / Game1.tileSize;
            var xLimit = (Game1.viewport.X + Game1.viewport.Width) / Game1.tileSize;
            var yStart = Game1.viewport.Y / Game1.tileSize;
            var yLimit = (Game1.viewport.Y + Game1.viewport.Height) / Game1.tileSize + 1;
            for (; x <= xLimit; ++x)
                for (var y = yStart; y <= yLimit; ++y)
                {
                    if (!Game1.currentLocation.objects.TryGetValue(new Vector2(x, y), out var o)) continue;

                    var v = new Vector2(o.TileLocation.X * Game1.tileSize - Game1.viewport.X + Game1.tileSize / 8f,
                        o.TileLocation.Y * Game1.tileSize - Game1.viewport.Y - Game1.tileSize * 5 / 4f);

                    var heldObject = o.heldObject.Value;
                    switch (o)
                    {
                        case Cask cask when _config.DrawBubbleCask && heldObject?.Quality > 0 &&
                                            heldObject.Quality >= _config.Cask &&
                                            heldObject.Quality < 4:
                            DrawBubble(b, Game1.objectSpriteSheet,
                                Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet,
                                    heldObject.ParentSheetIndex, 16, 16), v);
                            break;

                        case CrabPot cp when _config.DrawBubbleCrabpotsNotBaited && cp.bait.Value == null && !((ObjectTaskEngine)_Engine)._hasLuremaster:
                            DrawBubble(b, Game1.objectSpriteSheet, new Rectangle(209, 450, 13, 13), v);
                            break;

                        default:
                            break;
                    }
                }
        }
    }

}