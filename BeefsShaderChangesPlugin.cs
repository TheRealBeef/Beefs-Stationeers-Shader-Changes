using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BeefsShaderChanges
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("rocketstation.exe")]
    public class BeefsShaderChangesPlugin : BaseUnityPlugin
    {
        public static BeefsShaderChangesPlugin Instance;
        public static ManualLogSource Log;

        public static ConfigEntry<bool> EnableSSAO;
        public static ConfigEntry<string> QualityPreset;
        public static ConfigEntry<bool> EnableCustomNoise;

        public static ConfigEntry<int> CustomSamples;
        public static ConfigEntry<int> CustomDownsampling;
        public static ConfigEntry<float> CustomRadius;
        public static ConfigEntry<float> CustomIntensity;
        public static ConfigEntry<float> CustomBias;
        public static ConfigEntry<float> CustomDistance;
        public static ConfigEntry<float> CustomLumContribution;
        public static ConfigEntry<int> CustomBlurPasses;
        public static ConfigEntry<float> CustomCutoffDistance;

        public static ConfigEntry<bool> EnableBloomTweaks;
        public static ConfigEntry<float> BloomIntensity;
        public static ConfigEntry<float> BloomThreshold;

        public static ConfigEntry<bool> EnableHelmetVisor;
        public static ConfigEntry<int> VisorReflectionSamples;
        public static ConfigEntry<float> VisorReflectionIntensity;

        public static ConfigEntry<bool> EnableDepthOfField;
        public static ConfigEntry<float> DOFFocalLength;
        public static ConfigEntry<float> DOFFocalSize;
        public static ConfigEntry<float> DOFAperture;
        public static ConfigEntry<float> DOFMaxBlurSize;
        public static ConfigEntry<bool> DOFHighResolution;
        public static ConfigEntry<int> DOFSampleCount;

        public static ConfigEntry<bool> DOFAutoFocus;
        public static ConfigEntry<int> DOFAutoFocusMode;
        public static ConfigEntry<float> DOFAutoFocusSampleRadius;
        public static ConfigEntry<float> DOFAutoFocusOffset;
        public static ConfigEntry<float> DOFAutoFocusSmoothTime;
        public static ConfigEntry<float> DOFAutoFocusMinDistance;
        public static ConfigEntry<float> DOFAutoFocusMaxDistance;
        public static ConfigEntry<bool> DOFShowFocusIndicator;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            //SSAO
            EnableSSAO = Config.Bind("SSAO", "EnableSSAO", true,
                "Enable Ambient Occlusion (SSAO)");

            QualityPreset = Config.Bind("SSAO", "QualityPreset", "High",
                new ConfigDescription("SSAO quality preset",
                    new AcceptableValueList<string>("Low", "Medium", "High", "Custom")));

            EnableCustomNoise = Config.Bind("SSAO", "EnableCustomNoise", true,
                "Enable custom noise texture");

            CustomSamples = Config.Bind("SSAO Advanced", "CustomSamples", 2,
                new ConfigDescription("Sample count (0=VeryLow, 1=Low, 2=Medium, 3=High, 4=Ultra)",
                    new AcceptableValueRange<int>(0, 4)));

            CustomDownsampling = Config.Bind("SSAO Advanced", "CustomDownsampling", 2,
                new ConfigDescription("Downsampling factor (1=Full, 2=Half)",
                    new AcceptableValueRange<int>(1, 2)));

            CustomRadius = Config.Bind("SSAO Advanced", "CustomRadius", 0.04f,
                new ConfigDescription("AO sampling radius",
                    new AcceptableValueRange<float>(0.001f, 0.5f)));

            CustomIntensity = Config.Bind("SSAO Advanced", "CustomIntensity", 7.5f,
                new ConfigDescription("AO intensity",
                    new AcceptableValueRange<float>(0f, 16f)));

            CustomBias = Config.Bind("SSAO Advanced", "CustomBias", 0.66f,
                new ConfigDescription("AO bias",
                    new AcceptableValueRange<float>(0f, 1f)));

            CustomDistance = Config.Bind("SSAO Advanced", "CustomDistance", 3.0f,
                new ConfigDescription("AO distance fade",
                    new AcceptableValueRange<float>(0f, 10f)));

            CustomLumContribution = Config.Bind("SSAO Advanced", "CustomLumCont", 0.20f,
                new ConfigDescription("Luminance contribution",
                    new AcceptableValueRange<float>(0f, 1f)));

            CustomBlurPasses = Config.Bind("SSAO Advanced", "CustomBlurPasses", 2,
                new ConfigDescription("Number of blur passes",
                    new AcceptableValueRange<int>(1, 4)));

            CustomCutoffDistance = Config.Bind("SSAO Advanced", "CustomCutoffDistance", 50f,
                new ConfigDescription("AO cutoff distance",
                    new AcceptableValueRange<float>(10f, 100f)));


            //Bloom
            EnableBloomTweaks = Config.Bind("Bloom", "EnableBloomTweaks", false,
                "Enable bloom adjustments");

            BloomIntensity = Config.Bind("Bloom", "BloomIntensity", 0.2f,
                new ConfigDescription("Bloom intensity",
                    new AcceptableValueRange<float>(0f, 1f)));

            BloomThreshold = Config.Bind("Bloom", "BloomThreshold", 1.01f,
                new ConfigDescription("Brightness threshold (above which bloom is applied)",
                    new AcceptableValueRange<float>(0f, 5f)));


            //Helmet
            EnableHelmetVisor = Config.Bind("Helmet Visor", "EnableHelmetVisor", true,
                "Enable helmet visor effect");

            VisorReflectionSamples = Config.Bind("Helmet Visor", "ReflectionSamples", 32,
                new ConfigDescription("Reflection number of samples (more is slower)",
                    new AcceptableValueRange<int>(8, 64)));

            VisorReflectionIntensity = Config.Bind("Helmet Visor", "ReflectionIntensity", 0.3f,
                new ConfigDescription("Strength of the reflection",
                    new AcceptableValueRange<float>(0f, 2f)));


            //DOF
            EnableDepthOfField = Config.Bind("Depth of Field", "EnableDepthOfField", false,
                "Enable depth of field");

            DOFFocalLength = Config.Bind("Depth of Field", "DOFFocalLength", 2.0f,
                new ConfigDescription("Distance to focus point (used when auto-focus is disabled)",
                    new AcceptableValueRange<float>(0.1f, 100f)));

            DOFFocalSize = Config.Bind("Depth of Field", "DOFFocalSize", 0.2f,
                new ConfigDescription("Size of focused area (depth range that stays sharp)",
                    new AcceptableValueRange<float>(0f, 2f)));

            DOFAperture = Config.Bind("Depth of Field", "DOFAperture", 0.3f,
                new ConfigDescription("Lens aperture (affects blur falloff)",
                    new AcceptableValueRange<float>(0f, 1f)));

            DOFMaxBlurSize = Config.Bind("Depth of Field", "DOFMaxBlurSize", 5.0f,
                new ConfigDescription("Maximum blur amount away from focal area",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            DOFHighResolution = Config.Bind("Depth of Field", "DOFHighResolution", true,
                "Enable high resolution DOF (more performance hit)");

            DOFSampleCount = Config.Bind("Depth of Field", "DOFSampleCount", 2,
                new ConfigDescription("Blur sample quality (0=Low, 1=Medium, 2=High)",
                    new AcceptableValueRange<int>(0, 2)));

            DOFAutoFocus = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocus", false,
                "Enable auto-focus (automatically focus on what's at the center of the screen)");

            DOFAutoFocusMode = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusMode", 0,
                new ConfigDescription("Focus sampling mode (0=Single center point, 1=9-point average)",
                    new AcceptableValueRange<int>(0, 1)));

            DOFAutoFocusSampleRadius = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusSampleRadius", 0.05f,
                new ConfigDescription("Sample radius for 9-point mode (percentage of screen width)",
                    new AcceptableValueRange<float>(0.01f, 0.25f)));

            DOFAutoFocusOffset = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusOffset", 0f,
                new ConfigDescription("Offset added to auto-detected focus distance",
                    new AcceptableValueRange<float>(-10f, 10f)));

            DOFAutoFocusSmoothTime = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusSmoothTime", 0.15f,
                new ConfigDescription("Focus transition smoothing time (0 = instant)",
                    new AcceptableValueRange<float>(0f, 1f)));

            DOFAutoFocusMinDistance = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusMinDistance", 0.5f,
                new ConfigDescription("Minimum auto-focus distance",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            DOFAutoFocusMaxDistance = Config.Bind("Depth of Field Auto Focus", "DOFAutoFocusMaxDistance", 100f,
                new ConfigDescription("Maximum auto-focus distance",
                    new AcceptableValueRange<float>(10f, 500f)));

            DOFShowFocusIndicator = Config.Bind("Depth of Field Auto Focus", "DOFShowFocusIndicator", false,
                "Show focus point indicator on screen (useful for tuning)");


            if (AssetBundleLoader.LoadBundle())
            {
                Log.LogInfo("AssetBundle loaded successfully");
            }
            else
            {
                Log.LogWarning("AssetBundle not loaded - some effects may not work");
            }

            try
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                Log.LogInfo("Harmony patches applied successfully");
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to apply Harmony patches: {e}");
            }
        }

        public bool IsInGameWorld()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string lowerSceneName = sceneName.ToLower();
            bool isMenu = lowerSceneName.Contains("menu") || lowerSceneName.Contains("splash");
            return !isMenu;
        }
    }

    public static class AssetBundleLoader
    {
        private static readonly string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static AssetBundle _effectsBundle;

        public static AssetBundle Bundle
        {
            get
            {
                if (_effectsBundle == null)
                {
                    string bundlePath = Path.Combine(ModDirectory, "Content", "postprocessing.asset");
                    if (File.Exists(bundlePath))
                    {
                        _effectsBundle = AssetBundle.LoadFromFile(bundlePath);
                    }
                }
                return _effectsBundle;
            }
        }

        public static bool IsLoaded => _effectsBundle != null;

        public static bool LoadBundle()
        {
            string bundlePath = Path.Combine(ModDirectory, "Content", "postprocessing.asset");

            try
            {
                if (Bundle == null)
                {
                    BeefsShaderChangesPlugin.Log.LogError($"Failed to load AssetBundle from: {bundlePath}");

                    if (!File.Exists(bundlePath))
                    {
                        BeefsShaderChangesPlugin.Log.LogError("AssetBundle file does not exist");
                    }

                    return false;
                }

                var shaders = Bundle.LoadAllAssets<Shader>();
                if (shaders == null || shaders.Length == 0)
                {
                    BeefsShaderChangesPlugin.Log.LogWarning("No shaders found in bundle");
                }

                return true;
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error loading AssetBundle: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public static Shader GetShader(string shaderName)
        {
            if (Bundle == null) return null;

            try
            {
                var shader = Bundle.LoadAsset<Shader>(shaderName);
                if (shader != null)
                {
                    return shader;
                }

                var allShaders = Bundle.LoadAllAssets<Shader>();
                foreach (var s in allShaders)
                {
                    if (s.name.Contains(shaderName) || shaderName.Contains(s.name))
                    {
                        return s;
                    }
                }

                BeefsShaderChangesPlugin.Log.LogWarning($"Shader not found: {shaderName}");
                return null;
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error loading shader {shaderName}: {e.Message}");
                return null;
            }
        }

        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (Bundle == null) return null;

            try
            {
                return Bundle.LoadAsset<T>(assetName);
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error loading asset {assetName}: {e.Message}");
                return null;
            }
        }

        public static void UnloadBundle(bool unloadAllLoadedObjects = false)
        {
            if (_effectsBundle != null)
            {
                _effectsBundle.Unload(unloadAllLoadedObjects);
                _effectsBundle = null;
            }
        }
    }
}