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

            Func<string> leftTextLocalized = () => I18n.GetByKey($"Config.{_config!.GetType().Namespace}.{leftText}.Title");
            Func<string> rightTextLocalized = () => I18n.GetByKey($"Config.{_config!.GetType().Namespace}.{rightText}.Button");
            //Func<string>? hoverTextLocalized = () => I18n.GetByKey($"Config.{_config!.GetType().Namespace}.{hoverText}.Description");

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
            if (ConfigApi != null && harmony is Harmony realHarmony && Monitor != null)
            {
                new PageHelper(realHarmony, Monitor).Apply();
                new TooltipHelper(realHarmony, Monitor).Apply();
            }
        }
    }
}
#endif