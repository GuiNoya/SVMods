using DailyTasksReport.UI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DailyTasksReport.TaskEngines;

#pragma warning disable CC0021 // Use nameof

namespace DailyTasksReport.Tasks
{
    public class FarmCaveTask : Task
    {

        internal FarmCaveTask(TaskReportConfig config)
        {
            _config = config;
            _Engine = new FarmCaveTaskEngine(config);

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
            if (!Enabled || _Engine.DetailedInfo().Count == 0) return "";

            return I18n.Tasks_Cave_InCave() + ":^" + string.Join("^", _Engine.DetailedInfo()) + "^";

        }
        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
           _Engine.SetEnabled();
        }
    }
}

#pragma warning restore CC0021 // Use nameof