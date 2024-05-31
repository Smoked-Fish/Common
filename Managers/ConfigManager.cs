using Common.Interfaces;
using Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Reflection;

namespace Common.Managers
{
    public static partial class ConfigManager
    {
        public static IGenericModConfigMenuApi? ConfigApi { get; private set; }
        private static IManifest? Manifest { get; set; }
        public static IMonitor? Monitor { get; private set; }
        public static string? ModNamespace { get; private set; }
        public static Action SaveAction { get; } = () =>
        {
            if (_helper != null && _config != null)
            {
                _helper.WriteConfig((dynamic)_config);
            }
        };

        // Fields
        private static IModHelper? _helper;
        private static IConfigurable? _config;

        public static void Init(IManifest manifest, IConfigurable config, IModHelper helper, IMonitor monitor)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            ModNamespace = Manifest.UniqueID.Split('.')[1];

            I18n.Init(helper.Translation);
            helper.Events.GameLoop.GameLaunched  += FinishInit;
        }

        private static void FinishInit(object? sender, GameLaunchedEventArgs e)
        {
            ApiRegistry.Init(_helper!, Monitor!);
            ConfigApi = ApiRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            ConfigApi?.Register(Manifest!, ResetAction, SaveAction);
        }

        public static void AddOption(string name, bool skipOption = false)
        {
            if (!AreConfigObjectsInitialized()) return;
            if (skipOption) return;

            PropertyInfo? propertyInfo = _config!.GetType().GetProperty(name);
            if (propertyInfo == null)
            {
                Monitor?.Log($"Error: Property '{name}' not found in configuration object.", LogLevel.Error);
                return;
            }

            if (string.IsNullOrEmpty(GetName()) || string.IsNullOrEmpty(GetDescription()))
            {
                Monitor?.Log($"Error: Localization keys for '{name}' not found.", LogLevel.Error);
                return;
            }

            switch (propertyInfo.PropertyType.Name)
            {
                case nameof(Boolean):
                    ConfigApi!.AddBoolOption(Manifest!, () => (bool)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                case nameof(Int32):
                    ConfigApi!.AddNumberOption(Manifest!, () => (int)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                case nameof(Single):
                    ConfigApi!.AddNumberOption(Manifest!, () => (float)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                case nameof(String):
                    ConfigApi!.AddTextOption(Manifest!, () => (string)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                case nameof(SButton):
                    ConfigApi!.AddKeybind(Manifest!, () => (SButton)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                case nameof(KeybindList):
                    ConfigApi!.AddKeybindList(Manifest!, () => (KeybindList)GetConfig(name), value => SetConfig(name, value), GetName, GetDescription);
                    break;
                default:
                    Monitor?.Log($"Error: Unsupported property type '{propertyInfo.PropertyType.Name}' for '{name}'.", LogLevel.Error);
                    break;
            }

            return;

            string GetName() => I18n.GetByKey($"Config.{ModNamespace}.{name}.Name");
            string GetDescription() => I18n.GetByKey($"Config.{ModNamespace}.{name}.Description");
        }

        public static void AddSectionTitle(string title, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddSectionTitle(Manifest!,
                () => I18n.GetByKey($"Config.{ModNamespace}.{title}.Title"),
                () => tooltip != null ? I18n.GetByKey($"Config.{ModNamespace}.{tooltip}.Description") : string.Empty);
        }

        public static void AddPageLink(string name, string? tooltip = null)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPageLink(Manifest!, name,
                () => $"> {I18n.GetByKey($"Config.{ModNamespace}.{name}.Title")}",
                () => tooltip != null ? I18n.GetByKey($"Config.{ModNamespace}.{name}.Description") : string.Empty);
        }

        public static void AddPage(string name)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.AddPage(Manifest!, name, () => I18n.GetByKey($"Config.{ModNamespace}.{name}.Title"));
        }

        public static void OnFieldChanged(Action<string, object> onChange)
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigApi!.OnFieldChanged(Manifest!, onChange);
        }

        private static bool AreConfigObjectsInitialized()
        {
            if (ConfigApi != null && _config != null && Manifest != null) return true;
            Monitor?.LogOnce("Error: Configuration objects not initialized.", LogLevel.Error);
            return false;
        }

        private static readonly Action<string, object> SetConfig = (propertyName, newValue) =>
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigUtility.SetConfig(_config!, propertyName, newValue);
        };

        private static readonly Func<string, object> GetConfig = (propertyName) =>
        {
            if (!AreConfigObjectsInitialized()) throw new ArgumentNullException(propertyName);

            return ConfigUtility.GetConfig(_config!, propertyName) ?? throw new InvalidOperationException();
        };

        private static readonly Action ResetAction = () =>
        {
            if (!AreConfigObjectsInitialized()) return;

            ConfigUtility.InitializeDefaultConfig(_config!);
        };
    }
}
