using System;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace BeefsShaderChanges
{
    public static class BloomManager
    {
        private class BloomOriginalSettings
        {
            public float Intensity;
            public float Threshold;
            public UltimateBloom.HDRBloomMode HDRMode;
            public UltimateBloom.BloomIntensityManagement IntensityManagement;
        }

        private static System.Collections.Generic.Dictionary<UltimateBloom, BloomOriginalSettings> _originalSettings
            = new System.Collections.Generic.Dictionary<UltimateBloom, BloomOriginalSettings>();

        private static void StoreOriginalSettings(UltimateBloom bloom)
        {
            if (!_originalSettings.ContainsKey(bloom))
            {
                _originalSettings[bloom] = new BloomOriginalSettings
                {
                    Intensity = bloom.m_BloomIntensity,
                    Threshold = bloom.m_BloomThreshhold,
                    HDRMode = bloom.m_HDR,
                    IntensityManagement = bloom.m_IntensityManagement
                };
            }
        }

        private static void RestoreOriginalSettings(UltimateBloom bloom)
        {
            if (_originalSettings.TryGetValue(bloom, out var original))
            {
                bloom.m_BloomIntensity = original.Intensity;
                bloom.m_BloomThreshhold = original.Threshold;
                bloom.m_HDR = original.HDRMode;
                bloom.m_IntensityManagement = original.IntensityManagement;

                if (bloom.enabled)
                {
                    bloom.enabled = false;
                    bloom.enabled = true;
                }
            }
        }

        public static void UpdateBloom()
        {
            try
            {
                var blooms = UnityEngine.Object.FindObjectsOfType<UltimateBloom>();
                foreach (var bloom in blooms)
                {
                    StoreOriginalSettings(bloom);

                    if (BeefsShaderChangesPlugin.EnableBloomTweaks.Value)
                    {
                        bloom.m_BloomIntensity = BeefsShaderChangesPlugin.BloomIntensity.Value;
                        bloom.m_BloomThreshhold = BeefsShaderChangesPlugin.BloomThreshold.Value;
                        bloom.m_HDR = UltimateBloom.HDRBloomMode.On;
                        bloom.m_IntensityManagement = UltimateBloom.BloomIntensityManagement.Threshold;
                    }
                    else
                    {
                        RestoreOriginalSettings(bloom);
                    }

                    if (bloom.enabled)
                    {
                        bloom.enabled = false;
                        bloom.enabled = true;
                    }
                }
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error updating bloom: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(UltimateBloom), "Awake")]
    public static class BloomPatcher
    {
        static void Postfix(UltimateBloom __instance)
        {
            try
            {
                BloomManager.UpdateBloom();
            }
            catch (Exception e)
            {
                BeefsShaderChangesPlugin.Log.LogError($"Error applying bloom settings: {e.Message}");
            }
        }
    }
}