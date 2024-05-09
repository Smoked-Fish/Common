﻿#nullable enable
using Common.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SplitScreenRegions.Framework.Patches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace SplitScreenRegions.Framework.UI
{
    public class ButtonClickEventArgs(string fieldID)
    {
        public string FieldID { get; } = fieldID;
    }

    public class ButtonOptions
    {
        // Events
        private static Action<ButtonClickEventArgs>? click;

        // Fields
        private readonly string leftText;
        private readonly string rightText;
        private readonly string fieldID;
        private readonly string? hoverText;
        private readonly bool renderRightHover;
        private readonly bool renderLeftHover;
        private bool isRightHovered = false;
        private bool wasRightHoveredPreviously = false;
        private bool isLeftHovered = false;
        private bool wasLeftHoveredPreviously = false;
        private ButtonState lastButtonState;
        private (int Top, int Left) storedValues;
        private const double ClickCooldown = 0.1;
        private static double lastClickTime = 0;

        // Constructor
        public ButtonOptions(string leftText = "", string rightText = "", string? fieldID = null, bool rightHover = false, bool leftHover = false, string? hoverText = null)
        {
            this.leftText = leftText;
            this.rightText = rightText;
            this.fieldID = fieldID ?? "";
            this.renderRightHover = rightHover;
            this.renderLeftHover = leftHover;
            this.hoverText = hoverText;
            CalculateTextMeasurements();
        }

        // Properties
        public int RightTextWidth { get; private set; }
        public int RightTextHeight { get; private set; }
        public int LeftTextWidth { get; private set; }
        public int LeftTextHeight { get; private set; }
        public static Action<ButtonClickEventArgs>? Click { get => click; set => click = value; }


        // Private Methods
        // Calculate the width and height of the text for drawing
        private void CalculateTextMeasurements()
        {
            RightTextWidth = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText, Game1.dialogueFont, 800)).X;
            RightTextHeight = (int)Game1.dialogueFont.MeasureString(Game1.parseText(rightText, Game1.dialogueFont, 800)).Y;
            LeftTextWidth = (int)MeasureString(leftText).X;
            LeftTextHeight = (int)MeasureString(leftText).Y;
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
                Click?.Invoke(new ButtonClickEventArgs(fieldId));
                lastClickTime = currentTime;
            }
        }

        // Check if the mouse is hovering over the button
        private static bool IsHovered(int drawX, int drawY, int width, int height)
        {
            int mouseX = Mouse.GetState().X;
            int mouseY = Mouse.GetState().Y;
            return drawX <= mouseX && mouseX <= drawX + width && drawY <= mouseY && mouseY <= drawY + height;
        }

        // Update the mouse state and handle button hover sound effects
        private void UpdateMouseState(int drawX, int drawY)
        {
            ButtonState buttonState = Mouse.GetState().LeftButton;
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // Check if the button is clicked
            if (buttonState == ButtonState.Pressed && drawX <= mouseX && mouseX <= drawX + RightTextWidth && drawY <= mouseY && mouseY <= drawY + RightTextHeight && lastButtonState == ButtonState.Released)
            {
                OnClick(fieldID);
            }

            lastButtonState = buttonState;

            // Calculate the position of the button and check for hover effect
            int gmcmWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
            int gmcmLeft = (Game1.uiViewport.Width - gmcmWidth) / 2;
            int top = (drawY);
            int left = gmcmLeft;

            bool isRightHoveredNow = IsHovered(drawX, drawY, RightTextWidth, RightTextHeight);

            // Play hover sound effect if the button is hovered over
            if (isRightHoveredNow && !wasRightHoveredPreviously)
            {
                Game1.playSound("shiny4");
            }

            bool isLeftHoveredNow = IsHovered(drawX, drawY, LeftTextWidth, LeftTextHeight);

            isRightHovered = isRightHoveredNow;
            wasRightHoveredPreviously = isRightHoveredNow;

            isLeftHovered = isLeftHoveredNow;
            wasLeftHoveredPreviously = isLeftHoveredNow;

            storedValues = (top, left);
        }

        // Public Methods
        // Draw the button with hover effect
        public void Draw(SpriteBatch b, int posX, int posY)
        {
            try
            {
                UpdateMouseState(posX, posY);

                Color rightTextColor = isRightHovered ? Game1.unselectedOptionColor : Game1.textColor;
                Vector2 rightTextPosition = new(posX, posY);
                Utility.drawTextWithShadow(b, rightText, Game1.dialogueFont, rightTextPosition, rightTextColor);

                Vector2 leftTextPosition = new(storedValues.Left - 8, storedValues.Top);
                SpriteText.drawString(b, leftText, (int)leftTextPosition.X, (int)leftTextPosition.Y, layerDepth: 1f, color: new Color?());

                if (renderRightHover && isRightHovered)
                {
                    if (hoverText != null)
                    {
                        TooltipHelper.Hover = hoverText;
                    }
                    else
                    {
                        TooltipHelper.Title = leftText;
                        TooltipHelper.Body = rightText;
                    }
                }

                if (renderLeftHover && isLeftHovered)
                {
                    if (hoverText != null)
                    {
                        TooltipHelper.Hover = hoverText;
                    }
                    else
                    {
                        TooltipHelper.Title = leftText;
                        TooltipHelper.Body = rightText;
                    }
                }
            }
            catch (Exception e)
            {
                ConfigManager.Monitor?.Log($"Error in ButtonOptions Draw: {e}", LogLevel.Error);
            }
        }
    }
}
