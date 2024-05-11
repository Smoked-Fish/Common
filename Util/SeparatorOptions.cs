#nullable enable
#if EnableCommonPatches
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace Common.Util
{
    internal class SeparatorOptions
    {
        private const int SeparatorHeight = 3;

        // Static method to create an empty texture
        private static Texture2D CreateEmptyTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D texture = new(graphicsDevice, 1, 1);
            Color[] pixels = [Color.White];
            texture.SetData(pixels);
            return texture;
        }

        // Draw method to draw the separator
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            // Calculate position and width
            int top = (int)position.Y;
            int usableWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
            int left = (Game1.uiViewport.Width - usableWidth) / 2;

            // Draw separator
            spriteBatch.Draw(CreateEmptyTexture(Game1.graphics.GraphicsDevice), new Rectangle(left, top, usableWidth, SeparatorHeight), Game1.textColor);
        }
    }
}
#endif