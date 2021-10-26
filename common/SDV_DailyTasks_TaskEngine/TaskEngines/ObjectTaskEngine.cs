using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Locations;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DailyTasksReport.Tasks;

#if StandAlone

using DailyTasksReport.UI;

#elif StardewWeb

using StardewWeb;

#endif

namespace DailyTasksReport.TaskEngines
{
    class ObjectTaskEngine : TaskEngine
    {
        public readonly ObjectsTaskId _id;
        private bool _anyObject;

        public static ObjectsTaskId _who = ObjectsTaskId.None;
        public bool _hasLuremaster;

        private static readonly Dictionary<GameLocation, List<Vector2>> Machines =
            new Dictionary<GameLocation, List<Vector2>>();

        private static readonly Dictionary<GameLocation, List<Vector2>> CrabPots =
            new Dictionary<GameLocation, List<Vector2>>();

        internal ObjectTaskEngine(TaskReportConfig config, ObjectsTaskId id)
        {
            _config = config;
            _id = id;
            TaskClass = "Objects";
            TaskSubClass = id.ToString();
            TaskId = (int)id;

            DailyTaskHelper.Helper.Events.World.ObjectListChanged += World_ObjectListChanged;

            SetEnabled();
        }

        public override void Clear()
        {
            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    Enabled = _config.UncollectedCrabpots;
                    break;

                case ObjectsTaskId.NotBaitedCrabpots:
                    _hasLuremaster = Game1.player.professions.Contains(11);
                    Enabled = _config.NotBaitedCrabpots && !_hasLuremaster;
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


        public override List<ReportReturnItem> DetailedInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled || !_anyObject) return prItem;

            var stringBuilder = new StringBuilder();

            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    prItem.AddRange(EchoForCrabpots(c => c.heldObject.Value != null));
                    break;

                case ObjectsTaskId.NotBaitedCrabpots:
                    prItem.AddRange(EchoForCrabpots(c => c.bait.Value == null));
                    break;

                case ObjectsTaskId.UncollectedMachines:
                    prItem.AddRange(EchoForMachines(_helper));
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

            return prItem;
        }


