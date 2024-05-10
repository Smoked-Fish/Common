﻿#nullable enable
using Common.Interfaces;
using Common.Util;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Reflection;

namespace Common.Managers
{
    public static class TranslationHelper
    {
        private static ITranslationHelper? _translations;

        internal static void Init(ITranslationHelper translations)
        {
            _translations = translations;
        }

        public static Translation GetByKey(string key)
        {
            if (_translations == null)
            {
                throw new InvalidOperationException("TranslationHelper is not initialized.");
            }
            return _translations.Get(key);
        }
    }

    public static class ConfigManager
    {
        // Fields
        private static IManifest? _manifest;
        private static object? _config;

        // Properties
        public static IGenericModConfigMenuApi? ConfigApi { get; set; }
        public static IMonitor? Monitor { get; set; }

        public static void Initialize(IManifest manifest, object config, IModHelper helper, IMonitor monitor, Harmony harmony, bool? runPatches = false)
        {
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Monitor = monitor;
            ApiManager apiManager = new(helper, monitor);
            ConfigApi = apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            ConfigApi?.Register(manifest, resetAction, () => helper.WriteConfig(_config));

            if (ConfigApi != null && runPatches == true)
            {
                new PageHelper(harmony, monitor).Apply();
                new TooltipHelper(harmony, monitor).Apply();
            }

            TranslationHelper.Init(helper.Translation);
        }

        public static void AddOption(string name)
        {
            if (!AreConfigObjectsInitialized()) return;

            PropertyInfo? propertyInfo = _config!.GetType().GetProperty(name);
            if (propertyInfo == null)
            {
                Monitor?.Log($"Error: Property '{name}' not found in configuration object.", LogLevel.Error);
                return;
            }

            Func<string> getName = () => TranslationHelper.GetByKey($"Config.{_config.GetType().Namespace}.{name}.Name");
            Func<string> getDescription = () => TranslationHelper.GetByKey($"Config.{_config.GetType().Namespace}.{name}.Description");

            if (getName == null || getDescription == null)
            {
                Monitor?.Log($"Error: Localization keys for '{name}' not found.", LogLevel.Error);
                return;
            }

            var getterMethod = propertyInfo.GetGetMethod();
            var setterMethod = propertyInfo.GetSetMethod();

            if (getterMethod == null || setterMethod == null)
            {
                Monitor?.Log($"Error: The get/set methods are null for property '{name}'.", LogLevel.Error);
                return;
            }

            var getter = Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(propertyInfo.PropertyType), _config, getterMethod);
            var setter = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(propertyInfo.PropertyType), _config, setterMethod);

            switch (propertyInfo.PropertyType.Name)
            {
                case nameof(Boolean):
                    ConfigApi!.AddBoolOption(_manifest!, (Func<bool>)getter, (Action<bool>)setter, getName, getDescription);
                    break;
                case nameof(Int32):
                    ConfigApi!.AddNumberOption(_manifest!, (Func<int>)getter, (Action<int>)setter, getName, getDescription);
                    break;
                case nameof(Single):
                    ConfigApi!.AddNumberOption(_manifest!, (Func<float>)getter, (Action<float>)setter, getName, getDescription);
                    break;
                case nameof(String):
                    ConfigApi!.AddTextOption(_manifest!, (Func<string>)getter, (Action<string>)setter, getName, getDescription);
                    break;
                case nameof(SButton):
                    ConfigApi!.AddKeybind(_manifest!, (Func<SButton>)getter, (Action<SButton>)setter, getName, getDescription);
                    break;
                case nameof(KeybindList):
                    ConfigApi!.AddKeybindList(_manifest!, (Func<KeybindList>)getter, (Action<KeybindList>)setter, getName, getDescription);
                    break;
                default:
                    Monitor?.Log($"Error: Unsupported property type '{propertyInfo.PropertyType.Name}' for '{name}'.", LogLevel.Error);
                    break;
            }
        }

        public static void AddSectionTitle(string title, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddSectionTitle(_manifest!, 
                () => TranslationHelper.GetByKey($"Config.{_config!.GetType().Namespace}.{title}.Title") ?? title,
                () => tooltip != null ? TranslationHelper.GetByKey($"Config.{_config!.GetType().Namespace}.{tooltip}.Description") ?? tooltip : null!);
        }

        public static void AddPageLink(string name, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPageLink(_manifest!, name, 
                () => string.Concat("> ", TranslationHelper.GetByKey($"Config.{_config!.GetType().Namespace}.{name}.Title") ?? name),
                () => tooltip != null ? TranslationHelper.GetByKey($"Config.{_config!.GetType().Namespace}.{name}.Description") ?? name : null!);
        }

        public static void AddPage(string name)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPage(_manifest!, name, () => TranslationHelper.GetByKey($"Config.{_config!.GetType().Namespace}.{name}.Title") ?? name);
        }


        public static void AddButtonOption(string leftText, string rightText, string? fieldId = null, bool rightHover = false, bool leftHover = false, string? hoverText = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            var buttonOption = new ButtonOptions(leftText: leftText, rightText: rightText, fieldID: fieldId, rightHover: rightHover, leftHover: leftHover, hoverText: hoverText);

            ConfigApi!.AddComplexOption(
                mod: _manifest!,
                name: () => "",
                draw: buttonOption.Draw,
                height: () => buttonOption.RightTextHeight,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: () => { },
                fieldId: fieldId
            );
        }

        public static void AddHorizontalSeparator()
        {
            if (!AreConfigObjectsInitialized()) return;

            var separatorOption = new SeparatorOptions();
            ConfigApi!.AddComplexOption(
                mod: _manifest!,
                name: () => "",
                draw: separatorOption.Draw);
        }

        // Helper Methods
        private static bool AreConfigObjectsInitialized()
        {
            if (ConfigApi == null || _config == null || _manifest == null)
            {
                Monitor?.LogOnce($"Error: Configuration objects not initialized.", LogLevel.Error);
                return false;
            }
            return true;
        }

        private static readonly Action resetAction = () =>
        {
            MethodInfo? resetMethod = _config?.GetType().GetMethod("InitializeDefaultConfig", BindingFlags.Public | BindingFlags.Instance);
            if (resetMethod != null)
            {
                // Invoke ResetConfig method
                resetMethod.Invoke(_config, null);
            }
            else
            {
                Monitor?.Log("Error: ResetConfig method not found.", LogLevel.Error);
            }
        };
    }
}
