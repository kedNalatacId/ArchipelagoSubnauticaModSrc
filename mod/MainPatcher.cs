using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;


namespace Archipelago
{
    //QMODS Interface
    public class QModPatcher
    {
        public static void Patch()
        {
            APWrapper.Init();
            Debug.Log($"Plugin Archipelago (" + APWrapper.Version + ") for Server (" 
                      + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2] + 
                      ") is loaded!");
        }
    }
    //BepInEx Interface
    [BepInPlugin("Archipelago", "Archipelago", APWrapper.Version)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            APWrapper.Init();
            Logger.LogInfo($"Plugin Archipelago (" + APWrapper.Version + ") for Server (" 
                           + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2] + 
                           ") is loaded!");
        }
    }
    //Common interface
    public static class APWrapper
    {
        public const string Version = "1.3.4";
        public static void Init()
        {
            // Plugin startup logic
            var harmony = new Harmony("Archipelago");
            APState.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}