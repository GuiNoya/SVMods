using System;
using System.Collections.Generic;
using System.Linq;
using DailyTasksReport.UI;
using DailyTasksReport.TaskEngines;

namespace DailyTasksReport.Tasks
{
    class TerrainTask : Task
    {
        internal TerrainTask(TaskReportConfig config, TerrainTaskId id)
        {
            _config = config;
            _Engine = new TerrainTaskEngine(config, id);
            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }
        public override string DetailedInfo(out int usedLines, out bool skipNextPage)
        {
            skipNextPage = false;
            usedLines = 0;

            if (!Enabled || _Engine.DetailedInfo().Count() == 0) return "";


            string sHeader = (TerrainTaskId)_Engine.TaskId switch
            {
                TerrainTaskId.BrokenFences => I18n.Tasks_Terrain_Damaged(),
                _ => ""
            };

            return sHeader + ":^" + string.Join("^", _Engine.DetailedInfo()) + "^";
        }

        public override string GeneralInfo(out int usedLines)
        {
            usedLines = 0;


            if (!Enabled || _Engine.GeneralInfo().Count() == 0)
                return "";

            _Engine.UpdateList();
            usedLines = _Engine.GeneralInfo().Count();
            return string.Join("^", _Engine.GeneralInfo()) + "^";
        }
        private void SettingsMenu_ReportConfigChanged(object sender, EventArgs e)
        {
           _Engine.SetEnabled();
        }
    }
}
