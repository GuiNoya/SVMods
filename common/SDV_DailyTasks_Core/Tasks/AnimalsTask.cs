using DailyTasksReport.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Linq;

using DailyTasksReport.TaskEngines;

namespace DailyTasksReport.Tasks
{
    public class AnimalsTask : Task
    {

        internal AnimalsTask(TaskReportConfig config, AnimalsTaskId id)
        {
            _config = config;
            _Engine = new AnimalTaskEngine(config, id);

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
            usedLines = 0;
            skipNextPage = false;

            if (!Enabled) return "";

            usedLines = _Engine.DetailedInfo().Count();
            if (usedLines == 0) return "";

            string sHeader = (AnimalsTaskId)_Engine.TaskId switch
            {
                AnimalsTaskId.PondsWithItems => I18n.Tasks_Ponds_Collect(),
                AnimalsTaskId.PondsNeedingAttention => I18n.Tasks_Ponds_Attention(),
                AnimalsTaskId.MissingHay => I18n.Tasks_Animal_MissingHay(),
                AnimalsTaskId.AnimalProducts => I18n.Tasks_Animal_Uncollected(),
                AnimalsTaskId.UnpettedAnimals => I18n.Tasks_Animal_NotPetted(),
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
            if ((AnimalsTaskId)_Engine.TaskId != AnimalTaskEngine._who || !(Game1.currentLocation is Farm) && !(Game1.currentLocation is AnimalHouse)) return;

            // Truffles
            if (_config.DrawBubbleTruffles && Game1.currentLocation is Farm)
            {
                var x = Game1.viewport.X / Game1.tileSize;
                var xLimit = (Game1.viewport.X + Game1.viewport.Width) / Game1.tileSize;
                var yStart = Game1.viewport.Y / Game1.tileSize;
                var yLimit = (Game1.viewport.Y + Game1.viewport.Height) / Game1.tileSize + 1;
                for (; x <= xLimit; ++x)
                    for (var y = yStart; y <= yLimit; ++y)
                    {
                        if (!Game1.currentLocation.objects.TryGetValue(new Vector2(x, y), out var o)) continue;

                        var v = new Vector2(o.TileLocation.X * Game1.tileSize - Game1.viewport.X + Game1.tileSize / 8f,
                            o.TileLocation.Y * Game1.tileSize - Game1.viewport.Y - Game1.tileSize * 2 / 4f);
                        if (o.name == "Truffle")
                            DrawBubble(Game1.spriteBatch, Game1.objectSpriteSheet, new Rectangle(352, 273, 14, 14), v);
                    }
            }

            // Animals

            var animalDict = (Game1.currentLocation as Farm)?.animals ??
                             (Game1.currentLocation as AnimalHouse)?.animals;

            if (animalDict == null) return;

            foreach (var animal in animalDict.Pairs)
            {
                if (animal.Value.isEmoting) continue;

                var currentProduce = animal.Value.currentProduce.Value;

                var needsPet = _config.DrawBubbleUnpettedAnimals && !animal.Value.wasPet.Value;
                var hasProduct = currentProduce != 430 &&
                                 currentProduce > 0 &&
                                 _config.DrawBubbleAnimalsWithProduce;

                var v = new Vector2(animal.Value.getStandingX() - Game1.viewport.X,
                    animal.Value.getStandingY() - Game1.viewport.Y);
                if (animal.Value.home is Coop)
                {
                    v.X -= Game1.tileSize * 0.3f;
                    v.Y -= Game1.tileSize * 6 / 4f;
                }
                else
                {
                    v.X -= Game1.tileSize * 0.2f;
                    v.Y -= Game1.tileSize * 2f;
                }

                if (needsPet)
                {
                    if (hasProduct)
                    {
                        DrawBubble2Icons(b, Game1.mouseCursors, new Rectangle(117, 7, 9, 8),
                            Game1.objectSpriteSheet,
                            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet,
                                currentProduce, 16, 16),
                            v);
                        continue;
                    }
                    DrawBubble(b, Game1.mouseCursors, new Rectangle(117, 7, 9, 8), v);
                }
                else if (hasProduct)
                {
                    DrawBubble(b, Game1.objectSpriteSheet,
                        Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, currentProduce,
                            16, 16),
                        v);
                }
            }

            // Animal Houses

            if (!(Game1.currentLocation is Farm farm)) return;

            foreach (var building in farm.buildings)
                if (building.indoors.Value is AnimalHouse animalHouse)
                {
                    var anyHayMissing = _config.DrawBubbleBuildingsMissingHay &&
                                        animalHouse.numberOfObjectsWithName("Hay") < animalHouse.animalLimit.Value;
                    var anyProduce = _config.DrawBubbleBuildingsWithProduce && building is Coop &&
                                     animalHouse.objects.Values.Any(o =>
                                         Array.BinarySearch(AnimalTaskEngine.CollectableAnimalProducts, o.ParentSheetIndex) >= 0);

                    var v = new Vector2(building.tileX.Value * Game1.tileSize - Game1.viewport.X + Game1.tileSize * 1.1f,
                        building.tileY.Value * Game1.tileSize - Game1.viewport.Y + Game1.tileSize / 2);

                    if (building is Barn)
                        v.Y += Game1.tileSize / 2f;

                    if (anyHayMissing)
                    {
                        if (anyProduce)
                        {
                            DrawBubble2Icons(b, Game1.mouseCursors, new Rectangle(32, 0, 10, 10),
                                Game1.objectSpriteSheet, new Rectangle(160, 112, 16, 16), v);
                            continue;
                        }
                        DrawBubble(b, Game1.objectSpriteSheet, new Rectangle(160, 112, 16, 16), v);
                    }
                    else if (anyProduce)
                    {
                        DrawBubble(b, Game1.mouseCursors, new Rectangle(32, 0, 10, 10), v);
                    }
                }
        }

    }

}