#nullable enable
#if EnableCommonPatches
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System;

namespace Common.Util
{
    public class TooltipHelper
    {
        public static string? Title { get; set; }
        public static string? Body { get; set; }
        public static string? Hover { get; set; }

        private readonly Harmony _harmony;
        private readonly IMonitor _monitor;

        internal TooltipHelper(Harmony harmony, IMonitor monitor)
        {
            _harmony = harmony;
            _monitor = monitor;
        }

        public void Apply()
        {
            try
            {
                string method = "GenericModConfigMenu.Framework.SpecificModConfigMenu:draw";
                Type[] parameters = [typeof(SpriteBatch)];

                _harmony.Patch(
                    original: AccessTools.Method(method, parameters),
                    postfix: new HarmonyMethod(typeof(TooltipHelper), nameof(DrawPostfix)));
            }
            catch (Exception e)
            {
                string errorMessage = $"Issue with Harmony patching GenericModConfigMenu.Framework.SpecificModConfigMenu:draw: {e}";
                _monitor.Log(errorMessage, LogLevel.Error);
            }
        }

        private static void DrawPostfix(SpriteBatch b)
        {
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