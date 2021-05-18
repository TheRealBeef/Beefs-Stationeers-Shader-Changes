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

        static void Prefix()
        {
            var SSAOPatch = CameraController.Instance.AmbientOcclusionEffect;
            BeefShaders.BeefShaders.AppendLog("Applying SSAO Settings");

            SSAOPatch.Bias = 0.2f;
            SSAOPatch.Blur = SSAOPro.BlurMode.Gaussian;
            SSAOPatch.BlurDownsampling = true;
            SSAOPatch.Radius = 0.03f;
            SSAOPatch.BlurPasses = 2;
            SSAOPatch.CutoffDistance = 100.0f;
            SSAOPatch.CutoffFalloff = 10.0f;
            SSAOPatch.Distance = 2.18f;
            SSAOPatch.Downsampling = 2;
            SSAOPatch.Intensity = 4.5f;
            SSAOPatch.LumContribution = 0.60f;
        }

    }
}
