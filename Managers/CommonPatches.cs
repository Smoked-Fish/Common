#if EnableCommonPatches
#nullable enable
using Common.Util;
using HarmonyLib;
using System;

namespace Common.Managers
{
    public static partial class ConfigManager
    {
        public static void AddButtonOption(
            string leftText, string? rightText = null, string? descText = null, string? hoverText = null, 
            bool renderLeft = false, bool renderRight = false, string? fieldId = null, Action? afterReset = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            rightText ??= leftText;
            descText ??= leftText;
            hoverText ??= leftText;

            Func<string> leftTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{leftText}.Title");
            Func<string> rightTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{rightText}.Button");
            Func<string>? descTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{descText}.Description");
            Func<string>? hoverTextLocalized = () => I18n.GetByKey($"Config.{ModNamespace}.{hoverText}.Hover");

            var buttonOption = new ButtonOptions(leftTextLocalized, rightTextLocalized, descTextLocalized, hoverTextLocalized, renderLeft, renderRight, fieldId);

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
                draw: buttonOption.Draw,
                height: () => buttonOption.RightTextHeight,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: afterReset,
                fieldId: fieldId);
        }

        public static void AddHorizontalSeparator()
        {
            if (!AreConfigObjectsInitialized()) return;

            var separatorOption = new SeparatorOptions();
            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
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