﻿#if EnableCommonPatches
#nullable enable
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;

namespace Common.Util
{
    internal class TooltipHelper : PatchTemplate
    {
        public static string? Title { get; set; }
        public static string? Body { get; set; }
        public static string? Hover { get; set; }

        internal TooltipHelper(Harmony harmony) : base(harmony) { }

        public void Apply()
        {
            Patch(PatchType.Postfix, "GenericModConfigMenu.Framework.SpecificModConfigMenu:draw", nameof(DrawPostfix), [typeof(SpriteBatch)]);
        }

        private static void DrawPostfix(SpriteBatch b)
        {
            /*ConfigManager.ConfigApi!.TryGetCurrentMenu(out IManifest manifest, out string _);
            if (manifest.UniqueID != ConfigManager.Manifest!.UniqueID) return;*/

            var title = Title;
            var text = Body;
            var hover = Hover;
            if (hover != null)
            {
                if (!hover.Contains('\n')) text = Game1.parseText(text, Game1.smallFont, 800);
                IClickableMenu.drawHoverText(b, text, Game1.smallFont);
            }
            else if (title is not null && text is not null)
            {
                if (!text.Contains('\n')) text = Game1.parseText(text, Game1.smallFont, 800);
                if (!title.Contains('\n')) title = Game1.parseText(title, Game1.dialogueFont, 800);
                IClickableMenu.drawHoverText(b, text, Game1.smallFont);
            }

            Title = null;
            Body = null;
            Hover = null;
        }
    }
}
#endif