using System;
using Assets.Scripts;
using HarmonyLib;
using UnityEngine;

namespace BeefsShaderChanges
{
    public static class ShaderChangesManager
    {
        private static VignetteEffect _vignette;
        private static HelmetReflectionsEffect _helmetReflections;
        private static DepthOfFieldEffect _depthOfField;

        public static bool IsInitialized { get; private set; }

        public static bool Initialize()
        {
            if (IsInitialized)
                return true;

            if (!AssetBundleLoader.IsLoaded)
            {
                BeefsShaderChangesPlugin.Log.LogError("Cannot initialize effects - AssetBundle not loaded");
                return false;
            }

            try
            {
                var camera = CameraController.Instance?.MainCamera;
                if (camera == null)
                {
                    BeefsShaderChangesPlugin.Log.LogError("Main camera not found");
                    return false;
                }

                _vignette = camera.gameObject.GetComponent<VignetteEffect>();
                if (_vignette == null)
                    _vignette = camera.gameObject.AddComponent<VignetteEffect>();

                if (!_vignette.InitializeShaders())
                {
                    BeefsShaderChangesPlugin.Log.LogWarning("Vignette shader failed to initialize");
                }
                _vignette.enabled = false;

                _helmetReflections = camera.gameObject.GetComponent<HelmetReflectionsEffect>();
                if (_helmetReflections == null)
                    _helmetReflections = camera.gameObject.AddComponent<HelmetReflectionsEffect>();

                if (!_helmetReflections.InitializeShader())
                {
                    BeefsShaderChangesPlugin.Log.LogWarning("Helmet shader failed to initialize");
                }
                _helmetReflections.enabled = false;

                _depthOfField = camera.gameObject.GetComponent<DepthOfFieldEffect>();
                if (_depthOfField == null)
                    _depthOfField = camera.gameObject.AddComponent<DepthOfFieldEffect>();

                if (!_depthOfField.InitializeShader())
                {
                    BeefsShaderChangesPlugin.Log.LogWarning("Depth of Field shader failed to initialize");
                }
                _depthOfField.enabled = false;

                IsInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Failed to initialize effects: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public static void UpdateVignette(bool enabled, float? intensity = null, float? blur = null)
        {
            if (!EnsureInitialized() || _vignette == null) return;

            try
            {
                _vignette.enabled = enabled;
                if (_vignette.enabled)
                {
                    _vignette.intensity = intensity ?? 0.036f;
                    _vignette.blur = blur ?? 0.0f;
                    _vignette.blurSpread = 0.75f;
                }
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error updating vignette: {e.Message}");
            }
        }

        public static void UpdateDepthOfField()
        {
            if (!EnsureInitialized() || _depthOfField == null) return;

            try
            {
                _depthOfField.enabled = BeefsShaderChangesPlugin.EnableDepthOfField.Value;
                if (_depthOfField.enabled)
                {
                    _depthOfField.focalLength = BeefsShaderChangesPlugin.DOFFocalLength.Value;
                    _depthOfField.focalSize = BeefsShaderChangesPlugin.DOFFocalSize.Value;
                    _depthOfField.aperture = BeefsShaderChangesPlugin.DOFAperture.Value;
                    _depthOfField.maxBlurSize = BeefsShaderChangesPlugin.DOFMaxBlurSize.Value;
                    _depthOfField.highResolution = BeefsShaderChangesPlugin.DOFHighResolution.Value;

                    _depthOfField.blurSampleCount = BeefsShaderChangesPlugin.DOFSampleCount.Value switch
                    {
                        0 => DepthOfFieldEffect.BlurSampleCount.Low,
                        1 => DepthOfFieldEffect.BlurSampleCount.Medium,
                        _ => DepthOfFieldEffect.BlurSampleCount.High
                    };
                }
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error updating depth of field: {e.Message}");
            }
        }

        public static void UpdateHelmetVisor(bool enabled, float? radius = null, float? fade = null)
        {
            if (!EnsureInitialized() || _helmetReflections == null) return;

            try
            {
                _helmetReflections.enabled = enabled;

                if (_helmetReflections.enabled)
                {
                    _helmetReflections.enableReflections = true;
                    _helmetReflections.reflectionRadius = radius ?? 0.45f;
                    _helmetReflections.reflectionSamples = BeefsShaderChangesPlugin.VisorReflectionSamples?.Value ?? 32;
                    _helmetReflections.reflectionIntensity = BeefsShaderChangesPlugin.VisorReflectionIntensity?.Value ?? 0.8f;
                    _helmetReflections.innerFadeStart = fade ?? 0.6f;
                }
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error updating helmet visor: {e.Message}\n{e.StackTrace}");
            }
        }

        private static bool EnsureInitialized()
        {
            if (!IsInitialized)
            {
                return Initialize();
            }
            return true;
        }

        public static void Cleanup()
        {
            if (_vignette != null)
                UnityEngine.Object.Destroy(_vignette);
            if (_helmetReflections != null)
                UnityEngine.Object.Destroy(_helmetReflections);
            if (_depthOfField != null)
                UnityEngine.Object.Destroy(_depthOfField);

            IsInitialized = false;
        }
    }

    [HarmonyPatch(typeof(CameraController), "ManagerAwake")]
    public static class CameraControllerAwakePatch
    {
        private static bool IsInGameWorldExclMainMenu()
        {
            if (!(BeefsShaderChangesPlugin.Instance?.IsInGameWorld() ?? false))
                return false;

            try
            {
                Light worldSun = WorldManager.Instance?.WorldSun?.TargetLight;
                return worldSun != null;
            }
            catch
            {
                return false;
            }
        }

        static void Postfix()
        {
            if (AssetBundleLoader.IsLoaded)
            {
                ShaderChangesManager.Initialize();
            }

            CameraController.SetAmbientOcclusion();

            if (ShaderChangesManager.IsInitialized)
            {
                ShaderChangesManager.UpdateDepthOfField();

                if (IsInGameWorldExclMainMenu())
                {
                    ShaderChangesManager.UpdateHelmetVisor(BeefsShaderChangesPlugin.EnableHelmetVisor?.Value ?? false);


                }
                else
                {
                    ShaderChangesManager.UpdateHelmetVisor(false);
                    ShaderChangesManager.UpdateVignette(false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "Awake")]
    public static class GameManagerAwakePatch
    {
        static void Postfix(GameManager __instance)
        {
            try
            {
                var configMenu = __instance.gameObject.GetComponent<ConfigMenu>();
                if (configMenu == null)
                {
                    __instance.gameObject.AddComponent<ConfigMenu>();
                }
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error adding config menu: {e.Message}");
            }
        }
    }
}