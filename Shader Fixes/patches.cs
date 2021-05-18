using System;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts;

namespace Shader_Fixes
{
    // Fixes to SSAO
    [HarmonyPatch(typeof(Assets.Scripts.CameraController), "SetAmbientOcclusion")]
    public class SetAmbientOcclusion_patched
    {

        static bool Prefix(CameraController __instance)
        {
            var SSAOPatch = CameraController.Instance.AmbientOcclusionEffect;

            SSAOPatch.Bias = 0.1f;
            SSAOPatch.Blur = SSAOPro.BlurMode.Gaussian;
            SSAOPatch.BlurDownsampling = true;
            SSAOPatch.BlurPasses = 3;
            SSAOPatch.CutoffDistance = 150.0f;
            SSAOPatch.CutoffFalloff = 50.0f;
            SSAOPatch.Distance = 2.18f;
            SSAOPatch.Downsampling = 2;
            SSAOPatch.Intensity = 4.5f;
            SSAOPatch.LumContribution = 0.35f;

            CameraController.Instance.AmbientOcclusionEffect = SSAOPatch;
            return true;
        }

    }
}
