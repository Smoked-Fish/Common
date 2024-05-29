#nullable enable
using Common.Helpers;
using Common.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace Common.Utilities.Options
{
    public class ButtonClickEventData(string fieldID)
    {
        public string FieldID { get; } = fieldID;
    }

    internal sealed class ButtonOptions
    {
        // Fields
        private readonly Func<string> leftText;
        private readonly Func<string> rightText;
        private readonly Func<string>? descText;
        private readonly Func<string>? hoverText;
        private readonly bool renderLeft;
        private readonly bool renderRight;
        private readonly string fieldID;

        private bool isRightHovered;
        private bool wasRightHoveredPreviously;
        private bool isLeftHovered;
        private bool wasLeftHoveredPreviously;
        private ButtonState lastButtonState;
        private (int Top, int Left) storedValues;
        private const double ClickCooldown = 0.1;
        private static double lastClickTime;

        // Properties
        public int RightTextWidth { get; set; }
        public int RightTextHeight { get; set; }
        public int LeftTextWidth { get; set; }
        public int LeftTextHeight { get; set; }

        // Events
        public static Action<ButtonClickEventData>? Click { get; set; }

        // Constructor
        public ButtonOptions(Func<string> leftText, Func<string> rightText, Func<string>? descText = null, Func<string>? hoverText = null, bool renderLeft = false, bool renderRight = false, string? fieldID = null)
        {
            this.leftText = leftText;
            this.rightText = rightText;
            this.descText = descText;
            this.hoverText = hoverText;
            this.renderLeft = renderLeft;
            this.renderRight = renderRight;
            this.fieldID = fieldID ?? leftText();
            CalculateTextMeasurements();
        }

        // Private Methods
        // Calculate the width and height of the text for drawing
        private void CalculateTextMeasurements()
        {
            RightTextWidth = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText(), Game1.dialogueFont, 800)).X;
            RightTextHeight = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText(), Game1.dialogueFont, 800)).Y;

            LeftTextWidth = (int)MeasureString(leftText(), true).X;
            LeftTextHeight = (int)MeasureString(leftText(), true).Y;
        }

        // Measure the width and height of a string
        private static Vector2 MeasureString(string text, bool bold = false, float scale = 1f, SpriteFont? font = null)
        {
            return bold ? new Vector2(SpriteText.getWidthOfString(text) * scale, SpriteText.getHeightOfString(text) * scale) : (font ?? Game1.dialogueFont).MeasureString(text) * scale;
        }

        // Handle the button click event
        private static void OnClick(string fieldId)
        {
            double currentTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
            if (currentTime - lastClickTime >= ClickCooldown)
            {
                Click?.Invoke(new ButtonClickEventData(fieldId));
                lastClickTime = currentTime;
            }
            else
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
            bool isLeftHoveredNow = IsHovered(new Vector2(left, top), LeftTextWidth, LeftTextHeight);

            // Play hover sound effect if the button is hovered over
            if ((isRightHoveredNow && !wasRightHoveredPreviously) || (isLeftHoveredNow && !wasLeftHoveredPreviously))
            {
                Game1.playSound("shiny4");
            }

            isRightHovered = isRightHoveredNow;
            wasRightHoveredPreviously = isRightHoveredNow;

            isLeftHovered = isLeftHoveredNow;
            wasLeftHoveredPreviously = isLeftHoveredNow;

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

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (renderRight && hoverText?.Invoke() != null && isRightHovered)
                {
                    TooltipHelper.Hover = hoverText();
                }

                if ((!renderRight || !isRightHovered) && (!renderLeft || !isLeftHovered)) return;
                TooltipHelper.Title = leftText();
                TooltipHelper.Body = descText!();
            }
            catch (Exception e)
            {
                ConfigManager.Monitor?.Log($"Error in ButtonOptions Draw: {e}", LogLevel.Error);
            }
        }
    }
}