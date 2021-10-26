using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace DailyTasksReport.TaskEngines
{
    abstract class TaskEngine
    {
        internal IModHelper _helper;
        internal static readonly Dictionary<int, string> ObjectsNames = new Dictionary<int, string>();
        internal bool Enabled = true;
        public string TaskClass { get; set; }
        public string TaskSubClass { get; set; }
        public int TaskId { get; set; }
        internal abstract void FirstScan();
        public virtual void UpdateList() { }

        public abstract void Clear();

        public abstract List<ReportReturnItem> GeneralInfo();

        public abstract List<ReportReturnItem> DetailedInfo();

        public virtual void FinishedReport()
        {
        }

        public virtual void OnDayStarted()
        {
            Clear();
            FirstScan();
        }



        internal static void PopulateObjectsNames()
        {
            foreach (var pair in Game1.objectInformation)
                ObjectsNames[pair.Key] = pair.Value.Split("/".ToCharArray())[0];
        }
    }
}

