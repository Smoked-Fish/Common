using Common.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Reflection;

namespace Common.Managers
{
    public static class ConfigManager
    {
        private static IManifest _manifest;
        private static object _config;
        private static IMonitor _monitor;
        private static IGenericModConfigMenuApi _configApi;
        internal static ApiManager _apiManager;

        /// <summary>
        /// Initializes the configuration manager.
        /// </summary>
        public static void Initialize(IManifest manifest, object config, IModHelper helper, IMonitor monitor)
        {
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _monitor = monitor;
            _apiManager = new ApiManager(helper, monitor);
            _configApi = _apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            I18n.Init(helper.Translation);
        }

        /// <summary>
        /// Adds a configuration option to the mod config menu.
        /// </summary>
        public static void AddOption(string name)
        {
            if (_configApi == null)
            {
                _monitor?.LogOnce($"GenericModConfigMenu not found.", LogLevel.Debug);
                return;
            }

            if (_config == null)
            {
                _monitor?.Log($"Error: Configuration object is not initialized.", LogLevel.Error);
                return;
            }

            Type configType = _config.GetType();
            PropertyInfo propertyInfo = configType.GetProperty(name);
            if (propertyInfo == null)
            {
                _monitor.Log($"Error: Property '{name}' not found in configuration object.", LogLevel.Error);
                return;
            }

            string projectName = Assembly.GetEntryAssembly()?.GetName().Name;
            Func<string> getName = () => I18n.GetByKey($"Config.{projectName}.{name}.Name");
            Func<string> getDescription = () => I18n.GetByKey($"Config.{projectName}.{name}.Description");

            if (getName == null || getDescription == null)
            {
                _monitor.Log($"Error: Localization keys for '{name}' not found.", LogLevel.Error);
                return;
            }

            var getterMethod = propertyInfo.GetGetMethod();
            var setterMethod = propertyInfo.GetSetMethod();

            if (getterMethod == null || setterMethod == null)
            {
                _monitor.Log($"Error: The get/set methods are null for property '{name}'.", LogLevel.Error);
                return;
            }

            var getter = Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(propertyInfo.PropertyType), _config, getterMethod);
            var setter = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(propertyInfo.PropertyType), _config, setterMethod);

            switch (propertyInfo.PropertyType.Name)
            {
                case nameof(Boolean):
                    _configApi.AddBoolOption(_manifest, (Func<bool>)getter, (Action<bool>)setter, getName, getDescription);
                    break;
                case nameof(Int32):
                    _configApi.AddNumberOption(_manifest, (Func<int>)getter, (Action<int>)setter, getName, getDescription);
                    break;
                case nameof(Single):
                    _configApi.AddNumberOption(_manifest, (Func<float>)getter, (Action<float>)setter, getName, getDescription);
                    break;
                case nameof(String):
                    _configApi.AddTextOption(_manifest, (Func<string>)getter, (Action<string>)setter, getName, getDescription);
                    break;
                case nameof(SButton):
                    _configApi.AddKeybind(_manifest, (Func<SButton>)getter, (Action<SButton>)setter, getName, getDescription);
                    break;
                case nameof(KeybindList):
                    _configApi.AddKeybindList(_manifest, (Func<KeybindList>)getter, (Action<KeybindList>)setter, getName, getDescription);
                    break;
                default:
                    _monitor.Log($"Error: Unsupported property type '{propertyInfo.PropertyType.Name}' for '{name}'.", LogLevel.Error);
                    break;
            }
        }
    }
}
