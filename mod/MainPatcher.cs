using System.Reflection;
using BepInEx;
using HarmonyLib;


namespace Archipelago
{
    public class MainPatcher
    {
        public static void Patch()
        {
            APState.Init();
            var harmony = new Harmony("Archipelago");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

namespace Archipelago
{
    [BepInPlugin("Archipelago", "Archipelago", Version)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public const string Version = "1.3.2";
        public static ArchipelagoPlugin Main = null;
        private void Awake()
        {
            // Plugin startup logic
            Main = this;
            var harmony = new Harmony("Archipelago");
            APState.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin Archipelago (" + Version + ") for Server (" 
                           + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2] + 
                           ") is loaded!");
        }
    }
}