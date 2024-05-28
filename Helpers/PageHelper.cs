#if EnableCommonPatches
#nullable enable
using HarmonyLib;
using System;

namespace Common.Helpers
{
    internal sealed class PageHelper : PatchHelper
    {
        public static Action<string>? OpenPage { get; set; }
        public static string? CurrPage { get; set; }
        internal PageHelper(Harmony harmony) : base(harmony) { }
        internal void Apply()
        {
            Type[] parameters = [AccessTools.TypeByName("GenericModConfigMenu.Framework.ModConfig"), typeof(int), typeof(string), typeof(Action<string>), typeof(Action)];
            ConstructorPatch(PatchType.Postfix, "GenericModConfigMenu.Framework.SpecificModConfigMenu", nameof(SpecificModConfigMenuPostfix), parameters);
        }

        private static void SpecificModConfigMenuPostfix(string page, Action<string> openPage)
        {
            CurrPage = page;
            OpenPage = openPage;
        }
    }
}
#endif