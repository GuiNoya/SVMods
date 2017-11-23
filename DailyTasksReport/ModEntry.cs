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

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
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

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || e.Button == SButton.None)
                return;

            if (e.Button == Config.OpenReportKey)
                OpenReport();
            else if (e.Button == Config.OpenSettings)
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