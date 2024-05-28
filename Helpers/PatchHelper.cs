#if EnableHarmony
#nullable enable
using HarmonyLib;
using Common.Utilities.Exceptions;
using System.Reflection;
using System.ComponentModel;
using System;

namespace Common.Helpers
{
    public class PatchHelper
    {
        internal static Harmony? _harmony;
        internal static Type? _object;

        internal PatchHelper(Harmony harmonyInstance, Type? objectType = null)
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
                HarmonyMethod harmonyMethod = new(GetType(), newMethod);

                ApplyPatch(patchType, targetMethod, harmonyMethod);
            }
            catch (Exception ex)
            {
                throw new PatchingException(originalMethod, newMethod, ex);
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
                HarmonyMethod harmonyMethod = new(GetType(), newMethod);

                ApplyPatch(patchType, targetConstructor, harmonyMethod);
            }
            catch (Exception ex)
            {
                throw new PatchingException(originalMethod, newMethod, ex);
            }
        }

        private static void ApplyPatch(PatchType patchType, MethodBase targetMethod, HarmonyMethod harmonyMethod)
        {
            if (_harmony == null)
            {
                throw new InstanceNullException("Harmony instance is null. Make sure to initialize PatchTemplate with a valid Harmony instance.");
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
                    throw new InvalidEnumArgumentException($"Unknown enum PatchType: {patchType}");
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