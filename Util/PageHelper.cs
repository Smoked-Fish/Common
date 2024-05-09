#nullable enable
using HarmonyLib;
using StardewModdingAPI;
using System;

namespace SplitScreenRegions.Framework.Patches
{
    internal class PageHelper
    {
        readonly Harmony _harmony;
        readonly IMonitor _monitor;

        public static Action<string>? OpenPage;
        public static string? CurrPage;
        internal PageHelper(Harmony harmony, IMonitor monitor)
        {
            _harmony = harmony;
            _monitor = monitor;
        }
        internal void Apply()
        {
            try
            {
                Type constructor = AccessTools.TypeByName("GenericModConfigMenu.Framework.SpecificModConfigMenu");
                Type[] parameters = [AccessTools.TypeByName("GenericModConfigMenu.Framework.ModConfig"), typeof(int), typeof(string), typeof(Action<string>), typeof(Action)];
                _harmony.Patch(
                    original: AccessTools.Constructor(constructor, parameters), 
                    postfix: new HarmonyMethod(typeof(PageHelper), nameof(SpecificModConfigMenuPostfix)));
            }
            catch (Exception e)
            {
                string errorMessage = $"Issue with Harmony patching GenericModConfigMenu.Framework.SpecificModConfigMenu: {e}";
                _monitor.Log(errorMessage, LogLevel.Error);
            }
        }


        private static void SpecificModConfigMenuPostfix(string page, Action<string> openPage)
        {
            CurrPage = page;
            OpenPage = openPage;
        }
    }
}
