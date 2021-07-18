using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;

// Enforcement Platform button: (362.0, -70.3, 1082.3)

namespace Archipelago
{
#if DEBUG // Extra cheat
    //[HarmonyPatch(typeof(Player))]
    //[HarmonyPatch("Update")]
    //internal class Player_Update_Patch
    //{
    //    [HarmonyPostfix]
    //    public static void Cheats()
    //    {
    //        Player.main.oxygenMgr.AddOxygen(500.0f);
    //        Player.main.liveMixin.ResetHealth();
    //    }
    //}
#endif

    public class ArchipelagoUI : MonoBehaviour
    {
#if DEBUG
        public static string mouse_target_desc = "";
        private bool show_warps = false;
        private bool show_items = false;
        private float copied_fade = 0.0f;

        public static Dictionary<string, Vector3> WRECKS = new Dictionary<string, Vector3>
        {
            { "Blood Kelp Trench 1", new Vector3(-1201, -324, -396) },
            { "Bulb Zone 1", new Vector3(929, -198, 593) },
            { "Bulb Zone 2", new Vector3(1309, -215, 570) },
            { "Dunes 1", new Vector3(-1448, -332, 723) },
            { "Dunes 2", new Vector3(-1632, -334, 83) },
            { "Dunes 3", new Vector3(-1210, -217, 7) },
            { "Grand Reef 1", new Vector3(-290, -222, -773) },
            { "Grand Reef 2", new Vector3(-865, -430, -1390) },
            { "Grassy Plateaus 1", new Vector3(-15, -96, -624) },
            { "Grassy Plateaus 2", new Vector3(-390, -120, 648) },
            { "Grassy Plateaus 3", new Vector3(286, -72, 444) },
            { "Grassy Plateaus 4", new Vector3(-635, -50, -2) },
            { "Grassy Plateaus 5", new Vector3(-432, -90, -268) },
            { "Kelp Forest 1", new Vector3(-320, -57, 252) },
            { "Kelp Forest 2", new Vector3(65, -25, 385) },
            { "Mountains 1", new Vector3(701, -346, 1224) },
            { "Mountains 2", new Vector3(1057, -254, 1359) },
            { "Northwestern Mushroom Forest", new Vector3(-645, -120, 773) },
            { "Safe Shallows 1", new Vector3(-40, -14, -400) },
            { "Safe Shallows 2", new Vector3(366, -6, -203) },
            { "Sea Treader's Path", new Vector3(-1131, -166, -729) },
            { "Sparse Reef", new Vector3(-787, -208, -713) },
            { "Underwater Islands", new Vector3(-102, -179, 860) }
        };
#endif

#if DEBUG
        void Update()
        {
            if (mouse_target_desc != "")
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                {
                    Debug.Log("INSPECT GAME OBJECT: " + mouse_target_desc);
                    string id = mouse_target_desc.Split(new char[] { ':' })[0];
                    GUIUtility.systemCopyBuffer = id;
                    copied_fade = 1.0f;
                }
            }

            copied_fade -= Time.deltaTime;
        }
#endif