        public override List<ReportReturnItem> GeneralInfo()
        {
            _anyObject = false;
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;

            int count;

            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    count = (from pair in CrabPots
                             from pos in pair.Value
                             where (pair.Key.objects[pos] as CrabPot)?.heldObject.Value != null
                             select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Object_Crab(), Details = count.ToString() });
                    }
                    break;

                case ObjectsTaskId.NotBaitedCrabpots:
                    if (_hasLuremaster) break;
                    count = (from pair in CrabPots
                             from pos in pair.Value
                             where (pair.Key.objects[pos] as CrabPot)?.bait.Value == null
                             select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Object_CrabPot(), Details = count.ToString() });
                    }
                    break;

                case ObjectsTaskId.UncollectedMachines:
                    SObject machine;
                    count = (from pair in Machines
                             from pos in pair.Value
                             where pair.Key.objects.TryGetValue(pos, out machine) && MachineReady(machine)
                             select 1).Count();
                    if (count > 0)
                    {
                        _anyObject = true;
                        prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Object_Machine(), Details = count.ToString() });
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

            return prItem;
        }
        public override void SetEnabled()
        {
            switch (_id)
            {
                case ObjectsTaskId.UncollectedCrabpots:
                    Enabled = _config.UncollectedCrabpots;
                    break;

                case ObjectsTaskId.NotBaitedCrabpots:
                    if (Game1.hasLoadedGame && Game1.player.professions != null) _hasLuremaster = Game1.player.professions.Contains(11);
                    Enabled = _config.NotBaitedCrabpots && !_hasLuremaster;
                    break;

                case ObjectsTaskId.UncollectedMachines:
                    Enabled = _config.Machines.ContainsValue(true) || _config.Cask > 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Object type not implemented");
            }

        }
    
        private bool LocationHasWaterTiles(GameLocation gl)
        {
            //
            //  added for 1.5.5 compatability
            //  GameLocation.waterTiles changes Type between versions
            //
            for (int i = 0; i < gl.map.Layers[0].LayerWidth; i++)
            {
                for (int j = 0; j < gl.map.Layers[0].LayerHeight; j++)
                {
                    string text = gl.doesTileHaveProperty(i, j, "Water", "Back");
                    if (text != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        internal override void FirstScan()
        {
            if (_who == ObjectsTaskId.None)
                _who = _id;
            else if (_who != _id)
                return;

            foreach (GameLocation location in Game1.locations)
            {
                if (!Machines.ContainsKey(location))
                    Machines[location] = new List<Vector2>();


                if (LocationHasWaterTiles(location) && !CrabPots.ContainsKey(location))
                    CrabPots[location] = new List<Vector2>();

                foreach (var @object in location.Objects.Pairs)
                    if (_config.Machines.ContainsKey(@object.Value.name) || @object.Value is Cask)
                        Machines[location].Add(@object.Key);
                    else if (@object.Value is CrabPot)
                        CrabPots[location].Add(@object.Key);

                if (!(location is BuildableGameLocation farm)) continue;

                foreach (var building in farm.buildings)
                {
                    var indoors = building.indoors.Value;
                    if (indoors == null) continue;

                    if (!Machines.ContainsKey(indoors))
                        Machines[indoors] = new List<Vector2>();

                    foreach (var @object in indoors.Objects.Pairs)
                        if (_config.Machines.ContainsKey(@object.Value.name))
                            Machines[indoors].Add(@object.Key);
                }
            }
        }


        private static List<ReportReturnItem> EchoForCrabpots(Predicate<CrabPot> predicate)
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            foreach (var location in CrabPots)
                foreach (var position in location.Value)
                {
                    if (!(location.Key.objects[position] is CrabPot cp) || !predicate.Invoke(cp)) continue;
                    prItem.Add(new ReportReturnItem
                    {
                        Label = location.Key.Name + " (" + position.X.ToString() + ", " + position.Y.ToString() + ")",
                        WarpTo = new Tuple<string, int, int>(location.Key.Name, (int)position.X, (int)position.Y)
                    });
                }

            return prItem;
        }

        private List<ReportReturnItem> EchoForMachines(IModHelper helper)
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            foreach (var location in Machines)
                foreach (var position in location.Value)
                {
                    if (!location.Key.objects.TryGetValue(position, out var machine)) continue;
                    if (!MachineReady(machine)) continue;

                    var heldObject = machine.heldObject.Value;

                    var quality = "";
                    if (machine is Cask cask)
                    {
                        quality = heldObject.Quality == 1 ? I18n.Tasks_Object_Silver()
                            : heldObject.Quality == 2 ? I18n.Tasks_Object_Gold()
                            : heldObject.Quality == 4 ? I18n.Tasks_Object_Iridium()
                            : "";
                    }
                    try
                    {
                        prItem.Add(new ReportReturnItem
                        {
                            Label = machine.name + I18n.Tasks_Object_With() + quality + " " + heldObject.Name + I18n.Tasks_At() + location.Key.Name + " (" + position.X.ToString() + ", " + position.Y.ToString() + ")",
                            WarpTo = new Tuple<string, int, int>(location.Key.Name, (int)position.X, (int)position.Y)
                        }); ;
                    }
                    catch { }
                }

            return prItem;
        }

        private bool MachineReady(SObject o)
        {
            if (o != null)
            {
                return
                    (_config.Machines.TryGetValue(o.Name, out var enabled) && enabled && o.readyForHarvest.Value) ||
                    (o is Cask cask && cask.heldObject.Value?.Quality >= _config.Cask);
            }
            return false;
        }

        internal void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            Vector2 pos;
            SObject obj;
            var loc = Game1.currentLocation;
            if (e.Location is MineShaft || Game1.newDay) return;

            foreach (var item in e.Added)
            {
                pos = item.Key;
                obj = item.Value;
                if ((_config.Machines.ContainsKey(obj.name) || obj is Cask) &&
                        Machines.TryGetValue(loc, out var list))
                    list.Add(pos);
                else if (obj is CrabPot && CrabPots.TryGetValue(loc, out list))
                    list.Add(pos);
            }

            foreach (var item in e.Removed)
            {
                pos = item.Key;
                obj = item.Value;
                if (Machines.TryGetValue(loc, out var list))
                    list.Remove(pos);
                if (CrabPots.TryGetValue(loc, out list))
                    list.Remove(pos);
                break;
            }
        }
    }
}