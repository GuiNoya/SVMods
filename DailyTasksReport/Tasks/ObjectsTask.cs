using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DailyTasksReport.UI;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace DailyTasksReport.Tasks
{
    public class ObjectsTask : Task
    {
        private static ModConfig _config;
        private readonly ObjectsTaskId _id;
        private bool _anyObject;

        private static readonly Dictionary<GameLocation, List<Vector2>> Machines =
            new Dictionary<GameLocation, List<Vector2>>();
        private static readonly Dictionary<GameLocation, List<Vector2>> CrabPots =
            new Dictionary<GameLocation, List<Vector2>>();
        private static ObjectsTaskId _who = ObjectsTaskId.None;

        internal ObjectsTask(ModConfig config, ObjectsTaskId id)
        {
            _config = config;
            _id = id;

            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SettingsMenu_ReportConfigChanged(object sender, SettingsChangedEventArgs e)
        {
            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    Enabled = _config.UncollectedCrabpots;
                    break;
                case ObjectsTaskId.NotBaitedCrabpots:
                    Enabled = _config.NotBaitedCrabpots && !Game1.player.professions.Contains(11);
                    break;
                case ObjectsTaskId.UncollectedMachines:
                    Enabled = _config.Machines.ContainsValue(true) || _config.Cask > 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }
        }

        public override void FirstScan()
        {
            if (_who == ObjectsTaskId.None)
                _who = _id;
            else if (_who != _id)
                return;

            foreach (var location in Game1.locations)
            {
                if (!Machines.ContainsKey(location))
                    Machines[location] = new List<Vector2>();
                if (location.waterTiles != null && !CrabPots.ContainsKey(location))
                    CrabPots[location] = new List<Vector2>();

                foreach (var @object in location.objects)
                    if (_config.Machines.ContainsKey(@object.Value.name) || @object.Value is Cask)
                        Machines[location].Add(@object.Key);
                    else if (@object.Value is CrabPot)
                        CrabPots[location].Add(@object.Key);

                if (!(location is BuildableGameLocation farm)) continue;

                foreach (var building in farm.buildings)
                {
                    if (building.indoors == null) continue;

                    var indoors = building.indoors;
                    if (!Machines.ContainsKey(indoors))
                        Machines[indoors] = new List<Vector2>();

                    foreach (var @object in indoors.objects)
                        if (_config.Machines.ContainsKey(@object.Value.name))
                            Machines[indoors].Add(@object.Key);
                }
            }
        }

        private static void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            Game1.currentLocation.objects.CollectionChanged -= ObjectChangedEvent;
        }

        public override void OnDayStarted()
        {
            LocationEvents.CurrentLocationChanged -= LocationEvents_CurrentLocationChanged;
            base.OnDayStarted();
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
        }

        private static void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (e.PriorLocation != null)
                e.PriorLocation.objects.CollectionChanged -= ObjectChangedEvent;
            if (e.NewLocation != null)
                e.NewLocation.objects.CollectionChanged += ObjectChangedEvent;
        }

        private static void ObjectChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Vector2 pos;
            var loc = Game1.currentLocation;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    pos = (Vector2) e.NewItems[0];
                    var obj = loc.objects[pos];
                    if (obj == null) return;
                    if (_config.Machines.ContainsKey(obj.name) || obj is Cask)
                        Machines[loc].Add(pos);
                    else if (obj is CrabPot)
                        CrabPots[loc].Add(pos);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    pos = (Vector2) e.OldItems[0];
                    Machines[loc].Remove(pos);
                    if (CrabPots.TryGetValue(loc, out var list))
                        list.Remove(pos);
                    break;
            }
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;
            _anyObject = false;

            if (!Enabled) return "";

            int count;
            usedLines = 1;

            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    count = (from pair in CrabPots
                        from pos in pair.Value
                        where (pair.Key.objects[pos] as CrabPot)?.heldObject != null
                        select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        return $"Crabpots ready to collect: {count}^";
                    }
                    break;
                case ObjectsTaskId.NotBaitedCrabpots:
                    if (Game1.player.professions.Contains(11)) break;
                    count = (from pair in CrabPots
                        from pos in pair.Value
                        where (pair.Key.objects[pos] as CrabPot)?.bait == null
                        select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        return $"Crabpots not baited: {count}^";
                    }
                    break;
                case ObjectsTaskId.UncollectedMachines:
                    count = (from pair in Machines
                        from pos in pair.Value
                        where MachineReady(pair.Key.objects[pos])
                        select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        return $"Uncollected machines: {count}^";
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

            usedLines = 0;
            return "";
        }

        private static bool MachineReady(Object o)
        {
            return o != null && _config.Machines.TryGetValue(o.name, out var enabled) && enabled && o.readyForHarvest ||
                   o is Cask cask && cask.heldObject?.quality >= _config.Cask;
        }

        public override string DetailedInfo(out int usedLines, out bool skipNextPage)
        {
            skipNextPage = false;
            usedLines = 0;

            if (!Enabled || !_anyObject) return "";

            var stringBuilder = new StringBuilder();

            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    stringBuilder.Append("Uncollected crabpots:^");
                    usedLines++;
                    EchoForCrabpots(ref stringBuilder, ref usedLines, c => c.heldObject != null);
                    break;
                case ObjectsTaskId.NotBaitedCrabpots:
                    stringBuilder.Append("Crabpots still not baited:^");
                    usedLines++;
                    EchoForCrabpots(ref stringBuilder, ref usedLines, c => c.bait == null);
                    break;
                case ObjectsTaskId.UncollectedMachines:
                    stringBuilder.Append("Machines ready to collect:^");
                    usedLines++;
                    EchoForMachines(ref stringBuilder, ref usedLines);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

            return stringBuilder.ToString();
        }

        private static void EchoForCrabpots(ref StringBuilder stringBuilder, ref int usedLines,
            Predicate<CrabPot> predicate)
        {
            foreach (var location in CrabPots)
            foreach (var position in location.Value)
            {
                if (!(location.Key.objects[position] is CrabPot cp) || !predicate.Invoke(cp)) continue;
                stringBuilder.Append($"{location.Key.name} ({position.X}, {position.Y})^");
                usedLines++;
            }
        }

        private static void EchoForMachines(ref StringBuilder stringBuilder, ref int usedLines)
        {
            foreach (var location in Machines)
            foreach (var position in location.Value)
            {
                var machine = location.Key.objects[position];
                if (!MachineReady(machine)) continue;
                var quality = "";
                if (machine is Cask cask)
                    quality = cask.heldObject.quality == 1 ? "Silver "
                            : cask.heldObject.quality == 2 ? "Gold "
                            : cask.heldObject.quality == 4 ? "Iridium "
                            : "";
                stringBuilder.Append(
                    $"{machine.name} with {quality}{machine.heldObject.name} at {location.Key.name} ({position.X}, {position.Y})^");
                usedLines++;
            }
        }

        public override void Clear()
        {
            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    Enabled = _config.UncollectedCrabpots;
                    break;
                case ObjectsTaskId.NotBaitedCrabpots:
                    Enabled = _config.NotBaitedCrabpots && !Game1.player.professions.Contains(11);
                    break;
                case ObjectsTaskId.UncollectedMachines:
                    Enabled = _config.Machines.ContainsValue(true) || _config.Cask > 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

            if (_who != _id) return;

            foreach (var keyValuePair in Machines)
                keyValuePair.Value.Clear();
            foreach (var keyValuePair in CrabPots)
                keyValuePair.Value.Clear();
        }
    }

    public enum ObjectsTaskId
    {
        None = -1,
        UncollectedCrabpots = 0,
        NotBaitedCrabpots = 1,
        UncollectedMachines = 2
    }
}