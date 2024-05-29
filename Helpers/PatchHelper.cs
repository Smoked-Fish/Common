using Common.Utilities.Exceptions;
using HarmonyLib;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Common.Helpers
{
    public partial class PatchHelper
    {
        private static Harmony? _harmony;
        private readonly Type? _objectType;

        protected PatchHelper(Type? objectType = null)
        {
            _ = _harmony ?? throw new InvalidOperationException("Harmony instance is not initialized.");
            _objectType = objectType;
        }

        public static void Init(Harmony harmony)
        {
            _harmony = harmony ?? throw new ArgumentNullException(nameof(harmony), "Harmony instance cannot be null.");
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        protected void Patch(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
        {
            try
            {
                MethodInfo targetMethod = _objectType != null ? AccessTools.Method(_objectType, originalMethod, parameters) : AccessTools.Method(originalMethod, parameters);
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
        protected void ConstructorPatch(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
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
    }

    public partial class PatchHelper
    {
        public enum PatchType
        {
            Prefix,
            Postfix,
            Transpiler,
        }
    }
}