using System;
using System.Collections.Generic;
using System.Text;
using DailyTasksReport.UI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace DailyTasksReport.Tasks
{
    public class ObjectsTask : Task
    {
        private readonly ModConfig _config;
        private bool _checkAnyBigCraftable;

        private readonly List<TaskItem<CrabPot>> _notBaitedCrabpots = new List<TaskItem<CrabPot>>();
        private readonly List<TaskItem<CrabPot>> _uncollectedCrabpots = new List<TaskItem<CrabPot>>();
        private readonly List<TaskItem<Object>> _uncollectedMachines = new List<TaskItem<Object>>();
        

        internal ObjectsTask(ModConfig config)
        {
            _config = config;
            SettingsMenu_ReportConfigChanged(null, null);

            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }

        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
            _checkAnyBigCraftable = _config.Machines.ContainsValue(true);
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;
            if (!_checkAnyBigCraftable && !_config.UncollectedCrabpots && !_config.NotBaitedCrabpots) return "";

            Clear();
            DoCheck();

            var stringBuilder = new StringBuilder();

            if (_config.UncollectedCrabpots && _uncollectedCrabpots.Count > 0)
            {
                stringBuilder.Append($"Crabpots ready to collect: {_uncollectedCrabpots.Count}^");
                usedLines++;
            }
            if (_config.NotBaitedCrabpots && _notBaitedCrabpots.Count > 0)
            {
                stringBuilder.Append($"Crabpots not baited: {_notBaitedCrabpots.Count}^");
                usedLines++;
            }
            if (_checkAnyBigCraftable && _uncollectedMachines.Count > 0)
            {
                stringBuilder.Append($"Uncollected machines: {_uncollectedMachines.Count}^");
                usedLines++;
            }

            return stringBuilder.ToString();
        }

        public override string DetailedInfo()
        {
            var stringBuilder = new StringBuilder();
            var linesCount = 0;

            if (_config.UncollectedCrabpots && _uncollectedCrabpots.Count > 0)
            {
                stringBuilder.Append("Uncollected crabpots:^");
                linesCount++;

                foreach (var item in _uncollectedCrabpots)
                {
                    stringBuilder.Append($"{item.Location.name} ({item.Position.X}, {item.Position.Y})^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_config.NotBaitedCrabpots && _notBaitedCrabpots.Count > 0)
            {
                stringBuilder.Append("Crabpots still not baited:^");
                linesCount++;

                foreach (var item in _notBaitedCrabpots)
                {
                    stringBuilder.Append($"{item.Location.name} ({item.Position.X}, {item.Position.Y})^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            if (_checkAnyBigCraftable && _uncollectedMachines.Count > 0)
            {
                stringBuilder.Append("Machines ready to collect:^");
                linesCount++;

                foreach (var item in _uncollectedMachines)
                {
                    stringBuilder.Append(
                        $"{item.Name} with {item.Object.heldObject.name} at {item.Location.name} ({item.Position.X}, {item.Position.Y})^");
                    linesCount++;
                }

                NextPage(ref stringBuilder, ref linesCount);
            }

            return stringBuilder.ToString();
        }

        public override void Clear()
        {
            _uncollectedCrabpots.Clear();
            _notBaitedCrabpots.Clear();
            _uncollectedMachines.Clear();
        }

        private void DoCheck()
        {
            if (!_checkAnyBigCraftable && !_config.UncollectedCrabpots && !_config.NotBaitedCrabpots) return;

            var haveLuremasterProfession = Game1.player.professions.Contains(11);

            foreach (var location in Game1.locations)
            {
                foreach (var @object in location.objects)
                {
                    if (@object.Value is CrabPot cb)
                    {
                        if (_config.UncollectedCrabpots && cb.heldObject != null)
                            _uncollectedCrabpots.Add(new TaskItem<CrabPot>(location, @object.Key, cb.name, cb));
                        else if (_config.NotBaitedCrabpots && cb.bait == null && !haveLuremasterProfession)
                            _notBaitedCrabpots.Add(new TaskItem<CrabPot>(location, @object.Key, cb.name, cb));
                        continue;
                    }

                    if (_checkAnyBigCraftable && @object.Value.bigCraftable &&
                        _config.Machines.TryGetValue(@object.Value.name, out var enabled) && enabled &&
                        @object.Value.readyForHarvest)
                    {
                        _uncollectedMachines.Add(new TaskItem<Object>(location, @object.Key, @object.Value.name,
                            @object.Value));
                    }
                }

                if (!(location is Farm farm)) continue;

                foreach (var building in farm.buildings)
                {
                    if (building.indoors == null) continue;

                    foreach (var @object in building.indoors.objects)
                        if (_checkAnyBigCraftable && @object.Value.bigCraftable &&
                            _config.Machines.TryGetValue(@object.Value.name, out var enabled) && enabled &&
                            @object.Value.readyForHarvest)
                        {
                            _uncollectedMachines.Add(new TaskItem<Object>(building.indoors, @object.Key,
                                @object.Value.name, @object.Value));
                        }
                }
            }
        }
    }
}