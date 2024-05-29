using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    public static class ApiRegistry
    {
        private static readonly Dictionary<Type, object> _apisDictionary = [];
        private static IModHelper? _helper;
        private static IMonitor? _monitor;

        internal static void Init(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        /// <summary>
        /// Retrieves an instance of the specified API type from the mod registry.
        /// </summary>
        public static T? GetApi<T>(string apiName, bool logError = true) where T : class
        {
            if (_helper == null || _monitor == null)
            {
                throw new InvalidOperationException("ApiRegistry is not initialized.");
            }

            try
            {
                if (_apisDictionary.TryGetValue(typeof(T), out var api) && api is T typedApi)
                {
                    return typedApi;
                }

                api = _helper.ModRegistry.GetApi<T>(apiName);

                if (api == null)
                {
                    _monitor.Log($"Failed to hook into {apiName}.", logError ? LogLevel.Error : LogLevel.Trace);
                    return null;
                }

                _apisDictionary[typeof(T)] = api;
                _monitor.Log($"Successfully hooked into {apiName}.");
                return (T)api;
            }
            catch (Exception e)
            {
                _monitor.Log($"Failed to hook into the {apiName} API. Please check if the mod has an update: {e.Message}", LogLevel.Warn);
                return null;
            }
        }
    }
}