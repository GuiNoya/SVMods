using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace DailyTasksReport
{
    class SettingsMenu : IClickableMenu
    {
        private const int ItemsPerPage = 8;

        private ModEntry parent;
        private List<OptionsElement> options = new List<OptionsElement>();
        private List<Rectangle> slots = new List<Rectangle>();

        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;

        private int yMargin = Game1.tileSize / 4;
        private int currentIndex = 0;
        private int yScrollBarOffsetHeld = -1;

        public SettingsMenu(ModEntry parent, int currentIndex = 0) :
                        base(Game1.viewport.Width / 2 - Game1.tileSize * 10 / 2,
                             Game1.viewport.Height / 2 - Game1.tileSize * 8 / 2,
                             Game1.tileSize * 10,
                             Game1.tileSize * 8,
                             true)
        {
            this.parent = parent;
            this.currentIndex = currentIndex;

            Game1.playSound("bigSelect");

            upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width + Game1.pixelZoom * 4, yPositionOnScreen - Game1.pixelZoom * 20, Game1.pixelZoom * 12, Game1.pixelZoom * 12);

            // Initialize UI components
            upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen, Game1.pixelZoom * 11, Game1.pixelZoom * 12), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + height - Game1.pixelZoom * 12, Game1.pixelZoom * 11, Game1.pixelZoom * 12), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + Game1.pixelZoom * 3, upArrow.bounds.Y + upArrow.bounds.Height + Game1.pixelZoom, Game1.pixelZoom * 6, Game1.pixelZoom * 10), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, height - upArrow.bounds.Height * 2 - Game1.pixelZoom * 3);

            for (int i = 0; i < ItemsPerPage; ++i)
                slots.Add(new Rectangle(xPositionOnScreen, yPositionOnScreen + yMargin + (height - Game1.tileSize / 2) / ItemsPerPage * i, width, (height - yMargin * 2) / ItemsPerPage));

            // Add options
            options.Add(new Checkbox("Show detailed info", 1, parent.config));
            options.Add(new OptionsElement("Report:"));
            options.Add(new Checkbox("Unwatered crops", 2, parent.config));
            options.Add(new Checkbox("Unharvested crops", 3, parent.config));
            options.Add(new Checkbox("Dead crops", 4, parent.config));
            options.Add(new Checkbox("Unpetted pet", 5, parent.config));
            options.Add(new Checkbox("Unfilled pet bowl", 6, parent.config));
            options.Add(new Checkbox("Unpetted animals", 7, parent.config));
            options.Add(new Checkbox("Animal products:", -1, parent.config));
            options.Add(new Checkbox("Cow milk", 8, parent.config, 1));
            options.Add(new Checkbox("Goat milk", 9, parent.config, 1));
            options.Add(new Checkbox("Sheep wool", 10, parent.config, 1));
            options.Add(new Checkbox("Missing hay", 11, parent.config));
            options.Add(new Checkbox("Items in farm cave", 12, parent.config));
            options.Add(new Checkbox("Uncollected crabpots", 13, parent.config));
            options.Add(new Checkbox("Not baited crabpots", 14, parent.config));
            options.Add(new Checkbox("Machines:", -1, parent.config));
            options.Add(new Checkbox("Bee house", 15, parent.config, 1));
            options.Add(new Checkbox("Charcoal Kiln", 16, parent.config, 1));
            options.Add(new Checkbox("Cheese Press", 17, parent.config, 1));
            options.Add(new Checkbox("Crystalarium", 18, parent.config, 1));
            options.Add(new Checkbox("Furnace", 19, parent.config, 1));
            options.Add(new Checkbox("Keg", 20, parent.config, 1));
            options.Add(new Checkbox("Lightning Rod", 21, parent.config, 1));
            options.Add(new Checkbox("Loom", 22, parent.config, 1));
            options.Add(new Checkbox("Mayonnaise Machine", 23, parent.config, 1));
            options.Add(new Checkbox("Oil Maker", 24, parent.config, 1));
            options.Add(new Checkbox("Preserves Jar", 25, parent.config, 1));
            options.Add(new Checkbox("Recycling Machine", 26, parent.config, 1));
            options.Add(new Checkbox("Seed Maker", 27, parent.config, 1));
            options.Add(new Checkbox("Slime Egg-Press", 28, parent.config, 1));
            options.Add(new Checkbox("Soda Machine", 29, parent.config, 1));
            options.Add(new Checkbox("Statue Of Endless Fortune", 30, parent.config, 1));
            options.Add(new Checkbox("Statue Of Perfection", 31, parent.config, 1));
            options.Add(new Checkbox("Tapper", 32, parent.config, 1));
            options.Add(new Checkbox("Worm bin", 33, parent.config, 1));
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            drawTextureBox(Game1.spriteBatch, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            int yTitleOffset = (int)(SpriteText.getHeightOfString("Daily Tasks Report Settings") * 1.6);
            SpriteText.drawStringWithScrollCenteredAt(b, "Daily Tasks Settings", xPositionOnScreen + width / 2, yPositionOnScreen - yTitleOffset);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);

            for (int i = 0; i < ItemsPerPage; ++i)
            {
                options[currentIndex + i].draw(b, slots[i].X, slots[i].Y);
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            upArrow.draw(b);
            downArrow.draw(b);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, Game1.pixelZoom);
            scrollBar.draw(b);

            base.draw(b);
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (downArrow.bounds.Contains(x, y) && currentIndex < options.Count - ItemsPerPage)
            {
                ++currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shwip");
                return;
            }
            if (upArrow.bounds.Contains(x, y) && currentIndex > 0)
            {
                --currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shwip");
                return;
            }

            if (scrollBar.bounds.Contains(x, y))
            {
                yScrollBarOffsetHeld = y - scrollBar.bounds.Y;
                return;
            }

            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].Contains(x, y) && options[currentIndex + i].bounds.Contains(x - slots[i].X, y - slots[i].Y))
                {
                    options[currentIndex + i].receiveLeftClick(x, y);
                    return;
                }
            }

            // Check the close button
            base.receiveLeftClick(x, y, playSound);
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (yScrollBarOffsetHeld >= 0)
            {
                SetCurrentIndexFromScrollBar();
                AdjustScrollBarPosition();
                yScrollBarOffsetHeld = -1;
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            if (yScrollBarOffsetHeld < 0)
                return;

            int scrollBarTop = y - yScrollBarOffsetHeld;
            if (scrollBarTop < scrollBarRunner.Y)
                scrollBarTop = scrollBarRunner.Y;
            else if (scrollBarTop > scrollBarRunner.Bottom - scrollBar.bounds.Height)
                scrollBarTop = scrollBarRunner.Bottom - scrollBar.bounds.Height;
            scrollBar.bounds.Y = scrollBarTop;
            int oldCurrentIndex = currentIndex;
            SetCurrentIndexFromScrollBar();
            if (oldCurrentIndex != currentIndex)
                Game1.playSound("shiny4");
        }

        private void AdjustScrollBarPosition()
        {
            scrollBar.bounds.Y = scrollBarRunner.Y + (scrollBarRunner.Height - scrollBar.bounds.Height) / (options.Count - ItemsPerPage) * currentIndex;
        }

        private void SetCurrentIndexFromScrollBar()
        {
            currentIndex = (options.Count - ItemsPerPage) * (scrollBar.bounds.Y - scrollBarRunner.Y) / (scrollBarRunner.Height - scrollBar.bounds.Height);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (direction < 0 && currentIndex > 0)
            {
                ++currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shiny4");
            }
            else if (direction > 0 && currentIndex < options.Count - ItemsPerPage)
            {
                --currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shiny4");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            upArrow.tryHover(x, y);
            downArrow.tryHover(x, y);
            scrollBar.tryHover(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (Game1.activeClickableMenu is SettingsMenu)
                Game1.activeClickableMenu = new SettingsMenu(parent, currentIndex);
        }
    }
}
