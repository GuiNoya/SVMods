using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ZoomLevelKeybind
{
    // ReSharper disable once UnusedMember.Global
    public class ModEntry : Mod
    {
        private ModConfig _config;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            ControlEvents.ControllerButtonPressed += ControlEvents_ControllerButtonPressed;
            ControlEvents.ControllerTriggerPressed += ControlEvents_ControllerTriggerPressed;
        }
        
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            //if (!Context.IsWorldReady)
            //    return;

            if (e.KeyPressed.ToString() == _config.IncreaseZoomKey)
                IncreaseZoom();
            else if (e.KeyPressed.ToString() == _config.DecreaseZoomKey)
                DecreaseZoom();
        }

        private void ControlEvents_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            HandleController(e.ButtonPressed);
        }

        private void ControlEvents_ControllerTriggerPressed(object sender, EventArgsControllerTriggerPressed e)
        {
            HandleController(e.ButtonPressed);
        }

        private void HandleController(Buttons buttonPressed)
        {
            //if (!Context.IsWorldReady)
            //    return;

            var wasZoom = false;

            if (buttonPressed.ToString() == _config.IncreaseZoomButton)
            {
                IncreaseZoom();
                wasZoom = true;
            }
            else if (buttonPressed.ToString() == _config.DecreaseZoomButton)
            {
                DecreaseZoom();
                wasZoom = true;
            }

            if (_config.SuppressControllerButton && wasZoom)
                SuppressButton(buttonPressed);
        }

        private void IncreaseZoom()
        {
            if (_config.UnlimitedZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel >= 0.05f ? 0.05f : (float) Math.Round(Game1.options.zoomLevel + 0.05, 2);
            else
                Game1.options.zoomLevel = Game1.options.zoomLevel >= 1.25f ? 1.25f : (float) Math.Round(Game1.options.zoomLevel + 0.05, 2);

            Program.gamePtr.refreshWindowSettings();
        }

        private void DecreaseZoom()
        {
            if (_config.UnlimitedZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.05f ? 0.05f : (float) Math.Round(Game1.options.zoomLevel - 0.05, 2);
            else if (_config.MoreZoom)
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.35f ? 0.35f : (float) Math.Round(Game1.options.zoomLevel - 0.05, 2);
            else
                Game1.options.zoomLevel = Game1.options.zoomLevel <= 0.75f ? 0.75f : (float) Math.Round(Game1.options.zoomLevel - 0.05, 2);

            Program.gamePtr.refreshWindowSettings();
        }

        // From SMAPI 2.0 (SMAPI/Events/EventArgsInput.cs)
        private static void SuppressButton(Buttons button)
        {
            var newState = GamePad.GetState(PlayerIndex.One);
            var thumbsticks = Game1.oldPadState.ThumbSticks;
            var triggers = Game1.oldPadState.Triggers;
            var buttons = Game1.oldPadState.Buttons;
            var dpad = Game1.oldPadState.DPad;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (button)
            {
                // d-pad
                case Buttons.DPadDown:
                    dpad = new GamePadDPad(dpad.Up, newState.DPad.Down, dpad.Left, dpad.Right);
                    break;
                case Buttons.DPadLeft:
                    dpad = new GamePadDPad(dpad.Up, dpad.Down, newState.DPad.Left, dpad.Right);
                    break;
                case Buttons.DPadRight:
                    dpad = new GamePadDPad(dpad.Up, dpad.Down, dpad.Left, newState.DPad.Right);
                    break;
                case Buttons.DPadUp:
                    dpad = new GamePadDPad(newState.DPad.Up, dpad.Down, dpad.Left, dpad.Right);
                    break;

                // trigger
                case Buttons.LeftTrigger:
                    triggers = new GamePadTriggers(newState.Triggers.Left, triggers.Right);
                    break;
                case Buttons.RightTrigger:
                    triggers = new GamePadTriggers(triggers.Left, newState.Triggers.Right);
                    break;

                // thumbstick
                case Buttons.LeftThumbstickDown:
                case Buttons.LeftThumbstickLeft:
                case Buttons.LeftThumbstickRight:
                case Buttons.LeftThumbstickUp:
                    thumbsticks = new GamePadThumbSticks(newState.ThumbSticks.Left, thumbsticks.Right);
                    break;
                case Buttons.RightThumbstickDown:
                case Buttons.RightThumbstickLeft:
                case Buttons.RightThumbstickRight:
                case Buttons.RightThumbstickUp:
                    thumbsticks = new GamePadThumbSticks(newState.ThumbSticks.Right, thumbsticks.Left);
                    break;

                // buttons
                default:
                    var mask =
                        (buttons.A == ButtonState.Pressed ? Buttons.A : 0)
                        | (buttons.B == ButtonState.Pressed ? Buttons.B : 0)
                        | (buttons.Back == ButtonState.Pressed ? Buttons.Back : 0)
                        | (buttons.BigButton == ButtonState.Pressed ? Buttons.BigButton : 0)
                        | (buttons.LeftShoulder == ButtonState.Pressed ? Buttons.LeftShoulder : 0)
                        | (buttons.LeftStick == ButtonState.Pressed ? Buttons.LeftStick : 0)
                        | (buttons.RightShoulder == ButtonState.Pressed ? Buttons.RightShoulder : 0)
                        | (buttons.RightStick == ButtonState.Pressed ? Buttons.RightStick : 0)
                        | (buttons.Start == ButtonState.Pressed ? Buttons.Start : 0)
                        | (buttons.X == ButtonState.Pressed ? Buttons.X : 0)
                        | (buttons.Y == ButtonState.Pressed ? Buttons.Y : 0);
                    mask = mask ^ button;
                    buttons = new GamePadButtons(mask);
                    break;
            }

            Game1.oldPadState = new GamePadState(thumbsticks, triggers, buttons, dpad);
        }
    }
}