using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace DailyTasksReport.UI
{
    internal class InputListener : OptionsElement
    {
        private static readonly Rectangle ButtonSource = new Rectangle(294, 428, 21, 11);

        private readonly ModConfig _config;
        private readonly OptionsEnum _option;
        private Rectangle _buttonBounds;
        private string _buttonName;
        private bool _listening;

        public InputListener(string label, OptionsEnum whichOption, int slotWidth, ModConfig config)
            : base(label, -1, -1, slotWidth, Game1.pixelZoom * 11, (int) whichOption)
        {
            _buttonBounds = new Rectangle(slotWidth - 28 * Game1.pixelZoom, Game1.pixelZoom * 3 - 1,
                21 * Game1.pixelZoom, 11 * Game1.pixelZoom);

            _config = config;
            _option = whichOption;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (whichOption)
            {
                case OptionsEnum.OpenReportKey:
                    _buttonName = config.OpenReportKey;
                    break;
                case OptionsEnum.OpenSettings:
                    _buttonName = config.OpenSettings;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Option {_option} is not possible on a InputListener.");
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (greyedOut || _listening || !_buttonBounds.Contains(x, y))
                return;
            _listening = true;
            SettingsMenu.KeyReceiver = this;
            Game1.playSound("breathin");
        }

        public override void receiveKeyPress(Keys key)
        {
            if (greyedOut || !_listening)
                return;

            if (key == Keys.Escape)
            {
                Game1.playSound("bigDeSelect");
            }
            else
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_option)
                {
                    case OptionsEnum.OpenReportKey:
                        _config.OpenReportKey = key.ToString();
                        break;
                    case OptionsEnum.OpenSettings:
                        _config.OpenSettings = key.ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Option {_option} is not possible on a InputListener.");
                }
                _buttonName = key.ToString();
                Game1.playSound("coin");
            }
            _listening = false;
            SettingsMenu.KeyReceiver = null;
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (_buttonName.Length > 0 || whichOption == -1)
                if (whichOption == -1)
                    Utility.drawTextWithShadow(b, label, Game1.dialogueFont,
                        new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
                else
                    Utility.drawTextWithShadow(b, label + ": " + _buttonName, Game1.dialogueFont,
                        new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(_buttonBounds.X + slotX, _buttonBounds.Y + slotY),
                ButtonSource, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, false, 0.15f);
            if (!_listening)
                return;
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height),
                new Rectangle(0, 0, 1, 1), Color.Black * 0.75f, 0.0f, Vector2.Zero, SpriteEffects.None,
                0.999f);
            b.DrawString(Game1.dialogueFont, "Press new Key...",
                Utility.getTopLeftPositionForCenteringOnScreen(Game1.tileSize * 3, Game1.tileSize), Color.White,
                0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
        }
    }
}