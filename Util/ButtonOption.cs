#nullable enable
#if EnableCommonPatches
using Common.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace Common.Util
{
    public class ButtonClickEventArgs(string fieldID)
    {
        public string FieldID { get; } = fieldID;
    }

    internal class ButtonOptions
    {
        // Fields
        private readonly Func<string> leftText;
        private readonly Func<string> rightText;
        //private readonly Func<string>? hoverText;
        private readonly string fieldID;

        //private readonly bool renderRightHover = false;
        //private readonly bool renderLeftHover = false;
        private bool isRightHovered = false;
        private bool wasRightHoveredPreviously = false;
        //private bool isLeftHovered = false;
        //private bool wasLeftHoveredPreviously = false;
        private ButtonState lastButtonState;
        private (int Top, int Left) storedValues;
        private const double ClickCooldown = 0.1;
        private static double lastClickTime = 0;

        // Properties
        public int RightTextWidth { get; private set; }
        public int RightTextHeight { get; private set; }
        public int LeftTextWidth { get; private set; }
        public int LeftTextHeight { get; private set; }

        // Events
        public static Action<ButtonClickEventArgs>? Click { get; set; }

        // Constructor
        public ButtonOptions(Func<string>? leftText, Func<string>? rightText, string? fieldID = null)
        {
            this.leftText = leftText ?? (() => "");
            this.rightText = rightText ?? (() => "");
            this.fieldID = fieldID ?? "";
            //this.renderRightHover = rightHover;
            //this.renderLeftHover = leftHover;
            //this.hoverText = hoverText;
            CalculateTextMeasurements();
        }

        // Private Methods
        // Calculate the width and height of the text for drawing
        private void CalculateTextMeasurements()
        {
            RightTextWidth = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText(), Game1.dialogueFont, 800)).X;
            RightTextHeight = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText(), Game1.dialogueFont, 800)).Y;

            // TODO FIX
            LeftTextWidth = (int)MeasureString(leftText(), true).X;
            LeftTextHeight = (int)MeasureString(leftText(), true).Y;
        }

        // Measure the width and height of a string
        private static Vector2 MeasureString(string text, bool bold = false, float scale = 1f, SpriteFont? font = null)
        {
            return bold ? new Vector2((float)SpriteText.getWidthOfString(text) * scale, (float)SpriteText.getHeightOfString(text) * scale) : (font ?? Game1.dialogueFont).MeasureString(text) * scale;
        }

        // Handle the button click event
        private static void OnClick(string fieldId)
        {
            double currentTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
            if (currentTime - lastClickTime >= ClickCooldown)
            {
                //Game1.playSound("backpackIN");
                Click?.Invoke(new ButtonClickEventArgs(fieldId));
                lastClickTime = currentTime;
            } else
            {
                Game1.playSound("thudStep");
            }
        }

        // Check if the mouse is hovering over the button
        private static bool IsHovered(Vector2 drawPos, int width, int height)
        {
            int mouseX = Mouse.GetState().X;
            int mouseY = Mouse.GetState().Y;
            return drawPos.X <= mouseX && mouseX <= drawPos.X + width && drawPos.Y <= mouseY && mouseY <= drawPos.Y + height;
        }

        // Update the mouse state and handle button hover sound effects
        private void UpdateMouseState(Vector2 drawPos)
        {
            ButtonState buttonState = Mouse.GetState().LeftButton;
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // Check if the button is clicked
            if (buttonState == ButtonState.Pressed && drawPos.X <= mouseX && mouseX <= drawPos.X + RightTextWidth && drawPos.Y <= mouseY && mouseY <= drawPos.Y + RightTextHeight && lastButtonState == ButtonState.Released)
            {
                OnClick(fieldID);
            }

            lastButtonState = buttonState;

            // Calculate the position of the button and check for hover effect
            int gmcmWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
            int gmcmLeft = (Game1.uiViewport.Width - gmcmWidth) / 2;
            int top = (int)drawPos.Y;
            int left = gmcmLeft;

            bool isRightHoveredNow = IsHovered(drawPos, RightTextWidth, RightTextHeight);

            // Play hover sound effect if the button is hovered over
            if (isRightHoveredNow && !wasRightHoveredPreviously)
            {
                Game1.playSound("shiny4");
            }

            //bool isLeftHoveredNow = IsHovered(drawPos, LeftTextWidth, LeftTextHeight);

            isRightHovered = isRightHoveredNow;
            wasRightHoveredPreviously = isRightHoveredNow;

            //isLeftHovered = isLeftHoveredNow;
            //wasLeftHoveredPreviously = isLeftHoveredNow;

            storedValues = (top, left);
        }

        // Public Methods
        // Draw the button with hover effect
        public void Draw(SpriteBatch b, Vector2 position)
        {
            try
            {
                CalculateTextMeasurements(); // To correct size when switching languages
                UpdateMouseState(position);

                Color rightTextColor = isRightHovered ? Game1.unselectedOptionColor : Game1.textColor;
                Vector2 rightTextPosition = new(position.X, position.Y);
                Utility.drawTextWithShadow(b, rightText(), Game1.dialogueFont, rightTextPosition, rightTextColor);

                Vector2 leftTextPosition = new(storedValues.Left - 8, storedValues.Top);
                SpriteText.drawString(b, leftText(), (int)leftTextPosition.X, (int)leftTextPosition.Y, layerDepth: 1f, color: new Color?());


                // TODO FIX HOVER
                //TooltipHelper.Hover = hoverText!();

            }
            catch (Exception e)
            {
                ConfigManager.Monitor?.Log($"Error in ButtonOptions Draw: {e}", LogLevel.Error);
            }
        }
    }
}
#endif