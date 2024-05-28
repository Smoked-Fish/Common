#nullable enable
using Common.Helpers;
using Common.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Reflection;

namespace Common.Managers
{
    public static partial class ConfigManager
    {
        // Properties
        public static IGenericModConfigMenuApi? ConfigApi { get; private set; }
        public static IManifest? Manifest { get; private set; }
        public static IMonitor? Monitor { get; private set; }
        public static string? ModNamespace { get; set; }

        // Fields
        private static IModHelper? _helper;
        private static IConfigurable? _config;

        public static void Initialize(IManifest manifest, IConfigurable config, IModHelper helper, IMonitor monitor, object? harmony = null)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            ModNamespace = Manifest.UniqueID.Split('.')[1];

            ApiManager apiManager = new(helper, monitor);
            ConfigApi = apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            ConfigApi?.Register(manifest, resetAction, saveAction);

            I18n.Init(helper.Translation);

            #if EnableCommonPatches
            EnablePatches(harmony);
            #endif
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

            Func<string> getName = () => I18n.GetByKey($"Config.{ModNamespace}.{name}.Name");
            Func<string> getDescription = () => I18n.GetByKey($"Config.{ModNamespace}.{name}.Description");

            if (getName == null || getDescription == null)
            {
                Monitor?.Log($"Error: Localization keys for '{name}' not found.", LogLevel.Error);
                return;
            }

            switch (propertyInfo.PropertyType.Name)
            {
                case nameof(Boolean):
                    ConfigApi!.AddBoolOption(Manifest!, () => (bool)getConfig(name), (bool value) => setConfig(name, value), getName, getDescription);
                    break;
                case nameof(Int32):
                    ConfigApi!.AddNumberOption(Manifest!, () => (int)getConfig(name), (int value) => setConfig(name, value), getName, getDescription);
                    break;
                case nameof(Single):
                    ConfigApi!.AddNumberOption(Manifest!, () => (float)getConfig(name), (float value) => setConfig(name, value), getName, getDescription);
                    break;
                case nameof(String):
                    ConfigApi!.AddTextOption(Manifest!, () => (string)getConfig(name), (string value) => setConfig(name, value), getName, getDescription);
                    break;
                case nameof(SButton):
                    ConfigApi!.AddKeybind(Manifest!, () => (SButton)getConfig(name), (SButton value) => setConfig(name, value), getName, getDescription);
                    break;
                case nameof(KeybindList):
                    ConfigApi!.AddKeybindList(Manifest!, () => (KeybindList)getConfig(name), (KeybindList value) => setConfig(name, value), getName, getDescription);
                    break;
                default:
                    Monitor?.Log($"Error: Unsupported property type '{propertyInfo.PropertyType.Name}' for '{name}'.", LogLevel.Error);
                    break;
            }
        }

        public static void AddSectionTitle(string title, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddSectionTitle(Manifest!,
                () => I18n.GetByKey($"Config.{ModNamespace}.{title}.Title") ?? title,
                () => tooltip != null ? I18n.GetByKey($"Config.{ModNamespace}.{tooltip}.Description") ?? tooltip : null!);
        }

        public static void AddPageLink(string name, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPageLink(Manifest!, name,
                () => string.Concat("> ", I18n.GetByKey($"Config.{ModNamespace}.{name}.Title") ?? name),
                () => tooltip != null ? I18n.GetByKey($"Config.{ModNamespace}.{name}.Description") ?? name : null!);
        }

        public static void AddPage(string name)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPage(Manifest!, name, () => I18n.GetByKey($"Config.{ModNamespace}.{name}.Title") ?? name);
        }

        public static void OnFieldChanged(Action<string, object> onChange)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.OnFieldChanged(Manifest!, onChange);
        }

        // Helper Methods
        private static bool AreConfigObjectsInitialized()
        {
            if (ConfigApi == null || _config == null || Manifest == null)
            {
                Monitor?.LogOnce($"Error: Configuration objects not initialized.", LogLevel.Error);
                return false;
            }
            return true;
        }

        private static readonly Action<string, object> setConfig = (propertyName, newValue) =>
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigUtilities.SetConfig(_config, propertyName, newValue);
        };

        private static readonly Func<string, object> getConfig = (propertyName) =>
        {
            if (!AreConfigObjectsInitialized()) throw new ArgumentNullException(propertyName);

            return ConfigUtilities.GetConfig(_config, propertyName);
        };

        private static readonly Action resetAction = () =>
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigUtilities.InitializeDefaultConfig(_config);
        };

        private static readonly Action saveAction = () =>
        {
            if (_helper != null && _config != null)
            {
                _helper.WriteConfig((dynamic)_config);
            }
        };
    }
}
