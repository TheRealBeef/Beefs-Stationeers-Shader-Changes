using System;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts;

namespace Shader_Fixes
{
	
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
            float pix_size = 2.0f;
            float f = 0.0f;
            float result = 0.0f;

            for (int x = 0;x < size; x++)
            {
                u = Mathf.Floor(x / pix_size);
                for (int y = 0; y <size; y++)
                {
                    v = Mathf.Floor(y / pix_size);
                    f = 0.06711056f * u + 0.00583715f * v;
                    result = fractional(52.9829189f * fractional(f));
                    noise_tex.SetPixel(x,y,new Color (result, result, result, result)); // Only W channel matters here
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
            SSAOPatch.Blur = SSAOPro.BlurMode.Gaussian;
            SSAOPatch.BlurDownsampling = true;
            SSAOPatch.Radius = 0.015f;
            SSAOPatch.BlurPasses = 2;
            SSAOPatch.CutoffDistance = 150.0f;
            SSAOPatch.CutoffFalloff = 20.0f;
            SSAOPatch.Distance = 3.0f;
            SSAOPatch.Downsampling = 2;
            SSAOPatch.Intensity = 8.0f;
            SSAOPatch.LumContribution = 0.20f;
            
            //// GUH ////
            if (!tex_generated)
            {
                int size = 256;
                BeefShaders.BeefShaders.AppendLog("Generating Noise Texture...");
                SSAOPatch.NoiseTexture = beefs_CPU_destroyer_aka_generate_noise(size);
                BeefShaders.BeefShaders.AppendLog("Applied Noise Texture");
            }
        }
    }
}
