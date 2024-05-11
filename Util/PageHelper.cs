#nullable enable
#if EnableCommonPatches
using HarmonyLib;
using StardewModdingAPI;
using System;

namespace Common.Util
{
    public class PageHelper
    {

        public static Action<string>? OpenPage { get; set; }
        public static string? CurrPage { get; set; }

        private readonly Harmony _harmony;
        private readonly IMonitor _monitor;
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
#endif