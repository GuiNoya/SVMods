using DailyTasksReport.UI;
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

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
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

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || e.Button == SButton.None)
                return;

            if (e.Button == Config.OpenReportKey)
                OpenReport();
            else if (e.Button == Config.OpenSettings)
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