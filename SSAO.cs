using System;
using Assets.Scripts;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace BeefsShaderChanges
{
    public static class NoiseGenerator
    {
        private static bool _noiseGenerated = false;
        private static Texture2D _noiseTexture;
        private static Texture2D _originalNoiseTexture;
        private static int _lastGeneratedSize = -1;

        private static float Fractional(float num)
        {
            return num - Mathf.Floor(num);
        }

        public static int GetOptimalNoiseSize()
        {
            int height = Screen.height;
            if (height >= 2160) return 2048;
            if (height >= 1440) return 1024;
            return 512;
        }

        public static Texture2D GenerateBlueNoise(int size)
        {
            if (_noiseGenerated && _noiseTexture != null && _lastGeneratedSize == size)
            {
                return _noiseTexture;
            }

            if (_noiseTexture != null)
            {
                UnityEngine.Object.Destroy(_noiseTexture);
            }

            Texture2D noise = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            noise.filterMode = FilterMode.Point;
            noise.wrapMode = TextureWrapMode.Repeat;

            float goldenRatioConjugate = 0.61803398875f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float noise_value = Fractional(52.9829189f * Fractional(u * 0.06711056f + v * 0.00583715f));
                    float jitter = Fractional((x * goldenRatioConjugate + y) * goldenRatioConjugate);
                    noise_value = Fractional(noise_value + jitter * 0.1f);

                    noise.SetPixel(x, y, new Color(noise_value, noise_value, noise_value, noise_value));
                }
            }

            noise.Apply(false);
            _noiseGenerated = true;
            _noiseTexture = noise;
            _lastGeneratedSize = size;

            return noise;
        }

        public static void StoreOriginalTexture(Texture2D original)
        {
            if (_originalNoiseTexture == null && original != null)
            {
                _originalNoiseTexture = original;
            }
        }

        public static Texture2D GetOriginalTexture()
        {
            return _originalNoiseTexture;
        }

        public static void Reset()
        {
            _noiseGenerated = false;
            _lastGeneratedSize = -1;
            if (_noiseTexture != null)
            {
                UnityEngine.Object.Destroy(_noiseTexture);
                _noiseTexture = null;
            }
        }

        public static void ForceRegenerate(bool applyToSSAO = true)
        {
            Reset();
            if (BeefsShaderChangesPlugin.EnableCustomNoise.Value)
            {
                GenerateBlueNoise(GetOptimalNoiseSize());
                if (applyToSSAO && CameraController.Instance?.AmbientOcclusionEffect != null)
                {
                    CameraController.SetAmbientOcclusion();
                }
            }
        }
    }

    public static class SSAOPresets
    {
        public static void ApplyLow(SSAOPro component)
        {
            component.Samples = SSAOPro.SampleCount.Low;
            component.Downsampling = 2;
            component.Radius = 0.012f;
            component.Intensity = 5.0f;
            component.Distance = 3.0f;
            component.Bias = 0.66f;
            component.LumContribution = 0.20f;
            component.Blur = SSAOPro.BlurMode.Gaussian;
            component.BlurDownsampling = true;
            component.BlurPasses = 1;
            component.BlurBilateralThreshold = 10f;
            component.CutoffDistance = 120f;
            component.CutoffFalloff = 20f;
        }

        public static void ApplyMedium(SSAOPro component)
        {
            component.Samples = SSAOPro.SampleCount.High;
            component.Downsampling = 1;
            component.Radius = 0.02f;
            component.Intensity = 6.5f;
            component.Distance = 3.0f;
            component.Bias = 0.66f;
            component.LumContribution = 0.20f;
            component.Blur = SSAOPro.BlurMode.HighQualityBilateral;
            component.BlurDownsampling = false;
            component.BlurPasses = 1;
            component.BlurBilateralThreshold = 10f;
            component.CutoffDistance = 150f;
            component.CutoffFalloff = 25f;
        }

        public static void ApplyHigh(SSAOPro component)
        {
            component.Samples = SSAOPro.SampleCount.Ultra;
            component.Downsampling = 1;
            component.Radius = 0.03f;
            component.Intensity = 7.5f;
            component.Distance = 3.0f;
            component.Bias = 0.66f;
            component.LumContribution = 0.20f;
            component.Blur = SSAOPro.BlurMode.HighQualityBilateral;
            component.BlurDownsampling = false;
            component.BlurPasses = 1;
            component.BlurBilateralThreshold = 10f;
            component.CutoffDistance = 180f;
            component.CutoffFalloff = 30f;
        }

        public static void ApplyCustom(SSAOPro component)
        {
            component.Samples = (SSAOPro.SampleCount)BeefsShaderChangesPlugin.CustomSamples.Value;
            component.Downsampling = BeefsShaderChangesPlugin.CustomDownsampling.Value;
            component.Radius = BeefsShaderChangesPlugin.CustomRadius.Value;
            component.Intensity = BeefsShaderChangesPlugin.CustomIntensity.Value;
            component.Distance = BeefsShaderChangesPlugin.CustomDistance.Value;
            component.Bias = BeefsShaderChangesPlugin.CustomBias.Value;
            component.LumContribution = BeefsShaderChangesPlugin.CustomLumContribution.Value;
            component.Blur = SSAOPro.BlurMode.HighQualityBilateral;
            component.BlurDownsampling = true;
            component.BlurPasses = BeefsShaderChangesPlugin.CustomBlurPasses.Value;
            component.BlurBilateralThreshold = 10f;
            component.CutoffDistance = BeefsShaderChangesPlugin.CustomCutoffDistance.Value;
            component.CutoffFalloff = 20f;
        }

        public static string GetPresetName(string preset)
        {
            return preset switch
            {
                "Low" => "Low Quality",
                "Medium" => "Medium Quality",
                "High" => "High Quality",
                "Custom" => "Custom Settings",
                _ => "Unknown"
            };
        }

        public static string GetSampleCountName(int value)
        {
            return value switch
            {
                0 => "Very Low",
                1 => "Low",
                2 => "Medium",
                3 => "High",
                4 => "Ultra",
                _ => "Unknown"
            };
        }
    }

    [HarmonyPatch(typeof(CameraController), "SetAmbientOcclusion", new Type[] { })]
    public static class SSAOPatcher
    {
        public static void Prefix()
        {
            if (!BeefsShaderChangesPlugin.EnableSSAO.Value)
            {
                if (CameraController.Instance?.AmbientOcclusionEffect != null)
                {
                    CameraController.Instance.AmbientOcclusionEffect.enabled = false;
                }
                return;
            }

            if (CameraController.Instance == null || CameraController.Instance.AmbientOcclusionEffect == null)
                return;

            var ssao = CameraController.Instance.AmbientOcclusionEffect;
            string aoSetting = BeefsShaderChangesPlugin.QualityPreset.Value;

            if (ssao.NoiseTexture != null)
            {
                NoiseGenerator.StoreOriginalTexture(ssao.NoiseTexture);
            }

            ssao.enabled = true;

            switch (aoSetting)
            {
                case "Low":
                    SSAOPresets.ApplyLow(ssao);
                    break;
                case "Medium":
                    SSAOPresets.ApplyMedium(ssao);
                    break;
                case "High":
                    SSAOPresets.ApplyHigh(ssao);
                    break;
                case "Custom":
                    SSAOPresets.ApplyCustom(ssao);
                    break;
                default:
                    SSAOPresets.ApplyMedium(ssao);
                    break;
            }

            if (BeefsShaderChangesPlugin.EnableCustomNoise.Value)
            {
                ssao.NoiseTexture = NoiseGenerator.GenerateBlueNoise(NoiseGenerator.GetOptimalNoiseSize());
            }
            else
            {
                var originalTexture = NoiseGenerator.GetOriginalTexture();
                ssao.NoiseTexture = originalTexture;
            }

            if (ssao.enabled)
            {
                ssao.enabled = false;
                ssao.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(CameraController), "SetAmbientOcclusion", typeof(SSAOPro))]
    public static class SSAOProPatcher
    {
        static bool Prefix()
        {
            SSAOPatcher.Prefix();
            return false;
        }
    }
}