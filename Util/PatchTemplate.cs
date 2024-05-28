#if EnableHarmony
#nullable enable
using HarmonyLib;
using System.Reflection;
using System;

namespace Common.Util
{
    internal class PatchTemplate
    {
        internal static Harmony? _harmony;
        internal static Type? _object;

        internal PatchTemplate(Harmony harmonyInstance, Type? objectType = null)
        {
            _harmony = harmonyInstance ?? throw new ArgumentNullException(nameof(harmonyInstance), "Harmony instance cannot be null.");
            _object = objectType;
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        public void Patch(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
        {
            try
            {
                MethodInfo targetMethod = _object != null ? AccessTools.Method(_object, originalMethod, parameters) : AccessTools.Method(originalMethod, parameters);
                HarmonyMethod harmonyMethod = new HarmonyMethod(GetType(), newMethod);

                ApplyPatch(patchType, targetMethod, harmonyMethod);
            }
            catch (Exception e)
            {
                throw new Exception($"Error occurred while patching method {originalMethod} with {newMethod}: {e.Message}", e);
            }
        }

        /// <summary>
        /// Applies constructor patches using Harmony for a specified target method.
        /// </summary>
        public void ConstructorPatch(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
        {
            try
            {
                ConstructorInfo targetConstructor = AccessTools.Constructor(AccessTools.TypeByName(originalMethod), parameters);
                HarmonyMethod harmonyMethod = new HarmonyMethod(GetType(), newMethod);

                ApplyPatch(patchType, targetConstructor, harmonyMethod);
            }
            catch (Exception e)
            {
                throw new Exception($"Error occurred while patching constructor {originalMethod} with {newMethod}: {e.Message}", e);
            }
        }

        private static void ApplyPatch(PatchType patchType, MethodBase targetMethod, HarmonyMethod harmonyMethod)
        {
            if (_harmony == null)
            {
                throw new InvalidOperationException("Harmony instance is null. Make sure to initialize PatchTemplate with a valid Harmony instance.");
            }

            switch (patchType)
            {
                case PatchType.Prefix:
                    _harmony.Patch(targetMethod, prefix: harmonyMethod);
                    break;
                case PatchType.Postfix:
                    _harmony.Patch(targetMethod, postfix: harmonyMethod);
                    break;
                case PatchType.Transpiler:
                    _harmony.Patch(targetMethod, transpiler: harmonyMethod);
                    break;
                default:
                    throw new ArgumentException($"Unknown patch type: {patchType}", nameof(patchType));
            }
        }

        public enum PatchType
        {
            Prefix,
            Postfix,
            Transpiler
        }
    }
}
#endif