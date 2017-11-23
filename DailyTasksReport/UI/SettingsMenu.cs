using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace DailyTasksReport.UI
{
    internal class SettingsMenu : IClickableMenu
    {
        private const int ItemsPerPage = 8;

        private static IClickableMenu _previousMenu;
        internal static InputListener KeyReceiver = null;
        private readonly ClickableTextureComponent _downArrow;
        private readonly List<OptionsElement> _options = new List<OptionsElement>();

        private readonly ModEntry _parent;
        private readonly ClickableTextureComponent _scrollBar;
        private readonly List<Rectangle> _slots = new List<Rectangle>();

        private readonly ClickableTextureComponent _upArrow;
        private readonly int _yMargin = Game1.tileSize / 4;

        private int _currentIndex;
        private Rectangle _scrollBarRunner;
        private int _yScrollBarOffsetHeld = -1;

        private SettingsMenu(ModEntry parent, int currentIndex = 0) :
            base(Game1.viewport.Width / 2 - Game1.tileSize * 10 / 2,
                Game1.viewport.Height / 2 - Game1.tileSize * 8 / 2,
                Game1.tileSize * 10,
                Game1.tileSize * 8,
                true)
        {
            _parent = parent;
            _currentIndex = currentIndex;

            Game1.playSound("bigSelect");

            upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width + Game1.pixelZoom * 4,
                yPositionOnScreen - Game1.pixelZoom * 20, Game1.pixelZoom * 12, Game1.pixelZoom * 12);

            // Initialize UI components
            _upArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen, Game1.pixelZoom * 11,
                    Game1.pixelZoom * 12), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            _downArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width + Game1.tileSize / 4,
                    yPositionOnScreen + height - Game1.pixelZoom * 12, Game1.pixelZoom * 11, Game1.pixelZoom * 12),
                Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
            _scrollBar = new ClickableTextureComponent(
                new Rectangle(_upArrow.bounds.X + Game1.pixelZoom * 3,
                    _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom, Game1.pixelZoom * 6,
                    Game1.pixelZoom * 10), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
            _scrollBarRunner = new Rectangle(_scrollBar.bounds.X, _scrollBar.bounds.Y, _scrollBar.bounds.Width,
                height - _upArrow.bounds.Height * 2 - Game1.pixelZoom * 3);

            for (var i = 0; i < ItemsPerPage; ++i)
                _slots.Add(new Rectangle(xPositionOnScreen,
                    yPositionOnScreen + _yMargin + (height - Game1.tileSize / 2) / ItemsPerPage * i, width,
                    (height - _yMargin * 2) / ItemsPerPage));

            // Add options
            _options.Add(new InputListener("Open Report Key", OptionsEnum.OpenReportKey, _slots[0].Width, parent.Config));
            _options.Add(new InputListener("Open Settings Key", OptionsEnum.OpenSettings, _slots[0].Width, parent.Config));
            _options.Add(new Checkbox("Show detailed info", OptionsEnum.ShowDetailedInfo, parent.Config));
            _options.Add(new OptionsElement("Report:"));
            _options.Add(new Checkbox("Unwatered crops", OptionsEnum.UnwateredCrops, parent.Config));
            _options.Add(new Checkbox("Unharvested crops", OptionsEnum.UnharvestedCrops, parent.Config));
            _options.Add(new Checkbox("Dead crops", OptionsEnum.DeadCrops, parent.Config));
            _options.Add(new Checkbox("Unpetted pet", OptionsEnum.UnpettedPet, parent.Config));
            _options.Add(new Checkbox("Unfilled pet bowl", OptionsEnum.UnfilledPetBowl, parent.Config));
            _options.Add(new Checkbox("Unpetted animals", OptionsEnum.UnpettedAnimals, parent.Config));
            // Animal products
            _options.Add(new Checkbox("Animal products:", OptionsEnum.AllAnimalProducts, parent.Config));
            _options.Add(new Checkbox("Cow milk", OptionsEnum.CowMilk, parent.Config, 1));
            _options.Add(new Checkbox("Goat milk", OptionsEnum.GoatMilk, parent.Config, 1));
            _options.Add(new Checkbox("Sheep wool", OptionsEnum.SheepWool, parent.Config, 1));
            // Other configs
            _options.Add(new Checkbox("Missing hay", OptionsEnum.MissingHay, parent.Config));
            _options.Add(new Checkbox("Items in farm cave", OptionsEnum.FarmCave, parent.Config));
            _options.Add(new Checkbox("Uncollected crabpots", OptionsEnum.UncollectedCrabpots, parent.Config));
            _options.Add(new Checkbox("Not baited crabpots", OptionsEnum.NotBaitedCrabpots, parent.Config));
            // Machines
            _options.Add(new Checkbox("Machines:", OptionsEnum.AllMachines, parent.Config));
            _options.Add(new Checkbox("Bee house", OptionsEnum.BeeHouse, parent.Config, 1));
            _options.Add(new Checkbox("Charcoal Kiln", OptionsEnum.CharcoalKiln, parent.Config, 1));
            _options.Add(new Checkbox("Cheese Press", OptionsEnum.CheesePress, parent.Config, 1));
            _options.Add(new Checkbox("Crystalarium", OptionsEnum.Crystalarium, parent.Config, 1));
            _options.Add(new Checkbox("Furnace", OptionsEnum.Furnace, parent.Config, 1));
            _options.Add(new Checkbox("Keg", OptionsEnum.Keg, parent.Config, 1));
            _options.Add(new Checkbox("Lightning Rod", OptionsEnum.LightningRod, parent.Config, 1));
            _options.Add(new Checkbox("Loom", OptionsEnum.Loom, parent.Config, 1));
            _options.Add(new Checkbox("Mayonnaise Machine", OptionsEnum.MayonnaiseMachine, parent.Config, 1));
            _options.Add(new Checkbox("Oil Maker", OptionsEnum.OilMaker, parent.Config, 1));
            _options.Add(new Checkbox("Preserves Jar", OptionsEnum.PreservesJar, parent.Config, 1));
            _options.Add(new Checkbox("Recycling Machine", OptionsEnum.RecyclingMachine, parent.Config, 1));
            _options.Add(new Checkbox("Seed Maker", OptionsEnum.SeedMaker, parent.Config, 1));
            _options.Add(new Checkbox("Slime Egg-Press", OptionsEnum.SlimeEggPress, parent.Config, 1));
            _options.Add(new Checkbox("Soda Machine", OptionsEnum.SodaMachine, parent.Config, 1));
            _options.Add(new Checkbox("Statue Of Endless Fortune", OptionsEnum.StatueOfEndlessFortune, parent.Config, 1));
            _options.Add(new Checkbox("Statue Of Perfection", OptionsEnum.StatueOfPerfection, parent.Config, 1));
            _options.Add(new Checkbox("Tapper", OptionsEnum.Tapper, parent.Config, 1));
            _options.Add(new Checkbox("Worm bin", OptionsEnum.WormBin, parent.Config, 1));
        }
        
        public override void draw(SpriteBatch b)
        {
            _previousMenu?.draw(b);

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            drawTextureBox(Game1.spriteBatch, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            var yTitleOffset = (int) (SpriteText.getHeightOfString("Daily Tasks Report Settings") * 1.6);
            SpriteText.drawStringWithScrollCenteredAt(b, "Daily Tasks Settings", xPositionOnScreen + width / 2,
                yPositionOnScreen - yTitleOffset);

            _upArrow.draw(b);
            _downArrow.draw(b);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), _scrollBarRunner.X, _scrollBarRunner.Y,
                _scrollBarRunner.Width, _scrollBarRunner.Height, Color.White, Game1.pixelZoom);
            _scrollBar.draw(b);

            base.draw(b);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);

            for (var i = 0; i < ItemsPerPage; ++i)
                _options[_currentIndex + i].draw(b, _slots[i].X, _slots[i].Y);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (!Game1.options.hardwareCursor)
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                    Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0.0f,
                    Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (_downArrow.bounds.Contains(x, y) && _currentIndex < _options.Count - ItemsPerPage)
            {
                ++_currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shwip");
                return;
            }
            if (_upArrow.bounds.Contains(x, y) && _currentIndex > 0)
            {
                --_currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shwip");
                return;
            }

            if (_scrollBar.bounds.Contains(x, y))
            {
                _yScrollBarOffsetHeld = y - _scrollBar.bounds.Y;
                return;
            }

            if (_scrollBarRunner.Contains(x, y))
            {
                _yScrollBarOffsetHeld = _scrollBar.bounds.Height / 2;
                return;
            }

            var optionClicked = false;
            for (var i = 0; i < _slots.Count; ++i)
                // ReSharper disable once InvertIf
                if (_slots[i].Contains(x, y) &&
                    _options[_currentIndex + i].bounds.Contains(x - _slots[i].X, y - _slots[i].Y))
                {
                    _options[_currentIndex + i].receiveLeftClick(x - _slots[i].X, y - _slots[i].Y);
                    optionClicked = true;
                    _parent.RefreshReport = true;
                    break;
                }

            if (optionClicked)
            {
                RefreshOptionStatus();
                _parent.Helper.WriteConfig(_parent.Config);
            }

            // Check the close button
            base.receiveLeftClick(x, y, playSound);
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (_yScrollBarOffsetHeld < 0)
                return;

            SetCurrentIndexFromScrollBar();
            AdjustScrollBarPosition();
            _yScrollBarOffsetHeld = -1;
        }

        public override void leftClickHeld(int x, int y)
        {
            if (_yScrollBarOffsetHeld < 0)
                return;

            var scrollBarTop = y - _yScrollBarOffsetHeld;
            if (scrollBarTop < _scrollBarRunner.Y)
                scrollBarTop = _scrollBarRunner.Y;
            else if (scrollBarTop > _scrollBarRunner.Bottom - _scrollBar.bounds.Height)
                scrollBarTop = _scrollBarRunner.Bottom - _scrollBar.bounds.Height;
            _scrollBar.bounds.Y = scrollBarTop;
            var oldCurrentIndex = _currentIndex;
            SetCurrentIndexFromScrollBar();
            if (oldCurrentIndex != _currentIndex)
                Game1.playSound("shiny4");
        }

        private void AdjustScrollBarPosition()
        {
            _scrollBar.bounds.Y = _scrollBarRunner.Y + (_scrollBarRunner.Height - _scrollBar.bounds.Height) /
                                  (_options.Count - ItemsPerPage) * _currentIndex;
        }

        private void SetCurrentIndexFromScrollBar()
        {
            _currentIndex = (_options.Count - ItemsPerPage) * (_scrollBar.bounds.Y - _scrollBarRunner.Y) /
                            (_scrollBarRunner.Height - _scrollBar.bounds.Height);
        }

        private void RefreshOptionStatus()
        {
            foreach (var option in _options)
                if (option is Checkbox cb)
                    cb.RefreshStatus();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (direction > 0 && _currentIndex > 0)
            {
                --_currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && _currentIndex < _options.Count - ItemsPerPage)
            {
                ++_currentIndex;
                AdjustScrollBarPosition();
                Game1.playSound("shiny4");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            _upArrow.tryHover(x, y);
            _downArrow.tryHover(x, y);
            _scrollBar.tryHover(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (KeyReceiver != null)
            {
                KeyReceiver.receiveKeyPress(key);
                _parent.Helper.WriteConfig(_parent.Config);
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (!(Game1.activeClickableMenu is SettingsMenu)) return;

            Game1.activeClickableMenu = new SettingsMenu(_parent, _currentIndex);
            _previousMenu?.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public static void OpenMenu(ModEntry parent)
        {
            _previousMenu = Game1.activeClickableMenu;
            if (Game1.activeClickableMenu != null)
                Game1.exitActiveMenu();
            Game1.activeClickableMenu = new SettingsMenu(parent)
            {
                exitFunction = OnExitFunc
            };
        }

        private static void OnExitFunc()
        {
            if (_previousMenu == null) return;
            Game1.activeClickableMenu = _previousMenu;
            _previousMenu = null;
        }

        public static event EventHandler ReportConfigChanged;

        internal static void RaiseReportConfigChanged()
        {
            var handler = ReportConfigChanged;
            handler?.Invoke(null, null);
        }
    }
}