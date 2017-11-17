using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;

namespace DailyTasksReport.UI
{
    public class ReportMenu : IClickableMenu
    {
        private ModEntry parent;

        private List<string> mailMessage = new List<string>();
        private int page;
        private float scale;
        private bool firstKeyEvent;

        public Texture2D letterTexture;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;

        public ReportMenu(ModEntry parent, string text) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y, 320 * Game1.pixelZoom, 180 * Game1.pixelZoom, true)
        {
            Game1.playSound("shwip");
            this.backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + Game1.tileSize / 2, yPositionOnScreen + height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), Game1.pixelZoom)
            {
                myID = 101,
                rightNeighborID = 102
            };
            this.forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, yPositionOnScreen + height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), Game1.pixelZoom)
            {
                myID = 102,
                leftNeighborID = 101
            };
            this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, width - Game1.tileSize / 2, height - Game1.tileSize * 2);

            this.parent = parent;
            this.firstKeyEvent = true;
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(102);
            snapCursorToCurrentSnappedComponent();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X;
            yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y;
            backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + Game1.tileSize / 2, yPositionOnScreen + height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), Game1.pixelZoom)
            {
                myID = 101,
                rightNeighborID = 102
            };
            forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, yPositionOnScreen + height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), Game1.pixelZoom)
            {
                myID = 102,
                leftNeighborID = 101
            };
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.LeftTrigger && page > 0)
            {
                --page;
                Game1.playSound("shwip");
            }
            else if (b == Buttons.RightTrigger && page < mailMessage.Count - 1)
            {
                ++page;
                Game1.playSound("shwip");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (backButton.containsPoint(x, y) && page > 0)
            {
                --page;
                Game1.playSound("shwip");
            }
            else if (forwardButton.containsPoint(x, y) && page < mailMessage.Count - 1)
            {
                ++page;
                Game1.playSound("shwip");
            }
            else if (isWithinBounds(x, y))
            {
                if (page < mailMessage.Count - 1)
                {
                    ++page;
                    Game1.playSound("shwip");
                }
                else if (page == mailMessage.Count - 1 && mailMessage.Count > 1)
                {
                    page = 0;
                    Game1.playSound("shwip");
                }
                else if (mailMessage.Count == 1)
                {
                    exitThisMenuNoSound();
                    Game1.playSound("shwip");
                }
            }
            else
            {
                exitThisMenuNoSound();
                Game1.playSound("shwip");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            backButton.tryHover(x, y, 0.6f);
            forwardButton.tryHover(x, y, 0.6f);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (scale < 1.0)
            {
                scale = (float)(scale + time.ElapsedGameTime.Milliseconds * (3.0 / 1000.0));
                if (scale >= 1.0)
                    scale = 1f;
            }
            if (page >= mailMessage.Count - 1 || forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
                return;
            forwardButton.scale = (float)(4.0 + Math.Sin(time.TotalGameTime.Milliseconds / (64.0 * Math.PI)) / 1.5);
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            if ((SButton)key == parent.config.OpenReportKey && readyToClose())
            {
                if (firstKeyEvent)
                {
                    firstKeyEvent = false;
                    return;
                }
                exitThisMenu();
            }
            else if (key == Keys.Right && page < mailMessage.Count - 1)
            {
                ++page;
                Game1.playSound("shwip");
            }
            else if (key == Keys.Left && page > 0)
            {
                --page;
                Game1.playSound("shwip");
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            receiveLeftClick(x, y, playSound);
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle?(new Rectangle(0, 0, 320, 180)), Color.White, 0.0f, new Vector2(160f, 90f), Game1.pixelZoom * scale, SpriteEffects.None, 0.86f);
            if (scale == 1.0)
            {
                SpriteText.drawString(b, mailMessage[page], xPositionOnScreen + Game1.tileSize / 2, yPositionOnScreen + Game1.tileSize / 2, 999999, width - Game1.tileSize, 999999, 0.75f, 0.865f);
                base.draw(b);
                if (page < mailMessage.Count - 1)
                    forwardButton.draw(b);
                if (page > 0)
                    backButton.draw(b);
            }
            if (!Game1.options.hardwareCursor)
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }
    }
}
