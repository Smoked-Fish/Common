#nullable enable
using StardewModdingAPI;
using StardewValley;
using System;

namespace Common.Managers
{
    public static class I18n
    {
        private static ITranslationHelper? _translations;

        internal static void Init(ITranslationHelper translations)
        {
            _translations = translations;
        }

        public static Translation Message(string key, object? tokens = null)
        {
            if (_translations == null)
                throw new InvalidOperationException("I18n is not initialized.");

            return GetByKey($"Message.{ConfigManager.ModNamespace}.{key}", tokens);
        }

        public static Translation GetByKey(string key, object? tokens = null)
        {
            if (_translations == null)
                throw new InvalidOperationException("I18n is not initialized.");

            return _translations.Get(key, tokens);
        }
    }
}
