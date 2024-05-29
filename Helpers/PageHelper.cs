using HarmonyLib;
using System;

namespace Common.Helpers
{
    public sealed class PageHelper : PatchHelper
    {
        public static Action<string>? OpenPage { get; private set; }
        public static string? CurrPage { get; private set; }

        public void Apply()
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