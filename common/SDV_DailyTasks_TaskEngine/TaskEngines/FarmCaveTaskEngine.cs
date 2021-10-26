using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewValley.Locations;
#if StandAlone
using DailyTasksReport.UI;
#elif StardewWeb
using StardewWeb;

#endif

namespace DailyTasksReport.TaskEngines
{
    class FarmCaveTaskEngine : TaskEngine
    {
        public static readonly int[] Fruits = { 296, 396, 406, 410, 613, 634, 635, 636, 637, 638 };
        private string _farmCaveItemName;

        private readonly Dictionary<string, int> _objectsList = new Dictionary<string, int>();

        internal FarmCaveTaskEngine(TaskReportConfig config)
        {
            _config = config;
            TaskClass = "FarmCave";
            SetEnabled();
         }
        public override void Clear()
        {
            _farmCaveItemName = Game1.player.caveChoice.Value == 1 ? I18n.Tasks_Cave_ItemName_Fruits() : I18n.Tasks_Cave_ItemName_Mushrooms();
            _objectsList.Clear();
        }

        public override List<ReportReturnItem> DetailedInfo()
        {

            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled || _objectsList.Count == 0) return prItem;

            var stringBuilder = new StringBuilder();

            foreach (var pair in _objectsList)
            {
                var name = Pluralize(pair.Key, pair.Value);
                prItem.Add(new ReportReturnItem { Label = pair.Value.ToString() + " " + name });


            }

            return prItem;
        }

        public override List<ReportReturnItem> GeneralInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;

            if (_objectsList.Count == 0) return prItem;

            prItem.Add(new ReportReturnItem { Label = _farmCaveItemName + I18n.Tasks_Cave_InCave(), Details = _objectsList.Sum(o => o.Value).ToString() });

            return prItem;
        }

        public override void SetEnabled()
        {
            Enabled = _config.FarmCave && (Game1.player!=null &&  (Game1.player.caveChoice.Value != 0 || Game1.player.totalMoneyEarned >= 25000));
        }
        internal override void FirstScan()
        {
            var farmCave = Game1.locations.OfType<FarmCave>().FirstOrDefault();

            foreach (var obj in farmCave.objects.Values)
            {
                if (obj.ParentSheetIndex == 128 && obj.readyForHarvest.Value)
                {
                    var heldObject = obj.heldObject.Value;
                    if (heldObject != null)
                        if (_objectsList.ContainsKey(heldObject.displayName))
                            _objectsList[heldObject.displayName] += 1;
                        else
                            _objectsList[heldObject.displayName] = 1;
                }
                else if (Array.BinarySearch(Fruits, obj.ParentSheetIndex) >= 0)
                    if (_objectsList.ContainsKey(obj.displayName))
                        _objectsList[obj.displayName] += 1;
                    else
                        _objectsList[obj.displayName] = 1;
            }
        }
      
    }
}
