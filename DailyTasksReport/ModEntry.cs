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
        private bool _refreshReport;

        internal ModConfig Config;

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

            if (Config.Cask < 0 || Config.Cask > 4)
            {
                Monitor.Log("Wrong configuration for Casks, setting to iridium quality...", LogLevel.Error);
                Config.Cask = 4;
            }

            _report = new ReportBuilder(this);

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;
            SettingsMenu.ReportConfigChanged += SettingsMenu_ReportConfigChanged;
        }
        
        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (_refreshReport)
                if (e.PriorMenu is SettingsMenu && SettingsMenu.PreviousMenu is ReportMenu)
                    OpenReport(true);
                _refreshReport = false;
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

        private void SettingsMenu_ReportConfigChanged(object sender, System.EventArgs e)
        {
            _refreshReport = true;
        }

        private void OpenReport(bool skipAnimation = false)
        {
            if (Game1.activeClickableMenu != null)
                Game1.exitActiveMenu();
            Game1.activeClickableMenu = new ReportMenu(this, _report.GetReportText(), skipAnimation);
        }
    }
}