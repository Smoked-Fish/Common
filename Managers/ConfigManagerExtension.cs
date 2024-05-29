#nullable enable
using Common.Helpers;
using Common.Interfaces;
using Common.Utilities;
using Common.Utilities.Options;
using StardewModdingAPI;
using System;

namespace Common.Managers
{
    public static partial class ConfigManager
    {
        public static void Initialize(IManifest manifest, IConfigurable config, IModHelper helper, IMonitor monitor, bool enablePatches)
        {
            Initialize(manifest, config, helper, monitor);
            EnablePatches();
        }

        public static void AddButtonOption(
            string leftText, string? rightText = null, string? descText = null, string? hoverText = null,
            bool renderLeft = false, bool renderRight = false, string? fieldId = null, Action? afterReset = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            rightText ??= leftText;
            descText ??= leftText;
            hoverText ??= leftText;

            var buttonOption = new ButtonOptions(LeftTextLocalized, RightTextLocalized, DescTextLocalized, HoverTextLocalized, renderLeft, renderRight, fieldId);

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
                draw: buttonOption.Draw,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: afterReset,
                height: () => buttonOption.RightTextHeight,
                fieldId: fieldId);
            return;

            string LeftTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{leftText}.Title");
            string RightTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{rightText}.Button");
            string DescTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{descText}.Description");
            string HoverTextLocalized() => I18n.GetByKey($"Config.{ModNamespace}.{hoverText}.Hover");
        }

        public static void AddHorizontalSeparator()
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddComplexOption(
                mod: Manifest!,
                name: () => string.Empty,
                draw: SeparatorOptions.Draw);
        }

        private static void EnablePatches()
        {
            if (ConfigApi == null) return;
            new PageHelper().Apply();
            new TooltipHelper().Apply();
        }
    }
}