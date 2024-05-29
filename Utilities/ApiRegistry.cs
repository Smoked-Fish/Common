using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    public class ApiRegistry(IModHelper helper, IMonitor monitor)
    {
        private readonly Dictionary<Type, object> _apis = [];

        /// <summary>
        /// Retrieves an instance of the specified API type from the mod registry.
        /// </summary>
        public T? GetApi<T>(string apiName, bool logError = true) where T : class
        {
            try
            {
                if (_apis.TryGetValue(typeof(T), out var api) && api is T typedApi)
                {
                    return typedApi;
                }

                api = helper.ModRegistry.GetApi<T>(apiName);

                if (api == null)
                {
                    monitor.Log($"Failed to hook into {apiName}.", logError ? LogLevel.Error : LogLevel.Trace);
                    return null;
                }

                _apis[typeof(T)] = api;
                monitor.Log($"Successfully hooked into {apiName}.");
                return (T)api;
            }
            catch (Exception e)
            {
                monitor.Log($"Failed to hook into the {apiName} API. Please check if the mod has an update: {e.Message}", LogLevel.Warn);
                return null;
            }
        }
    }
}
