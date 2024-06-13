using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Archipelago.MultiClient.Net.Packets;
using System.Text;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using TMPro;
using Debug = UnityEngine.Debug;
using File = System.IO.File;
using Object = UnityEngine.Object;


namespace Archipelago
{
    public class ArchipelagoUI : MonoBehaviour
    {
        void OnGUI()
        {
            if (APState.state == APState.State.Cancelled)
            {
                return;
            }

            string ap_ver = "Archipelago v" + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2];
            if (APState.Session != null)
            {
                if (APState.Authenticated)
                {
                    GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Connected");
                }
                else
                {
                    GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Authentication failed");
                }
            }
            else
            {
                GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Not Connected");
            }

            if ((APState.Session == null || !APState.Authenticated) && APState.state == APState.State.Menu)
            {
                GUI.Label(new Rect(16, 36, 100, 20), "Host: ");
                GUI.Label(new Rect(16, 56, 100, 20), "PlayerName: ");
                GUI.Label(new Rect(16, 76, 100, 20), "Password: ");
                GUI.Label(new Rect(16, 96, 100, 20), "Game Name: ");

                bool submit = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;

                APState.ServerConnectInfo.host_name = GUI.TextField(new Rect(100 + 16 + 8, 36, 150, 20),
                    APState.ServerConnectInfo.host_name);
                APState.ServerConnectInfo.slot_name = GUI.TextField(new Rect(100 + 16 + 8, 56, 150, 20),
                    APState.ServerConnectInfo.slot_name);
                APState.ServerConnectInfo.password = GUI.TextField(new Rect(100 + 16 + 8, 76, 150, 20),
                    APState.ServerConnectInfo.password);
                APState.ServerConnectInfo.game_name = GUI.TextField(new Rect(100 + 16 + 8, 96, 150, 20),
                    APState.ServerConnectInfo.game_name);

                if (submit && Event.current.type == EventType.KeyDown)
                {
                    // The text fields have not consumed the event, which means they were not focused.
                    submit = false;
                }

                if (GUI.Button(new Rect(16, 120, 100, 20), "Cancel"))
                {
                    APState.state = APState.State.Cancelled;
                    return;
                }
                if ((GUI.Button(new Rect(136, 120, 100, 20), "Connect") || submit) && APState.ServerConnectInfo.Valid)
                {
                    APState.Connect();
                }
            }
            else if (APState.state == APState.State.InGame && APState.Session != null && Player.main != null)
            {

                if (APState.TrackedMode != TrackerMode.Disabled)
                {
                    string text = "Locations left: " + APState.TrackedLocationsCount;
                    if (APState.TrackedLocation != -1)
                    {
                        text += ". Closest is " + (long)APState.TrackedDistance + " m ("
                                + (int)APState.TrackedAngle + "°) away";
                        text += ", named " + APState.TrackedLocationName;
                        text += " (" + APState.TrackedDepth + "m)";
                    }

                    GUI.Label(new Rect(16, 36, 1000, 20), text);
                }

                int showing_fish = 0;
                if (APState.TrackedFishCount > 0 && APState.TrackedMode != TrackerMode.Disabled)
                {
                    showing_fish = 1;
                    GUI.Label(new Rect(16, 56, 1000, 22),
                        "Fish left: " + APState.TrackedFishCount + ". Such as: " + APState.TrackedFish);
                }

                int showing_plants = 0;
                if (APState.TrackedPlantCount > 0 && APState.TrackedMode != TrackerMode.Disabled)
                {
                    showing_plants = 1;
                    int y_pos = 56 + (showing_fish * 20);
                    GUI.Label(new Rect(16, y_pos, 1000, 22),
                        "Plants left: " + APState.TrackedPlantCount + ". Such as: " + APState.TrackedPlants);
                }

                if (PlayerNearStart())
                {
                    int y_pos = 56 + (showing_fish * 20) + (showing_plants * 20);
                    GUI.Label(new Rect(16, y_pos, 1000, 22), "Goal: " + APState.Goal);

                    if (APState.SwimRule == 0)
                    {
                        GUI.Label(new Rect(16, y_pos + 20, 1000, 22),
                            "No Swim Rule sent by Server. Assuming items_hard." +
                            " Current Logical Depth: " + (TrackerThread.LogicSwimDepth +
                                                          TrackerThread.LogicVehicleDepth));
                    }
                    else
                    {
                        string depth_string =
                            "Current Logical Depth: "
                            + (TrackerThread.LogicSwimDepth + TrackerThread.LogicItemDepth + TrackerThread.LogicVehicleDepth)
                            + " = "
                            + TrackerThread.LogicSwimDepth + " (Swim) + ";

                        if (APState.ConsiderItems)
                        {
                            depth_string += TrackerThread.LogicItemDepth + " (items) + ";
                        }

                        depth_string += TrackerThread.LogicVehicleDepth
                            + " (" + TrackerThread.LogicVehicle + ")";

                        GUI.Label(new Rect(16, y_pos + 20, 1000, 22), depth_string);
                    }
                }
                if (!APState.TrackerProcessing.IsAlive)
                {
                    GUI.Label(new Rect(16, 76, 1000, 22),
                        "Error: Tracker Thread died. Tracker will not update.");
                }
            }
        }

