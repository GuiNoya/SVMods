using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewValley.Locations;

namespace DailyTasksReport.Tasks
{
    public class FarmCaveTask : Task
    {
        private static readonly int[] Fruits = {296, 396, 406, 410, 613, 634, 635, 636, 637, 638};

        private readonly ModConfig _config;
        private string _farmCaveItemName;

        private readonly Dictionary<string, int> _objectsList = new Dictionary<string, int>();

        internal FarmCaveTask(ModConfig config)
        {
            _config = config;
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;
            if (!_config.FarmCave) return "";

            _farmCaveItemName = Game1.player.caveChoice == 1 ? "Fruits" : "Mushrooms";

            Clear();
            DoCheck();

            if (_objectsList.Count == 0) return "";

            usedLines++;
            return $"{_farmCaveItemName} in the farm cave: {_objectsList.Sum(o => o.Value)}^";
        }

        public override string DetailedInfo()
        {
            if (!_config.FarmCave || _objectsList.Count == 0) return "";

            var stringBuilder = new StringBuilder();
            var linesCount = 0;

            stringBuilder.Append($"{_farmCaveItemName} in the farm cave:^");
            linesCount++;

            foreach (var pair in _objectsList)
            {
                var name = Pluralize(pair.Key, pair.Value);
                stringBuilder.Append($"{pair.Value} {name}^");
                linesCount++;
            }

            NextPage(ref stringBuilder, ref linesCount);

            return stringBuilder.ToString();
        }

        public override void Clear()
        {
            _objectsList.Clear();
        }

        private void DoCheck()
        {
            var farmCave = Game1.locations.Find(l => l is FarmCave);

            foreach (var pair in farmCave.objects)
                if (pair.Value.parentSheetIndex == 128 && pair.Value.heldObject != null && pair.Value.readyForHarvest)
                    if (_objectsList.ContainsKey(pair.Value.heldObject.name))
                        _objectsList[pair.Value.heldObject.name] += 1;
                    else
                        _objectsList[pair.Value.heldObject.name] = 1;
                else if (Array.BinarySearch(Fruits, pair.Value.parentSheetIndex) >= 0)
                    if (_objectsList.ContainsKey(pair.Value.name))
                        _objectsList[pair.Value.name] += 1;
                    else
                        _objectsList[pair.Value.name] = 1;
        }

        private static string Pluralize(string name, int number)
        {
            if (number < 2)
                return name;
            if (name.EndsWith("y"))
                return name.Substring(0, name.Length - 1) + "ies";
            if (name == "Peach")
                return name + "es";
            return name + "s";
        }
    }
}