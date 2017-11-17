using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace DailyTasksReport.UI
{
    class InputListener : OptionsElement
    {
        public static Rectangle buttonSource = new Rectangle(294, 428, 21, 11);
        public string buttonName = "";

        private ModConfig config;
        private OptionsEnum option;
        private Rectangle buttonBounds;
        private bool listening = false;

        public InputListener(string label, OptionsEnum whichOption, int slotWidth, ModConfig config, int itemLevel = 0) : base(label, -1, -1, slotWidth, Game1.pixelZoom * 11, (int)whichOption)
        {
            buttonBounds = new Rectangle(slotWidth - 28 * Game1.pixelZoom, Game1.pixelZoom * 3 - 1, 21 * Game1.pixelZoom, 11 * Game1.pixelZoom);

            this.config = config;
            this.option = whichOption;

            switch (whichOption)
            {
                case OptionsEnum.OpenReportKey:
                    buttonName = config.OpenReportKey.ToString();
                    break;
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (greyedOut || listening || !buttonBounds.Contains(x, y))
                return;
            listening = true;
            SettingsMenu.keyReceiver = this;
            Game1.playSound("breathin");
        }

        public override void receiveKeyPress(Keys key)
        {
            if (greyedOut || !listening)
                return;

            if (key == Keys.Escape)
            {
                Game1.playSound("bigDeSelect");
            }
            else
            {
                switch (option)
                {
                    case OptionsEnum.OpenReportKey:
                        config.OpenReportKey = (SButton)key;
                        break;
                }
                buttonName = ((SButton)key).ToString();
                SettingsMenu.configChanged = true;
                Game1.playSound("coin");
            }
            listening = false;
            SettingsMenu.keyReceiver = null;
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (buttonName.Length > 0 || whichOption == -1)
            {
                if (whichOption == -1)
                    Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
                else
                    Utility.drawTextWithShadow(b, label + ": " + buttonName, Game1.dialogueFont, new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
            }
            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(buttonBounds.X + slotX, buttonBounds.Y + slotY), buttonSource, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, false, 0.15f);
            if (!listening)
                return;
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Black * 0.75f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.999f);
            b.DrawString(Game1.dialogueFont, "Press new Key...", Utility.getTopLeftPositionForCenteringOnScreen(Game1.tileSize * 3, Game1.tileSize, 0, 0), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
        }
    }
}
