using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System.Text;

// Enforcement Platform button: (362.0, -70.3, 1082.3)

namespace Archipelago
{
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

            if (APState.session == null)
            {
                GUI.Label(new Rect(16, 36, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 56, 150, 20), "Password: ");
                GUI.Label(new Rect(16, 76, 150, 20), "PlayerName: ");

                APState.host = GUI.TextField(new Rect(150 + 16 + 8, 36, 150, 20), APState.host);
                APState.password = GUI.TextField(new Rect(150 + 16 + 8, 56, 150, 20), APState.password);
                APState.player_name = GUI.TextField(new Rect(150 + 16 + 8, 76, 150, 20), APState.player_name);

                if (GUI.Button(new Rect(16, 96, 100, 20), "Connect"))
                {
                    // Start the archipelago session.
                    // Start the archipelago session.
                    var url = APState.host;
                    if (!url.Contains(":"))
                    {
                        url += ":38281";
                    }
                    if (!url.Contains("ws://"))
                    {
                        url = "ws://" + url;
                    }
                    APState.session = new ArchipelagoSession(url);
                    APState.session.PacketReceived += APState.Session_PacketReceived;
                    APState.session.ErrorReceived += APState.Session_ErrorReceived;
                    APState.session.Connect();
                }
            }

#if DEBUG
            GUI.Label(new Rect(16, 16 + 20, Screen.width - 32, 50), ((copied_fade > 0.0f) ? "Copied!" : "Target: ") + mouse_target_desc);

            if (APState.state == APState.State.InGame)
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
                            APState.unlock_queue.Add(kv.Value);
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

        private void Start()
        {
            DevConsole.RegisterConsoleCommand(this, "say", false, false);
        }

