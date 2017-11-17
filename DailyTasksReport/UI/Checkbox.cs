using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DailyTasksReport
{
    internal class Checkbox : OptionsElement
    {
        private ModConfig config;
        private readonly int itemLevel;
        private int whichCategory;
        public bool isChecked;

        public Checkbox(string label, int whichOption, ModConfig config, int itemLevel = 0) : base(label, -1, -1, Game1.pixelZoom * 9, Game1.pixelZoom * 9, whichOption < 0 ? -1 : whichOption)
        {
            this.whichCategory = whichOption;
            this.config = config;
            this.itemLevel = itemLevel;
            this.bounds.X += itemLevel * Game1.pixelZoom * 7;

            // Load options
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (whichOption == -1)
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y + Game1.pixelZoom), isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
                base.draw(b, slotX + bounds.X + Game1.pixelZoom * 4, slotY - Game1.pixelZoom * 3);
            }
            else
            {
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y), isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
                base.draw(b, slotX, slotY);
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (greyedOut)
                return;

            Game1.playSound("drumkit6");
            isChecked = !isChecked;
            // Change options
        }
    }
}
