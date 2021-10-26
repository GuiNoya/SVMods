using System.Collections.Generic;
using System.Linq;
using StardewValley;
using DailyTasksReport.Tasks;
using DailyTasksReport.UI;

namespace DailyTasksReport.TaskEngines
{
    class TerrainTaskEngine : TaskEngine
    {
        private List<Fence> _Fences = new List<Fence> { };
        public static TaskReportConfig _config;
        private readonly TerrainTaskId _id;

        internal TerrainTaskEngine(TaskReportConfig config, TerrainTaskId id)
        {
            _config = config;
            _id = id;
            TaskClass = "Terrain";
            TaskSubClass = id.ToString();
        }
        public override void Clear()
        {
            _Fences.Clear();
        }

        public override List<ReportReturnItem> DetailedInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;

            GameLocation location = Game1.locations.OfType<Farm>().FirstOrDefault();

            foreach (Fence f in location.Objects.Values.OfType<Fence>())
            {
                if (f.health < 1)
                {
                    //prItem.Add(new ReportReturnItem { Label = @"Fence Health (<span onclick=""WarpTo('Farm'," + f.tileLocation.X.ToString() + "," + f.tileLocation.Y.ToString() + @"); return false;"">" + f.tileLocation.X.ToString() + "," + f.tileLocation.Y.ToString() + "</span>): " + f.health.ToString() });
                    prItem.Add(new ReportReturnItem
                    {
                        Label = I18n.Tasks_Terrain_Fence() + " (" + f.tileLocation.X.ToString() + ", " + f.tileLocation.Y.ToString() + ")",
                        WarpTo = new System.Tuple<string, int, int>("Farm", (int)f.tileLocation.X, (int)f.tileLocation.Y)
                    });
                }
            }

            return prItem;
        }

        public override List<ReportReturnItem> GeneralInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;


            if (_Fences.Count > 0)
            {
                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Terrain_Damaged() + _Fences.Count.ToString() });
            }
            return prItem;
        }

        internal override void FirstScan()
        {
            GameLocation location = Game1.locations.OfType<Farm>().FirstOrDefault();

            foreach (Fence f in location.Objects.Values.OfType<Fence>())
            {
                if (f.health < 1)
                {
                    _Fences.Add(f);
                }
            }
        }
    }
}
