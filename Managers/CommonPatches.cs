#if EnableCommonPatches
#nullable enable
using Common.Util;
using HarmonyLib;
using System;

namespace Common.Managers
{
    public static partial class ConfigManager
    {
        public static void AddButtonOption(string leftText, string rightText, string? fieldId = null, Action? afterReset = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            Func<string> leftTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{leftText}.Title");
            Func<string> rightTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{rightText}.Button");
            //Func<string>? hoverTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{hoverText}.Description");

            var buttonOption = new ButtonOptions(leftText: leftTextLocalized, rightText: rightTextLocalized, fieldID: fieldId);

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => "",
                draw: buttonOption.Draw,
                height: () => buttonOption.RightTextHeight,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: () => { afterReset?.Invoke(); },
                fieldId: fieldId
            );
        }

        public static void AddHorizontalSeparator()
        {
            if (!AreConfigObjectsInitialized()) return;

            var separatorOption = new SeparatorOptions();
            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => "",
                draw: separatorOption.Draw);
        }
        
        private static void EnablePatches(object? harmony)
        {
            if (ConfigApi != null && harmony is Harmony realHarmony)
            {
                new PageHelper(realHarmony).Apply();
                new TooltipHelper(realHarmony).Apply();
            }
        }
    }
}
#endif