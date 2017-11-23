using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DailyTasksReport.Tasks
{
    public class CropsTask : Task
    {
        private readonly ModConfig _config;

        // index 0 = Farm, index 1 = Greenhouse
        private readonly List<TaskItem<Crop>>[] _unwateredCrops = { new List<TaskItem<Crop>>(), new List<TaskItem<Crop>>() };
        private readonly List<TaskItem<Crop>>[] _unharvestedCrops = { new List<TaskItem<Crop>>(), new List<TaskItem<Crop>>() };
        private readonly List<TaskItem<Crop>>[] _deadCrops = { new List<TaskItem<Crop>>(), new List<TaskItem<Crop>>() };
        
        internal CropsTask(ModConfig config)
        {
            _config = config;
            CanDrawBubble = false;

            if (!CanDrawBubble)
                return;
            
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
        }
        
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            Clear();
            Scan(Game1.locations.Find(l => l is Farm));
            Scan(Game1.locations.Find(l => l.name == "Greenhouse"));
        }
        
        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            Scan();
        }

        private void Scan(GameLocation location = null)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (location == null)
            {
                if (!(Game1.currentLocation is Farm) && Game1.currentLocation.name != "Greenhouse")
                    return;
                location = Game1.currentLocation;
            }
            
            var index = location is Farm ? 0 : 1;

            _unwateredCrops[index].Clear();
            _unharvestedCrops[index].Clear();
            _deadCrops[index].Clear();

            foreach (var pair in location.terrainFeatures)
            {
                if (!(pair.Value is HoeDirt dirt) || dirt.crop == null) continue;

                var cropName = Game1.objectInformation[dirt.crop.indexOfHarvest].Split("/".ToCharArray(), 2)[0];

                if (dirt.crop.dead)
                {
                    _deadCrops[index].Add(new TaskItem<Crop>(location, pair.Key, cropName, dirt.crop));
                    continue;
                }

                if (_config.UnwateredCrops && dirt.state == HoeDirt.dry &&
                    (dirt.crop.currentPhase < dirt.crop.phaseDays.Count - 1 || dirt.crop.dayOfCurrentPhase > 0))
                    _unwateredCrops[index].Add(new TaskItem<Crop>(location, pair.Key, cropName, dirt.crop));

                if (_config.UnharvestedCrops &&
                    dirt.crop.currentPhase >= dirt.crop.phaseDays.Count - 1 && dirt.crop.dayOfCurrentPhase <= 0)
                    _unharvestedCrops[index].Add(new TaskItem<Crop>(location, pair.Key, cropName, dirt.crop));
            }
        }
        
        public override string GeneralInfo(out int usedLines)
        {
            var stringBuilder = new StringBuilder();
            usedLines = 0;

            if (!CanDrawBubble)
            {
                Clear();
                if (!_config.UnwateredCrops && !_config.UnharvestedCrops && !_config.DeadCrops)
                    return "";
                Scan(Game1.locations.Find(l => l is Farm));
                Scan(Game1.locations.Find(l => l.name == "Greenhouse"));
            }

            if (_config.UnwateredCrops)
            {
                if (_unwateredCrops[0].Count > 0)
                {
                    stringBuilder.Append($"Farm crops not watered: {_unwateredCrops[0].Count}^");
                    usedLines++;
                }
                if (_unwateredCrops[1].Count > 0)
                {
                    stringBuilder.Append($"Greenhouse crops not watered: {_unwateredCrops[1].Count}^");
                    usedLines++;
                }
            }

            if (_config.UnharvestedCrops)
            {
                if (_unharvestedCrops[0].Count > 0)
                {
                    stringBuilder.Append($"Farm crops ready to harvest: {_unharvestedCrops[0].Count}^");
                    usedLines++;
                }
                if (_unharvestedCrops[1].Count > 0)
                {
                    stringBuilder.Append($"Greenhouse crops ready to harvest: {_unharvestedCrops[1].Count}^");
                    usedLines++;
                }
            }

            if (_config.DeadCrops)
            {
                if (_deadCrops[0].Count > 0)
                {
                    stringBuilder.Append($"Dead crops in the Farm: {_deadCrops[0].Count}^");
                    usedLines++;
                }
                if (_deadCrops[1].Count > 0)
                {
                    stringBuilder.Append($"Dead crops in the Greenhouse: {_deadCrops[1].Count}^");
                    usedLines++;
                }
            }

            return stringBuilder.ToString();
        }

        public override string DetailedInfo()
        {
            var stringBuilder = new StringBuilder();
            var linesCount = 0;

            if (_config.UnwateredCrops && (_unwateredCrops[0].Count > 0 || _unwateredCrops[1].Count > 0))
            {
                stringBuilder.Append("Unwatered crops:^");
                linesCount++;

                EchoForCrops(ref stringBuilder, _unwateredCrops[0], ref linesCount);
                EchoForCrops(ref stringBuilder, _unwateredCrops[1], ref linesCount);

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_config.UnharvestedCrops && (_unharvestedCrops[0].Count > 0 || _unharvestedCrops[1].Count > 0))
            {
                stringBuilder.Append("Ready to harvest crops:^");
                linesCount++;

                EchoForCrops(ref stringBuilder, _unharvestedCrops[0], ref linesCount);
                EchoForCrops(ref stringBuilder, _unharvestedCrops[1], ref linesCount);

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_config.DeadCrops && (_deadCrops[0].Count > 0 || _deadCrops[0].Count > 0))
            {
                stringBuilder.Append("Dead crops:^");
                linesCount++;

                EchoForCrops(ref stringBuilder, _deadCrops[0], ref linesCount);
                EchoForCrops(ref stringBuilder, _deadCrops[1], ref linesCount);

                NextPage(ref stringBuilder, ref linesCount);
            }

            return stringBuilder.ToString();
        }

        private static void EchoForCrops(ref StringBuilder stringBuilder, IEnumerable<TaskItem<Crop>> list, ref int count)
        {
            foreach (var item in list)
            {
                stringBuilder.Append($"{item.Name} at {item.Location.name} ({item.Position.X}, {item.Position.Y})^");
                count++;
            }
        }

        public override void Clear()
        {
            _unwateredCrops[0].Clear();
            _unwateredCrops[1].Clear();
            _unharvestedCrops[0].Clear();
            _unharvestedCrops[1].Clear();
            _deadCrops[0].Clear();
            _deadCrops[1].Clear();
        }
    }
}