using Assets.Scripts;
using HarmonyLib;
using System;
using UnityEngine;

namespace Shader_Fixes
{
    public class PatchConfigs
    {
        public static bool I_Have_A_Strong_PC_And_GPU = false; // Determines whether to use quarter or half resolution, and how many samples.
    }

    // Fixes to SSAO
    [HarmonyPatch(typeof(CameraController), "SetAmbientOcclusion", new Type[] { })]
    public class SSAOPatcher
    {
        public static bool tex_generated = false;

        public static float fractional(float num)
        {
            return num - Mathf.Floor(num);
        }
        public static Texture2D beefs_CPU_destroyer_aka_generate_noise(int size)
        {
            Texture2D noise_tex = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
            noise_tex.filterMode = FilterMode.Point;
            float u = 0.0f;
            float v = 0.0f;
            float pix_size = 1.0f;
            float f = 0.0f;
            float result = 0.0f;

            for (int x = 0; x < size; x++)
            {
                u = Mathf.Floor(x / pix_size);
                for (int y = 0; y < size; y++)
                {
                    v = Mathf.Floor(y / pix_size);
                    f = 0.06711056f * u + 0.00583715f * v;
                    result = fractional(52.9829189f * fractional(f));
                    noise_tex.SetPixel(x, y, new Color(result, result, result, result)); // Only W channel matters here
                    // BeefShaders.BeefShaders.AppendLog("Generating, curently on row " + y.ToString() +" and column " + x.ToString());
                }
            }
            noise_tex.Apply();
            tex_generated = true;
            return noise_tex;
        }

        static void Prefix()
        {

            var SSAOPatch = CameraController.Instance.AmbientOcclusionEffect;
            BeefShaders.BeefShaders.AppendLog("Applying SSAO Settings");

            SSAOPatch.Bias = 0.35f;
            SSAOPatch.Blur = SSAOPro.BlurMode.HighQualityBilateral;
            SSAOPatch.BlurDownsampling = true;
            SSAOPatch.Radius = 0.015f;
            SSAOPatch.BlurPasses = 2;
            SSAOPatch.CutoffDistance = 150.0f;
            SSAOPatch.CutoffFalloff = 20.0f;
            SSAOPatch.Distance = 3.0f;
            SSAOPatch.Downsampling = 2;
            SSAOPatch.Intensity = 7.5f;
            SSAOPatch.LumContribution = 0.20f;

            //// GUH ////
            if (!tex_generated)
            {
                int size = 512;
                BeefShaders.BeefShaders.AppendLog("Generating Noise Texture...");
                SSAOPatch.NoiseTexture = beefs_CPU_destroyer_aka_generate_noise(size);
                BeefShaders.BeefShaders.AppendLog("Applied Noise Texture");
            }
        }
    }

    // Changes to Bloom
    [HarmonyPatch(typeof(UltimateBloom), "Awake")]
    public class BloomPatcher
    {
        static void Postfix(UltimateBloom __instance)
        {
            UltimateBloom TonemapPatcher = __instance as UltimateBloom;

            TonemapPatcher.m_BloomIntensity = 0.07f;
            TonemapPatcher.m_BloomThreshhold = 1.2f;
            TonemapPatcher.m_HDR = UltimateBloom.HDRBloomMode.On;
            TonemapPatcher.m_IntensityManagement = UltimateBloom.BloomIntensityManagement.Threshold;
        }
    }

    // Changes to Volumetric Lighting
    [HarmonyPatch(typeof(VolumetricLight), "VolumetricLightRenderer_PreRenderEvent")]
    public class VolumetricPatcher
    {
        static void Prefix(VolumetricLight __instance)
        {
            VolumetricLight VolumetricPatcher = __instance as VolumetricLight;
            if (PatchConfigs.I_Have_A_Strong_PC_And_GPU == true)
            {
                VolumetricPatcher.SampleCount = 4; // 1;
                VolumetricPatcher.MaxRayLength = 200.0f; // 200.0f;
            }
            else
            {
                VolumetricPatcher.SampleCount = 2; // 1;
                VolumetricPatcher.MaxRayLength = 100.0f; // 200.0f;
            }


            VolumetricPatcher.ExtinctionCoef = 0.012f;    // 0.0211f;
            VolumetricPatcher.MieG = 0.25f;              // 0.321f;
            VolumetricPatcher.ScatteringCoef = 0.12f;    // 0.176f;
            VolumetricPatcher.SkyboxExtinctionCoef = 0.01f; // 0.246f;
        }
    }

    // Changes to Volumetric Lighting Part II
    [HarmonyPatch(typeof(VolumetricLightRenderer), "Awake")]
    public class VolumetricPatcherPt2
    {
        static void Postfix(VolumetricLightRenderer __instance)
        {
            VolumetricLightRenderer VolumetricPatcherPt2 = __instance as VolumetricLightRenderer;
            if (PatchConfigs.I_Have_A_Strong_PC_And_GPU == true)
            {
                VolumetricPatcherPt2.Resolution = VolumetricLightRenderer.VolumtericResolution.Half;
            }
            else
            {
                VolumetricPatcherPt2.Resolution = VolumetricLightRenderer.VolumtericResolution.Quarter;
            }
        }
    }
    /*
    [HarmonyPatch(typeof(CameraController),"Update")]
    public class TonemapPatchers
    {
        static void Postfix(CameraController __instance)
        {
            CameraController TonemapPatcher = __instance as CameraController;
            
        } 
    }*/
}