        public bool PlayerNearStart()
        {
            if (APState.state != APState.State.InGame)
            {
                return false;
            }

            if (ArchipelagoPlugin.Zero)
            {
                return true;
            }

            var pod = ArchipelagoPlugin.SubnauticaEscapePod.GetField("main")?.GetValue(ArchipelagoPlugin.SubnauticaEscapePod);
            if (pod is null)
            {
                return false;
            }
            //EscapePod.main.transform
            var podTransform = ArchipelagoPlugin.SubnauticaEscapePod.GetProperty("transform")?.GetValue(pod) as Transform;
            if (podTransform is null)
            {
                return false;
            }
            return (podTransform.position - Player.main.transform.position).magnitude < 10f;
        }

        private void Start()
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }
            RegisterCmds();
        }

        public void RegisterCmds()
        {
            DevConsole.RegisterConsoleCommand(this, "say", false, false);
            DevConsole.RegisterConsoleCommand(this, "silent", false, false);
            DevConsole.RegisterConsoleCommand(this, "tracker", false, false);
            DevConsole.RegisterConsoleCommand(this, "deathlink", false, false);
            DevConsole.RegisterConsoleCommand(this, "resync", false, false);
            DevConsole.RegisterConsoleCommand(this, "apdebug", false, false);
        }

        [HarmonyPatch(typeof(ConsoleInput))]
        [HarmonyPatch("Validate")]
        internal class ConsoleHook
        {
            [HarmonyPrefix]
            private static bool AllowExclamationPoint(string text, int pos, char ch, ref char __result)
            {
                if (ch == '!')
                {
                    __result = ch;
                    return false;
                }

                return true;
            }
        }

        private void OnConsoleCommand_say(NotificationCenter.Notification n)
        {
            string text = "";

            for (var i = 0; i < n.data.Count; i++)
            {
                text += (string)n.data[i];
                if (i < n.data.Count - 1) text += " ";
            }

            if (APState.Session != null && APState.Authenticated)
            {
                var packet = new SayPacket();
                packet.Text = text;
                APState.Session.Socket.SendPacket(packet);
            }
            else
            {
                Debug.Log("Can only 'say' while connected to Archipelago.");
                ErrorMessage.AddMessage("Can only 'say' while connected to Archipelago.");
            }
        }
        private void OnConsoleCommand_silent(NotificationCenter.Notification n)
        {
            APState.Silent = !APState.Silent;

            if (APState.Silent)
            {
                Debug.Log("Muted Archipelago chat.");
                ErrorMessage.AddMessage("Muted Archipelago chat.");
                APState.message_queue.Clear();
            }
            else
            {
                Debug.Log("Enabled Archipelago chat.");
                ErrorMessage.AddMessage("Enabled Archipelago chat.");
            }
        }
        private void OnConsoleCommand_tracker(NotificationCenter.Notification n)
        {
            switch (APState.TrackedMode)
            {
                case TrackerMode.Disabled:
                    APState.TrackedMode = TrackerMode.Closest;
                    Debug.Log("Tracking Locations by proximity.");
                    ErrorMessage.AddMessage("Tracking Locations by proximity.");
                    break;
                case TrackerMode.Closest:
                    APState.TrackedMode = TrackerMode.Logical;
                    Debug.Log("Tracking Locations by proximity and filtering by logic");
                    ErrorMessage.AddMessage("Tracking Locations by proximity and filtering by logic");
                    break;
                case TrackerMode.Logical:
                    APState.TrackedMode = TrackerMode.Disabled;
                    Debug.Log("Location tracking disabled.");
                    ErrorMessage.AddMessage("Location tracking disabled.");
                    break;
            }
        }
        private void OnConsoleCommand_deathlink(NotificationCenter.Notification n)
        {
            APState.ServerConnectInfo.death_link = !APState.ServerConnectInfo.death_link;
            APState.set_deathlink();

            if (APState.ServerConnectInfo.death_link)
            {
                Debug.Log("Enabled DeathLink.");
                ErrorMessage.AddMessage("Enabled DeathLink.");
            }
            else
            {
                Debug.Log("Disabled DeathLink.");
                ErrorMessage.AddMessage("Disabled DeathLink.");
            }
        }

        private void OnConsoleCommand_resync(NotificationCenter.Notification n)
        {
            if (APState.state == APState.State.InGame)
            {
                Debug.Log("Beginning Item resync.");
                ErrorMessage.AddMessage("Beginning Item resync.");
                APState.Resync();
                Debug.Log("Item resync completed.");
                ErrorMessage.AddMessage("Item resync completed.");
            }
            else
            {
                Debug.Log("Cannot resync in menu.");
                ErrorMessage.AddMessage("Cannot resync in menu.");
            }
        }

        private void OnConsoleCommand_apdebug(NotificationCenter.Notification n)
        {
            //var loc = APState.TrackedLocation;
            //var loc_data = APState.LOCATIONS[loc];
            //DevConsole.SendConsoleCommand("warp "+(int)loc_data.Position.x+" "+(int)loc_data.Position.y+" "+(int)loc_data.Position.z);

            //Debug.LogError("Analysis:");
            //string json = JsonConvert.SerializeObject(Player.main.pdaData.analysisTech);
            //Debug.LogError(json);
        }
    }

    // Remove scannable fragments as they spawn, we will unlock them from Databoxes, PDAs and Terminals.
    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch("Start")]
    internal class ResourceTracker_Start_Patch
    {
        [HarmonyPostfix]
        public static void RemoveFragment(ResourceTracker __instance, TechType ___techType)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            if (___techType == TechType.Fragment)
            {
                var techTag = __instance.GetComponent<TechTag>();
                if (techTag != null)
                {
                    if (APState.TechFragmentsToDestroy.Contains(techTag.type))
                    {
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
                else
                {
                    UnityEngine.Object.Destroy(__instance.gameObject); // No techtag, so it's just "fragment", remove it...
                }
            }
            else if (APState.TechFragmentsToDestroy.Contains(___techType)) // Not fragment, but could be one of the others
            {
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("UpdateTarget")]
    internal class PDAScanner_UpdateTarget_Patch
    {
        [HarmonyPostfix]
        public static void MakeUnscanable()
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            if (PDAScanner.scanTarget.gameObject)
            {
                var tech_tag = PDAScanner.scanTarget.gameObject.GetComponent<TechTag>();
                if (tech_tag != null)
                {
                    if (APState.TechFragmentsToDestroy.Contains(tech_tag.type))
                    {
                        PDAScanner.scanTarget.Invalidate();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BlueprintHandTarget))]
    [HarmonyPatch("Start")]
    internal class BlueprintHandTarget_Start_Patch
    {
        // Using TechType.None gives 2 titanium we don't want that
        [HarmonyPrefix]
        public static void ReplaceDataboxContent(BlueprintHandTarget __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            // needs to be a unique not taken ID
            __instance.unlockTechType = (TechType)__instance.transform.position.x+100000;
        }
    }

    [HarmonyPatch(typeof(DataboxSpawner))]
    [HarmonyPatch("Start")]
    internal class DataboxSpawner_Start_Patch
    {
        [HarmonyPrefix]
        public static bool AlwaysSpawn(DataboxSpawner __instance, ref IEnumerator __result)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            __result = PatchedStart(__instance);
            return false;
        }

        private static IEnumerator PatchedStart(DataboxSpawner __instance)
        {
            if (__instance.spawnTechType != 0)
            {
                yield return AddressablesUtility.InstantiateAsync(__instance.databoxPrefabReference.RuntimeKey as string,
                    __instance.transform.parent, __instance.transform.localPosition, __instance.transform.localRotation);
            }
            Object.Destroy(__instance.gameObject);
        }
    }

    // Once databox clicked, send it to Archipelago
    [HarmonyPatch(typeof(BlueprintHandTarget))]
    [HarmonyPatch("UnlockBlueprint")]
    internal class BlueprintHandTarget_UnlockBlueprint_Patch
    {
        [HarmonyPrefix]
        public static void OpenDatabox(BlueprintHandTarget __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            if (!__instance.used)
            {
                APState.CheckLocation(__instance.gameObject.transform.position);
            }
        }
    }

    // Once PDA clicked, send it to Archipelago.
    [HarmonyPatch(typeof(StoryHandTarget))]
    [HarmonyPatch("OnHandClick")]
    internal class StoryHandTarget_OnHandClick_Patch
    {
        [HarmonyPrefix]
        public static bool Interact(StoryHandTarget __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            APState.CheckLocation(__instance.gameObject.transform.position);

            var generic_console = __instance.gameObject.GetComponent<GenericConsole>();
            if (generic_console != null)
            {
                // Change its color
                generic_console.gotUsed = true;

                var UpdateState_method = typeof(GenericConsole).GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                UpdateState_method.Invoke(generic_console, new object[] { });

                return false; // Don't let the item in the console be given. (Like neptune blueprint)
            }

            return true;
        }
    }

    // There are 3 pickupable modules in the game
    [HarmonyPatch(typeof(Pickupable))]
    [HarmonyPatch("OnHandClick")]
    internal class Pickupable_OnHandClick_Patch
    {
        [HarmonyPrefix]
        public static bool PickModule(Pickupable __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            if (APState.CheckLocation(__instance.gameObject.transform.position))
            {
                var tech_tag = __instance.gameObject.GetComponent<TechTag>();
                if (tech_tag != null)
                {
                    if (tech_tag.type == TechType.VehicleHullModule1 ||
                        tech_tag.type == TechType.VehicleStorageModule ||
                        tech_tag.type == TechType.PowerUpgradeModule)
                    {
                        // Don't let the module in the console be given
                        UnityEngine.Object.Destroy(__instance.gameObject);
                        return false;
                    }
                }
            }
            return true;
        }
    }


    /* The next 3x patches graft the Cyclops Shield Module onto
       the Moonpool Fabricator if the goal is launch but the Cyclops is not in game */
    [HarmonyPatch(typeof(CraftTree))]
    [HarmonyPatch("SeamothUpgradesScheme")]
    internal class MoonpoolFabricator
    {
        [HarmonyPrefix]
        public static bool AdditionalShieldGenerator(ref CraftNode __result)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            // We only need to run this if the Cyclops isn't in game and we're trying to goal
            if (APState.CyclopsState != APState.Inclusion.Excluded || APState.Goal != "launch")
            {
                return true;
            }

            __result = new CraftNode("Root").AddNode(
                new CraftNode("CommonModules", TreeAction.Expand).AddNode(
                    new CraftNode("VehicleArmorPlating", TreeAction.Craft, TechType.VehicleArmorPlating),
                    new CraftNode("VehiclePowerUpgradeModule", TreeAction.Craft, TechType.VehiclePowerUpgradeModule),
                    new CraftNode("VehicleStorageModule", TreeAction.Craft, TechType.VehicleStorageModule)
                ),
                new CraftNode("SeamothModules", TreeAction.Expand).AddNode(
                    new CraftNode("VehicleHullModule1", TreeAction.Craft, TechType.VehicleHullModule1),
                    new CraftNode("SeamothSolarCharge", TreeAction.Craft, TechType.SeamothSolarCharge),
                    new CraftNode("SeamothElectricalDefense", TreeAction.Craft, TechType.SeamothElectricalDefense),
                    new CraftNode("SeamothTorpedoModule", TreeAction.Craft, TechType.SeamothTorpedoModule),
                    new CraftNode("SeamothSonarModule", TreeAction.Craft, TechType.SeamothSonarModule)
                ),
                new CraftNode("ExosuitModules", TreeAction.Expand).AddNode(
                    new CraftNode("ExoHullModule1", TreeAction.Craft, TechType.ExoHullModule1),
                    new CraftNode("ExosuitThermalReactorModule", TreeAction.Craft, TechType.ExosuitThermalReactorModule),
                    new CraftNode("ExosuitJetUpgradeModule", TreeAction.Craft, TechType.ExosuitJetUpgradeModule),
                    new CraftNode("ExosuitPropulsionArmModule", TreeAction.Craft, TechType.ExosuitPropulsionArmModule),
                    new CraftNode("ExosuitGrapplingArmModule", TreeAction.Craft, TechType.ExosuitGrapplingArmModule),
                    new CraftNode("ExosuitDrillArmModule", TreeAction.Craft, TechType.ExosuitDrillArmModule),
                    new CraftNode("ExosuitTorpedoArmModule", TreeAction.Craft, TechType.ExosuitTorpedoArmModule)
                ),
                new CraftNode("Torpedoes", TreeAction.Expand).AddNode(
                    new CraftNode("WhirlpoolTorpedo", TreeAction.Craft, TechType.WhirlpoolTorpedo),
                    new CraftNode("GasTorpedo", TreeAction.Craft, TechType.GasTorpedo)
                ),
                new CraftNode("CyclopsModules", TreeAction.Expand).AddNode(
                    new CraftNode("CyclopsShieldModule", TreeAction.Craft, TechType.CyclopsShieldModule)
                )
            );

            return false;
        }
    }

    [HarmonyPatch(typeof(Language))]
    [HarmonyPatch("Get")]
    internal class MoonpoolFabricator_CraftNodeText
    {
        [HarmonyPrefix]
        public static bool RewriteCraftNodeText(ref string __result, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return true;
            }

            // This is our own made-up string, it won't match anything else.
            if (key == "SeamothUpgradesMenu_CyclopsModules")
            {
                // Use a built-in replacement so it's translated properly for us
                // comes out as "Cyclops Upgrades" (there is no "Cyclops modules" translated string)
                __result = Language.main.Get("TechCategoryCyclopsUpgrades");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SpriteManager))]
    [HarmonyPatch("Get")]
    [HarmonyPatch(new Type[] { typeof(SpriteManager.Group), typeof(string) })]
    internal class MoonpoolFabricator_CraftNodeIcon
    {
        [HarmonyPrefix]
        public static bool RewriteCraftNodeIcon(ref Atlas.Sprite __result, SpriteManager.Group group, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            // our made-up string, which otherwise returns the default (question-mark) icon
            if (name == "SeamothUpgrades_CyclopsModules")
            {
                // This sprite comes from the vehicle fabricator
                __result = SpriteManager.Get(TechType.Cyclops);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("LoadInitialInventoryAsync")]
    internal class MainGameController_LoadInitialInventoryAsync_Patch
    {
        [HarmonyPostfix]
        public static void GameReady()
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            // Make sure the commands are registered
            APState.ArchipelagoUI.RegisterCmds();
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager.GameInfo))]
    [HarmonyPatch("SaveIntoCurrentSlot")]
    internal class GameInfo_SaveIntoCurrentSlot_Patch
    {
        [HarmonyPostfix]
        public static void SaveIntoCurrentSlot(SaveLoadManager.GameInfo info)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(APState.ServerConnectInfo));
            Platform.IO.File.WriteAllBytes(Platform.IO.Path.Combine(SaveLoadManager.GetTemporarySavePath(),
                "archipelago.json"), bytes);
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("SetCurrentSlot")]
    internal class SaveLoadManager_SetCurrentSlot_Patch
    {
        [HarmonyPostfix]
        public static void LoadArchipelagoState(string _currentSlot)
        {
            var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            var rawPath = storage.GetType().GetField("savePath",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(storage);
            var path = Platform.IO.Path.Combine((string)rawPath, _currentSlot);

            path = Platform.IO.Path.Combine(path, "archipelago.json");
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    APState.ServerConnectInfo = JsonConvert.DeserializeObject<APConnectInfo>(reader.ReadToEnd());

                    if (APState.Connect() && APState.ServerConnectInfo.@checked != null)
                    {
                        APState.Session.Locations.CompleteLocationChecks(APState.ServerConnectInfo.@checked.ToArray());
                    }
                    else
                    {
                        ErrorMessage.AddError("Null Checked");
                    }
                }
            }
            // compat handling, remove later
            else if (APState.archipelago_indexes.ContainsKey(_currentSlot))
            {
                APState.ServerConnectInfo.index = APState.archipelago_indexes[_currentSlot];
            }
            else
            {
                APState.ServerConnectInfo.index = 0;
            }
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("OnDestroy")]
    internal class MainGameController_OnDestroy_Patch
    {
        [HarmonyPostfix]
        public static void GameClosing()
        {
            APState.state = APState.State.Menu;
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("RegisterSaveGame")]
    internal class SaveLoadManager_RegisterSaveGame_Patch
    {
        [HarmonyPrefix]
        public static void RegisterSaveGame(string slotName, UserStorageUtils.LoadOperation loadOperation)
        {
            if (loadOperation.GetSuccessful())
            {
                byte[] jsonData = null;
                if (loadOperation.files.TryGetValue("gameinfo.json", out jsonData))
                {
                    try
                    {
                        var json_string = Encoding.UTF8.GetString(jsonData);
                        var splits = json_string.Split(new char[] { ',' });
                        var last = splits[splits.Length - 1];
                        splits = last.Split(new char[] { ':' });
                        var name = splits[0];
                        name = name.Substring(1, name.Length - 2);
                        splits = splits[1].Split(new char[] { '}' });
                        var value = splits[0];

                        if (name == "archipelago_item_index")
                        {
                            var index = int.Parse(value);
                            APState.archipelago_indexes[slotName] = index;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("archipelago_item_index error: " + e.Message);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuLoadPanel))]
    [HarmonyPatch("UpdateLoadButtonState")]
    internal class MainMenuPanel_ButtonName
    {
        [HarmonyPostfix]
        private static void MainMenuPanel_ButtonAddName(MainMenuLoadButton lb)
        {
            string txt = lb.saveGameLengthText.text;

            // Check that we're not overloading something important
            string[] error_strings = new string[] {
                "DamagedSavedGame",
                "IncompatibleChangesetSavedGame",
                "SlotEmpty"
            };
            foreach (string errstr in error_strings)
            {
                // It'll be surrounded by color text, so we can't check for equivalence
                if (txt.Contains(Language.main.Get(errstr)))
                {
                    return;
                }
            }

            // Get name from archipelago text
            var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            var rawPath = storage.GetType().GetField("savePath",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(storage);
            var saveConnectInfo = APLastConnectInfo.LoadFromFile(rawPath + "/" + lb.saveGame + "/archipelago.json");
            if (saveConnectInfo is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(saveConnectInfo.game_name))
            {
                lb.saveGameLengthText.text = saveConnectInfo.game_name;
            }
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("Update")]
    internal class MainGameControllerUpdatePatch
    {
        private static bool IsSafeToUnlock()
        {
            if (!Player.main.playerController.inputEnabled)
            {
                return false;
            }
            if (APState.unlock_dequeue_timeout > 0.0f)
            {
                return false;
            }

            if (APState.state != APState.State.InGame)
            {
                return false;
            }

            if (!ArchipelagoPlugin.Zero && SubnauticaCinematicPlaying())
            {
                return false;
            }

            if (PlayerCinematicController.cinematicModeCount > 0 && Time.time - PlayerCinematicController.cinematicActivityExpireTime <= 30f)
            {
                return false;
            }

            return !SaveLoadManager.main.isSaving;
        }

        private static bool SubnauticaCinematicPlaying()
        {
            return LaunchRocket.isLaunching || (EscapePod.main != null && EscapePod.main.IsPlayingIntroCinematic());
        }

        [HarmonyPostfix]
        public static void DequeueUnlocks()
        {
            const int dequeueCount = 2;
            const float dequeueTime = 3.0f;

            if (APState.unlock_dequeue_timeout > 0.0f) APState.unlock_dequeue_timeout -= Time.deltaTime;
            if (APState.message_dequeue_timeout > 0.0f) APState.message_dequeue_timeout -= Time.deltaTime;

            // Print messages
            if (APState.message_dequeue_timeout <= 0.0f)
            {
                // We only do x at a time. To not crowd the on screen log/events too fast
                List<string> to_process = new List<string>();
                while (to_process.Count < dequeueCount && APState.message_queue.Count > 0)
                {
                    to_process.Add(APState.message_queue[0]);
                    APState.message_queue.RemoveAt(0);
                }
                foreach (var message in to_process)
                {
                    ErrorMessage.AddMessage(message);
                }
                APState.message_dequeue_timeout = dequeueTime;
            }

            // Do unlocks
            if (IsSafeToUnlock())
            {
                if (APState.ServerConnectInfo.index < APState.Session.Items.AllItemsReceived.Count)
                {
                    APState.Unlock(APState.Session.Items.AllItemsReceived[
                        Convert.ToInt32(APState.ServerConnectInfo.index)].Item, APState.ServerConnectInfo.index);
                    APState.ServerConnectInfo.index++;
                    // We only do x at a time. To not crowd the on screen log/events too fast
                    APState.unlock_dequeue_timeout = dequeueTime;
                    // When at end of queue, validate all item counts.
                    // For some unknown reason, items may be missed sometimes in MultiClient.Net games,
                    // though Subnautica seems to have been particularly susceptible to that.
                    // Regardless, this workaround should take care of the problem.
                    if (APState.ServerConnectInfo.index == APState.Session.Items.AllItemsReceived.Count)
                    {
                        APState.Resync();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch("Start")]
    internal class MainMenuController_Start_Patch
    {
        [HarmonyPostfix]
        public static void CreateArchipelagoUI()
        {
            // Create a game object that will be responsible to drawing the IMGUI in the Menu.
            var guiGameobject = new GameObject();
            APState.ArchipelagoUI = guiGameobject.AddComponent<ArchipelagoUI>();
            GameObject.DontDestroyOnLoad(guiGameobject);
            var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            var rawPath = storage.GetType().GetField("savePath",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(storage);
            var lastConnectInfo = APLastConnectInfo.LoadFromFile(rawPath + "/archipelago_last_connection.json");
            if (lastConnectInfo != null)
            {
                APState.ServerConnectInfo.FillFromLastConnect(lastConnectInfo);
            }
        }
    }

    [HarmonyPatch(typeof(Story.UnlockBlueprintData))]
    [HarmonyPatch("Trigger")]
    internal class UnlockBlueprintData_Trigger_Patch
    {
        [HarmonyPrefix]
        public static bool PreventStoryUnlock(Story.UnlockBlueprintData __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            switch (__instance.techType)
            {
                case TechType.BaseLargeRoom:
                case TechType.PrecursorIonBattery:
                case TechType.PrecursorIonPowerCell:
                case TechType.RadiationSuit:
                    return false;
                default:
                    return true;
            }
        }
    }

    // Different target method signature based on game, manual patching is done.
    internal class CustomPDA
    {
        public static void Add(string key, PDAEncyclopedia.Entry entry)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }
            if (ArchipelagoData.Encyclopdia.TryGetValue(key, out var id))
            {
                APState.SendLocID(id);
            }
        }
    }

    [HarmonyPatch(typeof(Player), "OnKill")]
    internal class CustomPlayerKill
    {
        [HarmonyPostfix]
        public static void PlayerDeath(DamageType damageType)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }
            if (!APState.DeathLinkKilling)
            {
                if (APState.ServerConnectInfo.death_link)
                {
                    APState.DeathLinkService.SendDeathLink(new DeathLink(APState.ServerConnectInfo.slot_name));
                }
            }
            APState.DeathLinkKilling = false;
        }
    }

    // Subnautica specific hooks
    // Ship start already exploded
    internal class EscapePod_StopIntroCinematic_Patch
    {
        [HarmonyPostfix]
        public static void GameReady(EscapePod __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }
            DevConsole.SendConsoleCommand("explodeship");
            APState.ServerConnectInfo.index = 0; // New game detected
        }
    }

    // Advance rocket stage, but don't add to known tech the next stage! We'll find them in the world
    internal class Rocket_AdvanceRocketStage_Patch
    {
        [HarmonyPrefix]
        public static bool AdvanceRocketStage(Rocket __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            __instance.currentRocketStage++;
            if (__instance.currentRocketStage == 5)
            {
                var isFinishedMember = typeof(Rocket).GetField("isFinished", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
                isFinishedMember.SetValue(__instance, true);

                var IsAnyRocketReadyMember = typeof(Rocket).GetProperty("IsAnyRocketReady", BindingFlags.Static);
                IsAnyRocketReadyMember.SetValue(null, true);
            }
            //KnownTech.Add(__instance.GetCurrentStageTech(), true); // This is the part we don't want

            return false;
        }
    }

    internal class RocketConstructor_StartRocketConstruction_Patch
    {
        [HarmonyPrefix]
        public static bool StartRocketConstruction(RocketConstructor __instance)
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            TechType currentStageTech = __instance.rocket.GetCurrentStageTech();
            if (!KnownTech.Contains(currentStageTech))
            {
                return false;
            }

            return true;
        }
    }
    // When launching the rocket, send goal achieved to archipelago
    internal class LaunchRocket_SetLaunchStarted_Patch
    {
        [HarmonyPrefix]
        public static bool SetLaunchStarted()
        {
            if (APState.state != APState.State.InGame)
            {
                return true;
            }

            APState.send_completion();
            return true;
        }
    }

    [HarmonyPatch(typeof(StoryGoalCustomEventHandler))]
    [HarmonyPatch("NotifyGoalComplete")]
    internal class StoryGoalCustomEventHandler_NotifyGoalComplete_Patch
    {
        [HarmonyPrefix]
        public static void NotifyGoalComplete(string key)
        {
            if (APState.state != APState.State.InGame)
            {
                return;
            }

            if (key == APState.GoalEvent)
            {
                APState.send_completion();
            }
        }
    }
}
