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

        static AccessTools.FieldRef<Assets.Scripts.CameraController, bool> isRunningRef = AccessTools.FieldRefAccess<Assets.Scripts.CameraController, bool>("isRunning");
        static bool Prefix(Assets.Scripts.CameraController __instance)
        {
            var SSAOPatch = Assets.Scripts.CameraController.Instance.AmbientOcclusionEffect;

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

            Assets.Scripts.CameraController.Instance.AmbientOcclusionEffect = SSAOPatch;
            return true;
        }

    }
}
