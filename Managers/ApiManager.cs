using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace Common.Managers
{
    internal class ApiManager
    {
        private readonly Dictionary<Type, object> _apis;
        IModHelper _helper;
        IMonitor _monitor;

        public ApiManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _apis = new Dictionary<Type, object>();
        }

        public T GetApi<T>(string apiName, bool logError = true) where T : class
        {
            try
            {
                if (_apis.TryGetValue(typeof(T), out var api) && api is T typedApi)
                {
                    return typedApi;
                }

                api = _helper.ModRegistry.GetApi<T>(apiName);

                if (api == null)
                {
                    if (logError)
                    {
                        _monitor.Log($"Failed to hook into {apiName}.", LogLevel.Error);
                    }
                    else
                    {
                        _monitor.Log($"Failed to hook into {apiName}.", LogLevel.Trace);
                    }
                    return null;
                }

                _apis[typeof(T)] = api;
                _monitor.Log($"Successfully hooked into {apiName}.", LogLevel.Trace);
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
