#if EnableCommonPatches
#nullable enable
using Common.Utilities.Options;
using Common.Helpers;
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

            string leftTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{leftText}.Title");
            string rightTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{rightText}.Button");
            string descTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{descText}.Description");
            string hoverTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{hoverText}.Hover");

            var buttonOption = new ButtonOptions(leftTextLocalized, rightTextLocalized, descTextLocalized, hoverTextLocalized, renderLeft, renderRight, fieldId);

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
                draw: buttonOption.Draw,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: afterReset,
                height: () => buttonOption.RightTextHeight,
                fieldId: fieldId);
        }

        public static void AddHorizontalSeparator()
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
                draw: SeparatorOptions.Draw);
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