        void OnGUI()
        {
#if DEBUG
            GUI.Box(new Rect(0, 0, Screen.width, 120), "");
#endif

            if (APState.session != null && APState.session.Connected)
            {
                GUI.Label(new Rect(16, 16, 300, 20), "Archipelago Status: Connected");
            }
            else
            {
                GUI.Label(new Rect(16, 16, 300, 20), "Archipelago Status: Not Connected");
            }

#if DEBUG
            GUI.Label(new Rect(16, 16 + 20, Screen.width - 32, 50), ((copied_fade > 0.0f) ? "Copied!" : "Target: ") + mouse_target_desc);

            if (APState.is_in_game)
            {
                if (GUI.Button(new Rect(16, 16 + 25 + 8 + 25 + 8, 150, 25), "Activate Cheats"))
                {
                    DevConsole.SendConsoleCommand("nodamage");
                    DevConsole.SendConsoleCommand("oxygen");
                    DevConsole.SendConsoleCommand("item seaglide");
                    DevConsole.SendConsoleCommand("item battery 10");
                    DevConsole.SendConsoleCommand("fog");
                    DevConsole.SendConsoleCommand("speed 3");
                }
                if (GUI.Button(new Rect(16 + 150 + 8, 16 + 25 + 8 + 25 + 8, 150, 25), "Warp to Locations"))
                {
                    show_warps = !show_warps;
                    if (show_warps) show_items = false;
                }
                if (GUI.Button(new Rect(16 + 150 + 8 + 150 + 8, 16 + 25 + 8 + 25 + 8, 150, 25), "Items"))
                {
                    show_items = !show_items;
                    if (show_items) show_warps = false;
                }

                if (show_warps)
                {
                    int i = 0;
                    int j = 125;
                    foreach (var kv in WRECKS)
                    {
                        if (GUI.Button(new Rect(16 + i, j, 200, 25), kv.Key.ToString()))
                        {
                            string target = ((int)kv.Value.x).ToString() + " " +
                                            ((int)kv.Value.y).ToString() + " " +
                                            ((int)kv.Value.z + 50).ToString();
                            DevConsole.SendConsoleCommand("warp " + target);
                        }
                        j += 30;
                        if (j + 30 >= Screen.height)
                        {
                            j = 125;
                            i += 200 + 16;
                        }
                    }
                }

                if (show_items)
                {
                    int i = 0;
                    int j = 125;
                    foreach (var kv in APState.ITEM_CODE_TO_TECHTYPE)
                    {
                        if (GUI.Button(new Rect(16 + i, j, 200, 25), kv.Value.ToString()))
                        {
                            APState.unlock(kv.Value);
                        }
                        j += 30;
                        if (j + 30 >= Screen.height)
                        {
                            j = 125;
                            i += 200 + 16;
                        }
                    }
                }
            }
#endif
        }
    }

    public static class APState
    {
        public static string host;
        public static string player_name;
        public static string password;

        public static Dictionary<int, TechType> ITEM_CODE_TO_TECHTYPE = new Dictionary<int, TechType>();
        public static Dictionary<string, int> LOCATION_ADDRESS_TO_CHECK_ID = new Dictionary<string, int>();

        public static RoomInfoPacket room_info = null;
        public static DataPackagePacket data_package = null;
        public static ConnectedPacket connected_data = null;
        public static LocationInfoPacket location_infos = null;
        public static Dictionary<int, string> player_names_by_id = new Dictionary<int, string>
        {
            { 0, "Archipelago" }
        };
        public static List<TechType> unlock_queue = new List<TechType>();
        public static bool is_in_game = false;

        public static ArchipelagoSession session;

#if DEBUG
        public static string InspectGameObject(GameObject gameObject)
        {
            string msg = gameObject.transform.position.ToString().Trim() + ": ";

            Component[] components = gameObject.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var component_name = components[i].ToString().Split('(').GetLast();
                component_name = component_name.Substring(0, component_name.Length - 1);

                // Propulsion Cannon: (324.6, -104.8, 441.8): UnityEngine.Transform, UnityEngine.Rigidbody, PrefabIdentifier, Pickupable, LargeWorldEntity, WorldForces, VFXSurface, EntityTag, EcoTarget, ResourceTracker, SkyApplier, 

                //[HarmonyPostfix]
                //public static void RemoveFragment(ResourceTracker __instance)
                //{
                msg += component_name;

                if (component_name == "ResourceTracker")
                {
                    var techTypeMember = typeof(ResourceTracker).GetField("techType", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                    var techType = (TechType)techTypeMember.GetValue(component);
                    msg += $"({techType.ToString()},{((ResourceTracker)component).overrideTechType.ToString()})";
                }

                msg += ", ";
            } // Wreck8_BloodKelp_PDA1(Clone) (StoryHandTarget)

            //ErrorMessage.AddMessage("(" + gameObject.name + "):" + msg);
            //Debug.Log("INSPECT GAME OBJECT: " + msg);

            return msg;
        }
#endif

        static public void Init()
        {
            // Load connect info
            {
                var reader = File.OpenText("QMods/Archipelago/connect_info.json");
                var content = reader.ReadToEnd();
                var json = new JSONObject(content);
                reader.Close();

                host = json.GetField("host").str;
                password = json.GetField("password").str;
                player_name = json.GetField("player_name").str;
            }

            // Load items.json
            {
                var reader = File.OpenText("QMods/Archipelago/items.json");
                var content = reader.ReadToEnd();
                var json = new JSONObject(content);
                reader.Close();

                foreach (var item_json in json)
                {
                    ITEM_CODE_TO_TECHTYPE[(int)item_json.GetField("id").i] = 
                        (TechType)Enum.Parse(typeof(TechType), item_json.GetField("tech_type").str);
                }
            }

            // Load locations.json
            {
                var reader = File.OpenText("QMods/Archipelago/locations.json");
                var content = reader.ReadToEnd();
                var json = new JSONObject(content);
                reader.Close();

                foreach (var location_json in json)
                {
                    LOCATION_ADDRESS_TO_CHECK_ID[location_json.GetField("game_id").str] =
                        (int)location_json.GetField("id").i;
                }
            }
        }

        public static void Session_ErrorReceived(Exception e, string message)
        {
            Debug.LogError(message);
            if (e != null) Debug.LogError(e.ToString());
        }

        public static void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.RoomInfo:
                    {
                        room_info = packet as RoomInfoPacket;
                        updatePlayerList(room_info.Players);
                        session.SendPacket(new GetDataPackagePacket());
                        break;
                    }
                case ArchipelagoPacketType.ConnectionRefused:
                    {
                        var p = packet as ConnectionRefusedPacket;
                        foreach (string err in p.Errors)
                        {
                            Debug.LogError(err);
                        }
                        break;
                    }
                case ArchipelagoPacketType.Connected:
                    {
                        connected_data = packet as ConnectedPacket;
                        updatePlayerList(connected_data.Players);
                        break;
                    }
                case ArchipelagoPacketType.ReceivedItems:
                    {
                        var p = packet as ReceivedItemsPacket;
                        foreach (var item in p.Items)
                        {
                            if (connected_data.ItemsChecked.Contains(item.Item)) continue; // We already have it
                            connected_data.ItemsChecked.Add(item.Item);
                            connected_data.MissingChecks.Remove(item.Item);

                            var techType = ITEM_CODE_TO_TECHTYPE[item.Item];
                            unlock_queue.Add(techType);
                        }
                        break;
                    }
                case ArchipelagoPacketType.LocationInfo:
                    {
                        // This should contain all our checks
                        location_infos = packet as LocationInfoPacket;
                        break;
                    }
                case ArchipelagoPacketType.RoomUpdate:
                    {
                        var p = packet as RoomUpdatePacket;
                        // Hint points? Dont care
                        break;
                    }
                case ArchipelagoPacketType.Print:
                    {
                        var p = packet as PrintPacket;
                        ErrorMessage.AddMessage(p.Text);
                        break;
                    }
                case ArchipelagoPacketType.PrintJSON:
                    {
                        var p = packet as PrintJsonPacket;
                        string text = "";
                        foreach (var part in p.Data)
                        {
                            switch (part.Type)
                            {
                                case "player_id":
                                    {
                                        int player_id = int.Parse(part.Text);
                                        text += player_names_by_id[player_id];
                                        break;
                                    }
                                case "item_id":
                                    {
                                        int item_id = int.Parse(part.Text);
                                        text += data_package.DataPackage.ItemLookup[item_id];
                                        break;
                                    }
                                case "location_id":
                                    {
                                        int location_id = int.Parse(part.Text);
                                        text += data_package.DataPackage.LocationLookup[location_id];
                                        break;
                                    }
                                default:
                                    {
                                        text += part.Text;
                                        break;
                                    }
                            }
                        }
                        ErrorMessage.AddMessage(text);
                        break;
                    }
                case ArchipelagoPacketType.DataPackage:
                    {
                        data_package = packet as DataPackagePacket;

                        var connect_packet = new ConnectPacket();

                        connect_packet.Game = "Subnautica";
                        connect_packet.Name = player_name;
                        connect_packet.Uuid = Convert.ToString(player_name.GetHashCode(), 16);
                        connect_packet.Version = new Version(0, 1, 0);
                        connect_packet.Tags = new List<string> { "AP" };
                        connect_packet.Password = password;

                        APState.session.SendPacket(connect_packet);
                        break;
                    }
            }
        }

        public static void updatePlayerList(List<MultiClient.Net.Models.NetworkPlayer> players)
        {
            player_names_by_id = new Dictionary<int, string>
            {
                { 0, "Archipelago" }
            };

            foreach (var player in players)
            {
                player_names_by_id[player.Slot] = player.Name;
            }
        }

        public static bool checkLocation(string game_id)
        {
            if (APState.LOCATION_ADDRESS_TO_CHECK_ID.ContainsKey(game_id))
            {
                var location_id = APState.LOCATION_ADDRESS_TO_CHECK_ID[game_id];
                var location_packet = new LocationChecksPacket();
                location_packet.Locations = new List<int> { location_id };
                APState.session.SendPacket(location_packet);
                return true;
            }
            return false;
        }

        // That's the big function.
        public static void unlock(TechType techType)
        {
            if (PDAScanner.IsFragment(techType))
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

                PDAScanner.Entry entry;
                if (!PDAScanner.GetPartialEntryByKey(techType, out entry))
                {
                    var add_method = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static);
                    entry = (PDAScanner.Entry)add_method.Invoke(null, new object[] { techType, 0 });
                }

                bool unlockBlueprint = false;
                bool unlockEncyclopedia = false;
                if (entry != null)
                {
                    unlockEncyclopedia = true;
                    entry.unlocked++;
                    if (entry.unlocked >= entryData.totalFragments)
                    {
                        unlockBlueprint = true;

                        var partial = (List<PDAScanner.Entry>)typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Static).GetValue(null);

                        var complete = (HashSet<TechType>)typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Static).GetValue(null);

                        partial.Remove(entry);
                        complete.Add(entry.techType);

                        var NotifyRemove_method = typeof(PDAScanner).GetMethod("NotifyRemove", BindingFlags.NonPublic | BindingFlags.Static);
                        NotifyRemove_method.Invoke(null, new object[] { entry });
                    }
                    else
                    {
                        int totalFragments = entryData.totalFragments;
                        if (totalFragments > 1)
                        {
                            float arg = (float)Mathf.RoundToInt((float)entry.unlocked / (float)totalFragments * 100f);
                            ErrorMessage.AddError(Language.main.GetFormat<string, float, int, int>("ScannerInstanceScanned", Language.main.Get(techType.AsString(false)), arg, entry.unlocked, totalFragments));
                        }

                        var NotifyProgress_method = typeof(PDAScanner).GetMethod("NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static);
                        NotifyProgress_method.Invoke(null, new object[] { entry });

                        FMODUWE.PlayOneShot("event:/loot/pickup_default", MainCamera.camera.transform.position, 1.5f); // A bit lounder
                    }
                }

                ResourceTracker.UpdateFragments();
                if (unlockBlueprint || unlockEncyclopedia)
                {
                    var Unlock_method = typeof(PDAScanner).GetMethod("Unlock", BindingFlags.NonPublic | BindingFlags.Static);
                    Unlock_method.Invoke(null, new object[] { entryData, unlockBlueprint, unlockEncyclopedia, true });
                }
            }
            else
            {
                // Blueprint
                KnownTech.Add(techType, true);

                //var NotifyAdd_method = typeof(KnownTech).GetMethod("NotifyAdd", BindingFlags.NonPublic | BindingFlags.Static);
                //NotifyAdd_method.Invoke(null, new object[] { techType, true });

                //PDAScanner.CompleteAllEntriesWhichUnlocks(techType);

                //var NotifyChanged_method = typeof(KnownTech).GetMethod("NotifyChanged", BindingFlags.NonPublic | BindingFlags.Static);
                //NotifyChanged_method.Invoke(null, new object[] { });

                //var UnlockCompoundTech_method = typeof(KnownTech).GetMethod("UnlockCompoundTech", BindingFlags.NonPublic | BindingFlags.Static);
                //UnlockCompoundTech_method.Invoke(null, new object[] { true });

                //KnownTech.Analyze(techType, true);
            }
        }
    }

    // Remove scannable fragments as they spawn, we will unlock them from Databoxes, PDAs and Terminals.
    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch("Start")]
    internal class ResourceTracker_Start_Patch
    {
        public static List<TechType> tech_fragments_to_destroy = new List<TechType>
        {
            TechType.SeamothFragment,
            TechType.StasisRifleFragment,
            TechType.ExosuitFragment,
            TechType.TransfuserFragment,
            TechType.TerraformerFragment,
            TechType.ReinforceHullFragment,
            TechType.WorkbenchFragment,
            TechType.PropulsionCannonFragment,
            TechType.BioreactorFragment,
            TechType.ThermalPlantFragment,
            TechType.NuclearReactorFragment,
            TechType.MoonpoolFragment,
            TechType.BaseFiltrationMachineFragment,
            TechType.CyclopsHullFragment,
            TechType.CyclopsBridgeFragment,
            TechType.CyclopsEngineFragment,
            TechType.CyclopsDockingBayFragment,
            TechType.SeaglideFragment,
            TechType.ConstructorFragment,
            TechType.SolarPanelFragment,
            TechType.PowerTransmitterFragment,
            TechType.BaseUpgradeConsoleFragment,
            //TechType.BaseObservatoryFragment, // Cosmetic only, leave it in the world
            //TechType.BaseWaterParkFragment, // Cosmetic only, leave it in the world
            TechType.RadioFragment,
            //TechType.BaseRoomFragment, //TODO: Add later
            //TechType.BaseBulkheadFragment, // Cosmetic only, leave it in the world
            TechType.BatteryChargerFragment,
            TechType.PowerCellChargerFragment,
            TechType.ScannerRoomFragment,
            TechType.SpecimenAnalyzerFragment,
            //TechType.FarmingTrayFragment, //TODO: Add later - unlocks farming
            //TechType.SignFragment,
            //TechType.PictureFrameFragment,
            //TechType.BenchFragment,
            //TechType.PlanterPotFragment, //TODO: Add later - unlocks farming
            //TechType.PlanterBoxFragment, //TODO: Add later - unlocks farming
            //TechType.PlanterShelfFragment, //TODO: Add later - unlocks farming
            //TechType.AquariumFragment, // Cosmetic only, leave it in the world
            TechType.ReinforcedDiveSuitFragment,
            TechType.RadiationSuitFragment,
            TechType.StillsuitFragment,
            TechType.BuilderFragment,
            TechType.LEDLightFragment,
            TechType.TechlightFragment,
            TechType.SpotlightFragment,
            TechType.BaseMapRoomFragment,
            TechType.BaseBioReactorFragment,
            TechType.BaseNuclearReactorFragment,
            TechType.LaserCutterFragment,
            TechType.BeaconFragment,
            TechType.GravSphereFragment,
            TechType.ExosuitDrillArmFragment,
            TechType.ExosuitPropulsionArmFragment,
            TechType.ExosuitGrapplingArmFragment,
            TechType.ExosuitTorpedoArmFragment,
            TechType.ExosuitClawArmFragment,
            TechType.PrecursorKey_PurpleFragment
        };

        [HarmonyPostfix]
        public static void RemoveFragment(ResourceTracker __instance)
        {
            var techTypeMember = typeof(ResourceTracker).GetField("techType", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            var techType = (TechType)techTypeMember.GetValue(__instance);
            if (techType == TechType.Fragment)
            {
                var techTag = __instance.GetComponent<TechTag>();
                if (techTag != null)
                {
                    if (tech_fragments_to_destroy.Contains(techTag.type))
                    {
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
                else
                {
                    UnityEngine.Object.Destroy(__instance.gameObject); // No techtag, so it's just "fragment", remove it...
                }
            }
            else if (tech_fragments_to_destroy.Contains(techType)) // Not fragment, but could be one of the others
            {
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
        }
    }

    // Spawn databoxes with blank item inside
    [HarmonyPatch(typeof(DataboxSpawner))]
    [HarmonyPatch("Start")]
    internal class DataboxSpawner_Start_Patch
    {
        [HarmonyPrefix]
        public static bool ReplaceDataboxContent(DataboxSpawner __instance)
        {
            // We make sure to spawn it
            var databox = UnityEngine.Object.Instantiate<GameObject>(__instance.databoxPrefab, __instance.transform.position, __instance.transform.rotation, __instance.transform.parent);

            // Blank item inside
            BlueprintHandTarget component = databox.GetComponent<BlueprintHandTarget>();
            component.unlockTechType = (TechType)20000; // Using TechType.None gives 2 titanium we don't want that

            // Delete the spawner entity
            UnityEngine.Object.Destroy(__instance.gameObject);

            return false; // Don't call original code!
        }
    }

    // If databox was already spawned, make sure it's blank
    [HarmonyPatch(typeof(BlueprintHandTarget))]
    [HarmonyPatch("Start")]
    internal class BlueprintHandTarget_Start_Patch
    {
        public static int uid = 20000;

        [HarmonyPrefix]
        public static void ReplaceDataboxContent(BlueprintHandTarget __instance)
        {
            __instance.unlockTechType = (TechType)uid; // Using TechType.None gives 2 titanium we don't want that
            uid++;
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
            if (!__instance.used)
            {
                var databox_id = __instance.gameObject.transform.position.ToString().Trim();
                APState.checkLocation(databox_id);
            }
        }
    }

    // Once PDA clicked, send it to Archipelago.
    [HarmonyPatch(typeof(StoryHandTarget))]
    [HarmonyPatch("OnHandClick")]
    internal class StoryHandTarget_OnHandClick_Patch
    {
        [HarmonyPrefix]
        public static bool PickupPDA(StoryHandTarget __instance)
        {
            var pda_id = __instance.gameObject.transform.position.ToString().Trim();
            if (APState.checkLocation(pda_id))
            {
                var generic_console = __instance.gameObject.GetComponent<GenericConsole>();
                if (generic_console != null)
                {
                    // Change it's color
                    generic_console.gotUsed = true;

                    var UpdateState_method = typeof(GenericConsole).GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                    UpdateState_method.Invoke(generic_console, new object[] {});

                    return false; // Don't let the item in the console be given. (Like neptune blueprint)
                }
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
            var pickup_id = __instance.gameObject.transform.position.ToString().Trim();
            if (APState.checkLocation(pickup_id))
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

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("LoadInitialInventoryAsync")]
    internal class MainGameController_LoadInitialInventoryAsync_Patch
    {
        [HarmonyPostfix]
        public static void GameReady()
        {
            APState.is_in_game = true;
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("OnDestroy")]
    internal class MainGameController_OnDestroy_Patch
    {
        [HarmonyPostfix]
        public static void GameClosing()
        {
            APState.is_in_game = false;
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("Update")]
    internal class MainGameController_Update_Patch
    {
        [HarmonyPostfix]
        public static void DequeueUnlocks()
        {
            if (APState.is_in_game)
            {
                foreach (var unlock in APState.unlock_queue)
                {
                    APState.unlock(unlock);
                }
                APState.unlock_queue.Clear();
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
            var gui_gameobject = new GameObject();
            gui_gameobject.AddComponent<ArchipelagoUI>();
            GameObject.DontDestroyOnLoad(gui_gameobject);

            // Start the archipelago session. (This will freeze the game)
            APState.session = new ArchipelagoSession("ws://" + APState.host);
            APState.session.PacketReceived += APState.Session_PacketReceived;
            APState.session.ErrorReceived += APState.Session_ErrorReceived;
            APState.session.Connect();
        }
    }

#if DEBUG
    [HarmonyPatch(typeof(GUIHand))]
    [HarmonyPatch("OnUpdate")]
    internal class GUIHand_OnUpdate_Patch
    {
        [HarmonyPostfix]
        public static void OnUpdate(GUIHand __instance)
        {
            var active_target = __instance.GetActiveTarget();
            if (active_target)
                ArchipelagoUI.mouse_target_desc = APState.InspectGameObject(active_target.gameObject);
            else
                ArchipelagoUI.mouse_target_desc = "";
        }
    }
#endif

    // Ship start already exploded
    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("StopIntroCinematic")]
    internal class EscapePod_StopIntroCinematic_Patch
    {
        [HarmonyPostfix]
        public static void ExplodeShip(EscapePod __instance)
        {
            DevConsole.SendConsoleCommand("explodeship");
        }
    }
}
