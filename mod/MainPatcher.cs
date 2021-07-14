using System.Reflection;
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
