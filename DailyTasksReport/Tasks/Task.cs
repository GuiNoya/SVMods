using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DailyTasksReport.Tasks
{
    public abstract class Task
    {
        internal static readonly Dictionary<int, string> ObjectsNames = new Dictionary<int, string>();
        protected bool Enabled = true;

        public abstract void FirstScan();
        public abstract void Clear();
        public abstract string GeneralInfo(out int usedLines);
        public abstract string DetailedInfo(out int usedLines, out bool skipNextPage);

        public virtual void FinishedReport()
        {
        }

        public virtual void OnDayStarted()
        {
            Clear();
            FirstScan();
        }

        public virtual void Draw(SpriteBatch b)
        {
        }

        internal static void PopulateObjectsNames()
        {
            foreach (var pair in Game1.objectInformation)
                ObjectsNames[pair.Key] = pair.Value.Split("/".ToCharArray())[0];
        }
    }
}