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

        /// <summary>
        /// Initializes a new instance of the <see cref="PatchHelper"/> class.
        /// </summary>
        /// <param name="objectType">The type of the target class to patch methods on.</param>
        /// <exception cref="InvalidOperationException">Thrown if Harmony instance is not initialized.</exception>
        protected PatchHelper(Type? objectType = null)
        {
            _ = _harmony ?? throw new InvalidOperationException("Harmony instance is not initialized.");
            _objectType = objectType;
        }

        /// <summary>
        /// Initializes the Harmony instance.
        /// </summary>
        /// <param name="harmony">The Harmony instance to use for patching.</param>
        /// <exception cref="ArgumentNullException">Thrown if the Harmony instance is null.</exception>
        public static void Init(Harmony harmony)
        {
            _harmony = harmony ?? throw new ArgumentNullException(nameof(harmony), "Harmony instance cannot be null.");
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        /// <param name="patchType">The type of patch to apply (Prefix, Postfix, or Transpiler).</param>
        /// <param name="originalMethod">The name of the target method to patch.</param>
        /// <param name="newMethod">The name of the method to use for the patch.</param>
        /// <param name="parameters">The parameters of the target method.</param>
        /// <exception cref="PatchingException">Thrown if an error occurs during patching.</exception>
        protected void Patch(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
        {
            try
            {
                MethodInfo targetMethod;
                if (_objectType != null)
                {
                    targetMethod = AccessTools.Method(_objectType, originalMethod, parameters);
                }
                else
                {
                    targetMethod = AccessTools.Method(originalMethod, parameters);
                    if (targetMethod == null)
                    {
                        throw new InstanceNullException("_objectType is null and the method signature could not be resolved. Please use Patch<T> instead.");
                    }
                }

                HarmonyMethod harmonyMethod = new(GetType(), newMethod);

                ApplyPatch(patchType, targetMethod, harmonyMethod);
            }
            catch (Exception ex)
            {
                throw new PatchingException(originalMethod, newMethod, ex);
            }
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        /// <typeparam name="T">The type of the target class to patch methods on.</typeparam>
        /// <param name="patchType">The type of patch to apply (Prefix, Postfix, or Transpiler).</param>
        /// <param name="originalMethod">The name of the target method to patch.</param>
        /// <param name="newMethod">The name of the method to use for the patch.</param>
        /// <param name="parameters">The parameters of the target method.</param>
        /// <exception cref="PatchingException">Thrown if an error occurs during patching.</exception>
        protected void Patch<T>(PatchType patchType, string originalMethod, string newMethod, Type[]? parameters = null)
        {
            try
            {
                Type objectType = typeof(T);
                MethodInfo targetMethod = AccessTools.Method(objectType, originalMethod, parameters);
                HarmonyMethod harmonyMethod = new(GetType(), newMethod);

                ApplyPatch(patchType, targetMethod, harmonyMethod);
            }
            catch (Exception ex)
            {
                throw new PatchingException(originalMethod, newMethod, ex);
            }
        }

        /// <summary>
        /// Applies constructor patches using Harmony for a specified target constructor.
        /// </summary>
        /// <param name="patchType">The type of patch to apply (Prefix, Postfix, or Transpiler).</param>
        /// <param name="originalMethod">The name of the target constructor's declaring type.</param>
        /// <param name="newMethod">The name of the method to use for the patch.</param>
        /// <param name="parameters">The parameters of the target constructor.</param>
        /// <exception cref="PatchingException">Thrown if an error occurs during patching.</exception>
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

        /// <summary>
        /// Applies the specified Harmony patch to the target method or constructor.
        /// </summary>
        /// <param name="patchType">The type of patch to apply (Prefix, Postfix, or Transpiler).</param>
        /// <param name="targetMethod">The target method or constructor to patch.</param>
        /// <param name="harmonyMethod">The Harmony method to use for the patch.</param>
        /// <exception cref="InstanceNullException">Thrown if the Harmony instance is null.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if an unknown PatchType is specified.</exception>
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
        /// <summary>
        /// Specifies the type of patch to apply.
        /// </summary>
        protected enum PatchType
        {
            Prefix,
            Postfix,
            Transpiler,
        }
    }
}