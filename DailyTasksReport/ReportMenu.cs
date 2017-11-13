using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace DailyTasksReport
{
    public class ReportMenu : LetterViewerMenu
    {
        private ModEntry parent;
        private IPrivateField<int> pageNumber;
        private int numberOfPages;
        private bool firstKeyEvent;

        public ReportMenu(ModEntry parent, string text) : base(text)
        {
            this.parent = parent;
            this.pageNumber = parent.Helper.Reflection.GetPrivateField<int>(this, "page");
            this.numberOfPages = parent.Helper.Reflection.GetPrivateField<List<string>>(this, "mailMessage").GetValue().Count;
            this.firstKeyEvent = true;
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            if (key.ToString() == parent.config.OpenReportKey && this.readyToClose())
            {
                if (firstKeyEvent)
                {
                    firstKeyEvent = false;
                    return;
                }
                this.exitThisMenu();
            }
            else if (key == Keys.Right && pageNumber.GetValue() < numberOfPages - 1)
            {
                pageNumber.SetValue(pageNumber.GetValue() + 1);
                Game1.playSound("shwip");
            }
            else if (key == Keys.Left && pageNumber.GetValue() > 0)
            {
                pageNumber.SetValue(pageNumber.GetValue() - 1);
                Game1.playSound("shwip");
            }
        }
    }
}
