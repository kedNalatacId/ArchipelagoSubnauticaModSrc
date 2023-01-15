using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Archipelago
{
    //BepInEx Interface
    [BepInPlugin("Archipelago", "Archipelago", Version)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public const string Version = "1.4.0";
        private void Awake()
        {
            var harmony = new Harmony("Archipelago");
            APState.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin Archipelago (" + Version + ") for Server (" 
                           + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2] + 
                           ") is loaded!");
        }
    }
}