        private void OnConsoleCommand_say(NotificationCenter.Notification n)
        {
            var text = "";

            for (var i = 0; i < n.data.Count; i++)
            {
                text += (string)n.data[i];
                if (i < n.data.Count - 1) text += " ";
            }

            // Cannot type the '!' character in subnautica console, will use / instead and replace them
            text = text.Replace('/', '!');

            if (APState.session != null)
            {
                var packet = new SayPacket();
                packet.Text = text;
                APState.session.SendPacket(packet);
            }
        }
    }

    public static class APState
    {
        public struct Location
        {
            public int id;
            public Vector3 position;
        }

        public enum State
        {
            Menu,
            AwaitingSync,
            InGame
        }

        public static string host = "";
        public static string player_name = "";
        public static string password = "";

        public static Dictionary<int, TechType> ITEM_CODE_TO_TECHTYPE = new Dictionary<int, TechType>();
        public static List<Location> LOCATIONS = new List<Location>();

        public static Dictionary<string, int> archipelago_indexes = new Dictionary<string, int>();
        public static RoomInfoPacket room_info = null;
        public static DataPackagePacket data_package = null;
        public static ConnectedPacket connected_data = null;
        public static LocationInfoPacket location_infos = null;
        public static Dictionary<int, string> player_names_by_id = new Dictionary<int, string>
        {
            { 0, "Archipelago" }
        };
        public static List<TechType> unlock_queue = new List<TechType>();
        public static State state = State.Menu;
        public static int next_item_index = 0;

        public static ArchipelagoSession session;

#if DEBUG
        public static string InspectGameObject(GameObject gameObject)
        {
            string msg = gameObject.transform.position.ToString().Trim() + ": ";

            var tech_tag = gameObject.GetComponent<TechTag>();
            if (tech_tag != null)
            {
                msg += "(" + tech_tag.type.ToString() + ")";
            }

            Component[] components = gameObject.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var component_name = components[i].ToString().Split('(').GetLast();
                component_name = component_name.Substring(0, component_name.Length - 1);

                msg += component_name;

                if (component_name == "ResourceTracker")
                {
                    var techTypeMember = typeof(ResourceTracker).GetField("techType", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                    var techType = (TechType)techTypeMember.GetValue(component);
                    msg += $"({techType.ToString()},{((ResourceTracker)component).overrideTechType.ToString()})";
                }

                msg += ", ";
            }

            return msg;
        }
#endif

        static public void Init()
        {
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
                    Location location = new Location();
                    location.id = (int)location_json.GetField("id").i;
                    location.position = new Vector3(
                        location_json.GetField("position").GetField("x").f,
                        location_json.GetField("position").GetField("y").f,
                        location_json.GetField("position").GetField("z").f
                    );
                    LOCATIONS.Add(location);
                }
            }
        }

        public static void Session_ErrorReceived(Exception e, string message)
        {
            Debug.LogError(message);
            if (e != null) Debug.LogError(e.ToString());
            if (session != null)
            {
                session.Disconnect();
                session = null;
            }
        }

        public static void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            //ErrorMessage.AddError("Incoming Packet: " + packet.PacketType.ToString());
            Debug.Log("Incoming Packet: " + packet.PacketType.ToString());
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
                        if (state == State.Menu)
                        {
                            // Ignore, we will do a sync when the game starts
                            break;
                        }

                        var p = packet as ReceivedItemsPacket;
                        //ErrorMessage.AddError("next_item_index: " + next_item_index.ToString() + ", Index: " + p.Index.ToString() + ", items: " + p.Items.Count.ToString());
                        Debug.Log("next_item_index: " + next_item_index.ToString() + ", Index: " + p.Index.ToString() + ", items: " + p.Items.Count.ToString());

                        if (state == State.AwaitingSync)
                        {
                            if (next_item_index > 0)
                            {
                                // Make sure to not unlock stuff we've already unlocked
                                p.Items.RemoveRange(0, next_item_index);
                            }
                            state = State.InGame;
                        }
                        else if (next_item_index < p.Index)
                        {
                            ErrorMessage.AddError("Out of sync with server, resynchronizing");
                            // Resync, we're out of sync!
                            var sync_packet = new SyncPacket();
                            session.SendPacket(sync_packet);
                            state = State.AwaitingSync;
                            break;
                        }

                        foreach (var item in p.Items)
                        {
                            unlock_queue.Add(ITEM_CODE_TO_TECHTYPE[item.Item]);
                        }

                        next_item_index += p.Items.Count;

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

        public static bool checkLocation(Vector3 position)
        {
            int closest_id = -1;
            float closest_dist = 100000.0f;
            foreach (var location in LOCATIONS)
            {
                var dist = Vector3.Distance(location.position, position);
                if (dist < closest_dist && dist < 10.0f) // More than 10m, ignore. Can't have errors that big... can we?
                {
                    closest_dist = dist;
                    closest_id = location.id;
                }
            }

            if (closest_id != -1)
            {
                var location_packet = new LocationChecksPacket();
                location_packet.Locations = new List<int> { closest_id };
                APState.session.SendPacket(location_packet);
                return true;
            }

            return false;
        }

        public static void unlock(TechType techType)
        {
            if (PDAScanner.IsFragment(techType))
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

                PDAScanner.Entry entry;
                if (!PDAScanner.GetPartialEntryByKey(techType, out entry))
                {
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { techType, 0 });
                }

                if (entry != null)
                {
                    entry.unlocked++;

                    if (entry.unlocked >= entryData.totalFragments)
                    {
                        List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        partial.Remove(entry);
                        complete.Add(entry.techType);

                        MethodInfo methodNotifyRemove = typeof(PDAScanner).GetMethod("NotifyRemove", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.Entry) }, null);
                        methodNotifyRemove.Invoke(null, new object[] { entry });

                        MethodInfo methodUnlock = typeof(PDAScanner).GetMethod("Unlock", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.EntryData), typeof(bool), typeof(bool), typeof(bool) }, null);
                        methodUnlock.Invoke(null, new object[] { entryData, true, false, true });
                    }
                    else
                    {
                        int totalFragments = entryData.totalFragments;
                        if (totalFragments > 1)
                        {
                            float num2 = (float)entry.unlocked / (float)totalFragments;
                            float arg = (float)Mathf.RoundToInt(num2 * 100f);
                            ErrorMessage.AddError(Language.main.GetFormat<string, float, int, int>("ScannerInstanceScanned", Language.main.Get(entry.techType.AsString(false)), arg, entry.unlocked, totalFragments));
                        }

                        MethodInfo methodNotifyProgress = typeof(PDAScanner).GetMethod("NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.Entry) }, null);
                        methodNotifyProgress.Invoke(null, new object[] { entry });
                    }
                }
            }
            else
            {
                // Blueprint
                KnownTech.Add(techType, true);
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
            TechType.BaseObservatoryFragment,
            TechType.BaseWaterParkFragment,
            TechType.RadioFragment,
            TechType.BaseRoomFragment,
            TechType.BaseBulkheadFragment,
            TechType.BatteryChargerFragment,
            TechType.PowerCellChargerFragment,
            TechType.ScannerRoomFragment,
            TechType.SpecimenAnalyzerFragment,
            TechType.FarmingTrayFragment,
            TechType.SignFragment,
            TechType.PictureFrameFragment,
            TechType.BenchFragment,
            TechType.PlanterPotFragment,
            TechType.PlanterBoxFragment,
            TechType.PlanterShelfFragment,
            TechType.AquariumFragment,
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

    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("UpdateTarget")]
    internal class PDAScanner_UpdateTarget_Patch
    {
        public static List<TechType> tech_fragments_to_ignore = new List<TechType>
        {
            TechType.BaseRoom,
            TechType.FarmingTray,
            TechType.BaseBulkhead,
            TechType.BasePlanter,
            TechType.Spotlight,
            TechType.BaseObservatory,
            TechType.PlanterBox,
            TechType.BaseWaterPark,
            TechType.StarshipDesk,
            TechType.StarshipChair,
            TechType.StarshipChair3,
            TechType.LabCounter,
            TechType.NarrowBed,
            TechType.Bed1,
            TechType.Bed2,
            TechType.CoffeeVendingMachine,
            TechType.Trashcans,
            TechType.Techlight,
            TechType.BarTable,
            TechType.VendingMachine,
            TechType.SingleWallShelf,
            TechType.WallShelves,
            TechType.Bench,
            TechType.PlanterPot,
            TechType.PlanterShelf,
            TechType.PlanterPot2,
            TechType.PlanterPot3,
            TechType.LabTrashcan,
            TechType.BasePlanter,
            TechType.ExosuitClawArmFragment
        };

        [HarmonyPostfix]
        public static void MakeUnscanable()
        {
            if (PDAScanner.scanTarget.gameObject)
            {
                var tech_tag = PDAScanner.scanTarget.gameObject.GetComponent<TechTag>();
                if (tech_tag != null)
                {
                    if (tech_fragments_to_ignore.Contains(tech_tag.type))
                    {
                        PDAScanner.scanTarget.Invalidate();
                    }
                }
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
                APState.checkLocation(__instance.gameObject.transform.position);
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
            if (APState.checkLocation(__instance.gameObject.transform.position))
            {
                var generic_console = __instance.gameObject.GetComponent<GenericConsole>();
                if (generic_console != null)
                {
                    // Change it's color
                    generic_console.gotUsed = true;

                    var UpdateState_method = typeof(GenericConsole).GetMethod("UpdateState", BindingFlags.NonPublic | BindingFlags.Instance);
                    UpdateState_method.Invoke(generic_console, new object[] { });

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
            if (APState.checkLocation(__instance.gameObject.transform.position))
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
            APState.state = APState.State.AwaitingSync;

            // Make sure we have everything that's been unlocked already
            SyncPacket sync_packet = new SyncPacket();
            APState.session.SendPacket(sync_packet);
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("SetCurrentSlot")]
    internal class SaveLoadManager_SetCurrentSlot_Patch
    {
        [HarmonyPostfix]
        public static void SetCurrentItemIndex(string _currentSlot)
        {
            if (APState.archipelago_indexes.ContainsKey(_currentSlot))
            {
                APState.next_item_index = APState.archipelago_indexes[_currentSlot];
            }
            else
            {
                APState.next_item_index = 0;
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

    [HarmonyPatch(typeof(SaveLoadManager.GameInfo))]
    [HarmonyPatch("SaveIntoCurrentSlot")]
    internal class GameInfo_SaveIntoCurrentSlot_Patch
    {
        [HarmonyPrefix]
        public static bool SaveIntoCurrentSlot(SaveLoadManager.GameInfo info)
        {
            var SaveFile_method = typeof(SaveLoadManager.GameInfo).GetMethod("SaveFile", BindingFlags.NonPublic | BindingFlags.Static);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                string value = JsonUtility.ToJson(info);

                // Hax0rz
                value = value.Insert(value.Length - 1, ",\"archipelago_item_index\":" + APState.next_item_index.ToString());

                using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(value);
                }
                SaveFile_method.Invoke(null, new object[] { "gameinfo.json", memoryStream.ToArray() });
            }
            using (MemoryStream memoryStream2 = new MemoryStream())
            {
                var screenshot_member = typeof(SaveLoadManager.GameInfo).GetField("screenshot", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.Instance);

                Texture2D texture2D = (Texture2D)screenshot_member.GetValue(info);
                if (texture2D != null)
                {
                    var thumbnail = MathExtensions.ScaleTexture(texture2D, 200, false, false);
                    screenshot_member.SetValue(info, thumbnail);
                    byte[] array = thumbnail.EncodeToJPG(75);
                    if (array.Length != 0)
                    {
                        memoryStream2.Write(array, 0, array.Length);
                        SaveFile_method.Invoke(null, new object[] { "screenshot.jpg", memoryStream2.ToArray() });
                    }
                    UnityEngine.Object.Destroy(texture2D);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("Update")]
    internal class MainGameController_Update_Patch
    {
        static private bool IsSafeToUnlock()
        {
            if (APState.state != APState.State.InGame)
            {
                return false;
            }

            if (IntroVignette.isIntroActive || LaunchRocket.isLaunching)
            {
                return false;
            }

            if (PlayerCinematicController.cinematicModeCount > 0 && Time.time - PlayerCinematicController.cinematicActivityStart <= 30f)
            {
                return false;
            }

            return !SaveLoadManager.main.isSaving;
        }

        [HarmonyPostfix]
        public static void DequeueUnlocks()
        {
            if (IsSafeToUnlock())
            {
                var to_process = new List<TechType>(APState.unlock_queue);
                APState.unlock_queue.Clear();
                foreach (var unlock in to_process)
                {
                    APState.unlock(unlock);
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
            var gui_gameobject = new GameObject();
            gui_gameobject.AddComponent<ArchipelagoUI>();
            GameObject.DontDestroyOnLoad(gui_gameobject);
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
            else if (PDAScanner.scanTarget.gameObject)
                ArchipelagoUI.mouse_target_desc = APState.InspectGameObject(PDAScanner.scanTarget.gameObject);
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
        public static void GameReady(EscapePod __instance)
        {
            DevConsole.SendConsoleCommand("explodeship");
        }
    }

    // Advance rocket stage, but don't add to known tech the next stage! We'll find them in the world
    [HarmonyPatch(typeof(Rocket))]
    [HarmonyPatch("AdvanceRocketStage")]
    internal class Rocket_AdvanceRocketStage_Patch
    {
        [HarmonyPrefix]
        static public bool AdvanceRocketStage(Rocket __instance)
        {
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

    [HarmonyPatch(typeof(RocketConstructor))]
    [HarmonyPatch("StartRocketConstruction")]
    internal class RocketConstructor_StartRocketConstruction_Patch
    {
        [HarmonyPrefix]
        static public bool StartRocketConstruction(RocketConstructor __instance)
        {
            TechType currentStageTech = __instance.rocket.GetCurrentStageTech();
            if (!KnownTech.Contains(currentStageTech))
            {
                return false;
            }

            return true;
        }
    }

    // Prevent aurora explosion story event to give a radiationsuit...
    [HarmonyPatch(typeof(Story.UnlockBlueprintData))]
    [HarmonyPatch("Trigger")]
    internal class UnlockBlueprintData_Trigger_Patch
    {
        [HarmonyPrefix]
        static public bool PreventRadiationSuitUnlock(Story.UnlockBlueprintData __instance)
        {
            if (__instance.techType == TechType.RadiationSuit)
            {
                return false;
            }
            return true;
        }
    }

    // When launching the rocket, send goal achieved to archipelago
    [HarmonyPatch(typeof(LaunchRocket))]
    [HarmonyPatch("SetLaunchStarted")]
    internal class LaunchRocket_SetLaunchStarted_Patch
    {
        [HarmonyPrefix]
        static public void SetLaunchStarted()
        {
            var status_update_packet = new StatusUpdatePacket();
            status_update_packet.Status = ArchipelagoClientState.ClientGoal;
            APState.session.SendPacket(status_update_packet);
        }
    }
}
