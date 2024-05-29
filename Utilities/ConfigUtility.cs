using Common.Interfaces;
using Common.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common.Utilities
{
    public static class ConfigUtility
    {
        public static event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

        public static void InitializeDefaultConfig(IConfigurable config, string? category = null)
        {
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                if (property.GetCustomAttribute(typeof(DefaultValueAttribute)) is not DefaultValueAttribute defaultValueAttribute) continue;

                object? defaultValue = defaultValueAttribute.Value;
                if (category != null && defaultValueAttribute.Category != category) continue;

                if (property.PropertyType == typeof(KeybindList) && defaultValue is SButton button)
                {
                    defaultValue = new KeybindList(button);
                }

                // Handle list default value
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Create a new instance of List<T> where T is the generic argument of the property type
                    Type elementType = property.PropertyType.GetGenericArguments()[0];
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    defaultValue = Activator.CreateInstance(listType);
                }

                if (defaultValue == null || config == null) continue;
                OnConfigChanged(config, property.Name, property.GetValue(config), defaultValue);
                property.SetValue(config, defaultValue);
            }

            ConfigManager.SaveAction.Invoke();
        }

        public static void SetConfig(IConfigurable config, string propertyName, object value)
        {
            PropertyInfo? property = config.GetType().GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    object convertedValue = Convert.ChangeType(value, property.PropertyType);
                    OnConfigChanged(config, property.Name, property.GetValue(config), convertedValue);
                    property.SetValue(config, convertedValue);
                }
                catch (Exception ex)
                {
                    ConfigManager.Monitor?.Log($"Error setting property '{propertyName}': {ex.Message}", LogLevel.Error);
                }
            }
            else
            {
                ConfigManager.Monitor?.Log($"Property '{propertyName}' not found in config.", LogLevel.Error);
            }
        }

        public static object? GetConfig(IConfigurable config, string propertyName)
        {
            PropertyInfo? property = config.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(config);
            }

            ConfigManager.Monitor?.Log($"Property '{propertyName}' not found in config.", LogLevel.Error);
            return null;
        }

        private static void OnConfigChanged(IConfigurable config, string propertyName, object? oldValue, object newValue)
        {
            ConfigChanged?.Invoke(config, new ConfigChangedEventArgs(propertyName, oldValue, newValue));
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute(object? value, string? category = null) : Attribute
    {
        public object? Value { get; } = value;
        public string? Category { get; } = category;
    }

    public class ConfigChangedEventArgs(string configName, object? oldValue, object newValue) : EventArgs
    {
        public string ConfigName { get; } = configName;
        public object? OldValue { get; } = oldValue;
        public object NewValue { get; } = newValue;
    }
}
