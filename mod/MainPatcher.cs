using System.Reflection;
using BepInEx;
using HarmonyLib;

// QMODS interface
// namespace Archipelago
// {
//     public class MainPatcher
//     {
//         public static void Patch()
//         {
//             APState.Init();
// 
//             var harmony = new Harmony("Archipelago");
//             harmony.PatchAll(Assembly.GetExecutingAssembly());
//         }
//     }
// }
namespace Archipelago
{
    [BepInPlugin("Archipelago", "Archipelago", "1.0")]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public static ArchipelagoPlugin Main = null;
        private void Awake()
        {
            // Plugin startup logic
            Main = this;
            var harmony = new Harmony("Archipelago");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin Archipelago is loaded!");
        }
    }
}