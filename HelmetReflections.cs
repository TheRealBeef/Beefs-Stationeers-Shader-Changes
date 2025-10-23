using Assets.Scripts;
using Assets.Scripts.Inventory;
using HarmonyLib;
using UnityEngine;

namespace BeefsShaderChanges
{
    public class HelmetReflectionsEffect : MonoBehaviour
    {
        private Material _visorMaterial;
        private Shader _visorShader;

        public bool enableReflections = true;
        public float reflectionRadius = 0.45f;
        public int reflectionSamples = 32;
        public float reflectionIntensity = 0.8f;
        public float innerFadeStart = 0.8f;

        public bool InitializeShader()
        {
            _visorShader = AssetBundleLoader.GetShader("Hidden/HelmetVisorReflection");

            if (_visorShader == null)
            {
                BeefsShaderChangesPlugin.Log.LogError("Helmet shader not found in bundle");
                return false;
            }

            _visorMaterial = new Material(_visorShader);
            _visorMaterial.hideFlags = HideFlags.DontSave;

            return true;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_visorMaterial == null || !enabled || !enableReflections)
            {
                Graphics.Blit(source, destination);
                return;
            }

            _visorMaterial.SetFloat("_ReflectionRadius", reflectionRadius);
            _visorMaterial.SetInt("_ReflectionSamples", reflectionSamples);
            _visorMaterial.SetFloat("_ReflectionIntensity", reflectionIntensity);
            _visorMaterial.SetFloat("_InnerFadeStart", innerFadeStart);

            Graphics.Blit(source, destination, _visorMaterial);
        }

        private void OnDestroy()
        {
            if (_visorMaterial != null)
                DestroyImmediate(_visorMaterial);
        }
    }

    public static class HelmetStateManager
    {
        private static bool _lastHelmetState = false;
        private static string _lastHelmetType = "";
        private static bool _wasInGameWorld = false;

        private static readonly System.Collections.Generic.Dictionary<string, HelmetSettings> _helmetConfigs =
            new System.Collections.Generic.Dictionary<string, HelmetSettings>()
            {
                { "ItemSuitHelmetHARM", new HelmetSettings(0.45f, 0.6f, 0.35f, 0.4f) },
                { "ItemHardsuitHelmet", new HelmetSettings(0.45f, 0.6f, 0.35f, 0.4f) },
                { "ItemSpaceHelmet", new HelmetSettings(0.45f, 0.6f, 0.35f, 0.4f) },
                { "ItemEmergencySpaceHelmet", new HelmetSettings(0.45f, 0.6f, 0.35f, 0.4f) },
                { "ItemIcarusHelmet", new HelmetSettings(0.45f, 0.6f, 0.35f, 0.4f) }
            };

        public class HelmetSettings
        {
            public float ReflectionRadius;
            public float InnerFadeStart;
            public float VignetteIntensity;
            public float VignetteBlur;

            public HelmetSettings(float radius, float fade, float vignette, float blur)
            {
                ReflectionRadius = radius;
                InnerFadeStart = fade;
                VignetteIntensity = vignette;
                VignetteBlur = blur;
            }
        }

        public static void ForceRefresh()
        {
            _lastHelmetState = false;
            _lastHelmetType = "";
            Update();
        }

        private static bool IsInGameWorld()
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

        public static void Update()
        {
            bool inGameWorld = IsInGameWorld();

            if (_wasInGameWorld && !inGameWorld)
            {
                ResetEffects();
                _wasInGameWorld = false;
                return;
            }

            _wasInGameWorld = inGameWorld;

            if (!inGameWorld)
                return;

            try
            {
                bool hasClosedHelmet = false;
                string helmetType = "";

                var localHuman = InventoryManager.ParentHuman;
                if (localHuman != null)
                {
                    var helmet = localHuman.HeadAsSpaceHelmet;
                    hasClosedHelmet = helmet != null && !helmet.IsOpen;

                    if (hasClosedHelmet)
                    {
                        helmetType = helmet.PrefabName;
                    }
                }

                if (hasClosedHelmet != _lastHelmetState || helmetType != _lastHelmetType)
                {
                    _lastHelmetState = hasClosedHelmet;
                    _lastHelmetType = helmetType;
                    ApplyHelmetEffects(hasClosedHelmet, helmetType);
                }
            }
            catch (System.Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error in HelmetStateManager.Update: {e.Message}");
            }
        }

        private static void ApplyHelmetEffects(bool hasClosedHelmet, string helmetType)
        {
            if (!ShaderChangesManager.IsInitialized)
                return;

            bool visorConfigEnabled = BeefsShaderChangesPlugin.EnableHelmetVisor?.Value ?? false;

            if (hasClosedHelmet && _helmetConfigs.ContainsKey(helmetType))
            {
                var settings = _helmetConfigs[helmetType];

                if (visorConfigEnabled)
                {
                    ShaderChangesManager.UpdateHelmetVisor(
                        true,
                        settings.ReflectionRadius,
                        settings.InnerFadeStart
                    );

                    ShaderChangesManager.UpdateVignette(
                        true,
                        settings.VignetteIntensity,
                        settings.VignetteBlur
                    );
                }
                else
                {
                    ShaderChangesManager.UpdateHelmetVisor(false, null, null);
                    ShaderChangesManager.UpdateVignette(false, null, null);
                }
            }
            else
            {
                ShaderChangesManager.UpdateHelmetVisor(false, null, null);
                ShaderChangesManager.UpdateVignette(false, null, null);
            }
        }

        private static void ResetEffects()
        {
            if (!ShaderChangesManager.IsInitialized)
                return;

            try
            {
                ShaderChangesManager.UpdateHelmetVisor(false, null, null);
                ShaderChangesManager.UpdateVignette(false, null, null);

                _lastHelmetState = false;
                _lastHelmetType = "";
            }
            catch (System.Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error resetting helmet effects: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(CameraController), "LateUpdate")]
    public static class HelmetStateUpdatePatch
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
            if (!ShaderChangesManager.IsInitialized ||
                !(BeefsShaderChangesPlugin.EnableHelmetVisor?.Value ?? false))
            {
                return;
            }

            HelmetStateManager.Update();
        }
    }
}