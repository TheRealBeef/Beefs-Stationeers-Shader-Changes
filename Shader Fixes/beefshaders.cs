using System;
using HarmonyLib;
using BepInEx;
using UnityEngine;

namespace BeefShaders
{
    [BepInPlugin("org.bepinex.plugins.beefshaders", "Beef's Beefy Shaders", "0.1")]
    [BepInProcess("rocketstation.exe")]
    public class init : BaseUnityPlugin
    {
        void Awake()
        {
            BeefShaders.Awake();
        }
    }

    public class BeefShaders
    {
        // Variable Definitions

        public static void AppendLog(string logdetails)
        {
            UnityEngine.Debug.Log("Beef's Shader Fixes - " + logdetails);
        }

        // Awake is called once when both the game and the plug-in are loaded
        public static void Awake()
        {
            //Initialize();

            AppendLog("Initialized");
            var harmony = new Harmony("org.bepinex.plugins.beefshaders");
            harmony.PatchAll();
            AppendLog("Patched with Harmony");
        }
    }
}
