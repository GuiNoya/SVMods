using System;
using DailyTasksReport.UI;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DailyTasksReport
{
    /// <summary>The mod entry point.</summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModEntry : Mod
    {
        internal static IMonitor MonitorHelper;
        internal static IReflectionHelper ReflectionHelper;

        private ReportBuilder _report;

        internal ModConfig Config;
        internal bool RefreshReport;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            MonitorHelper = Monitor;
            ReflectionHelper = helper.Reflection;

            Config = helper.ReadConfig<ModConfig>();

            _report = new ReportBuilder(this);

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (RefreshReport && e.PriorMenu is SettingsMenu && e.NewMenu is ReportMenu)
            {
                RefreshReport = false;
                OpenReport(true);
            }
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || e.KeyPressed == Keys.None)
                return;

            if (e.KeyPressed.ToString() == Config.OpenReportKey)
                OpenReport();
            else if (e.KeyPressed.ToString() == Config.OpenSettings)
                SettingsMenu.OpenMenu(this);
        }

        private void OpenReport(bool skipAnimation = false)
        {
            if (Game1.activeClickableMenu != null)
                Game1.exitActiveMenu();
            Game1.activeClickableMenu = new ReportMenu(this, _report.GetReportText(), skipAnimation);
        }
    }
}