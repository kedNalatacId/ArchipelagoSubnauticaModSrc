using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago
{
    // Comment this out for real game! Cheats for testing out stuff
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

    public class ArchipelagoUI : MonoBehaviour
    {
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

        void OnGUI()
        {
            if (APState.session != null && APState.session.Connected)
            {
                GUI.Label(new Rect(16, 16, 300, 25), "Archipelago Status: Connected");
            }
            else
            {
                GUI.Label(new Rect(16, 16, 300, 25), "Archipelago Status: Not Connected");
            }

            if (APState.is_in_game)
            {
                if (GUI.Button(new Rect(16, 50, 150, 25), "Activate Cheats"))
                {
                    DevConsole.SendConsoleCommand("nodamage");
                    DevConsole.SendConsoleCommand("oxygen");
                    DevConsole.SendConsoleCommand("item seaglide");
                    DevConsole.SendConsoleCommand("item battery 10");
                    DevConsole.SendConsoleCommand("speed 3");
                }

                int i = 0;
                int j = 100;
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
                        j = 100;
                        i += 200 + 16;
                    }
                }

                //int i = 0;
                //int j = 100;
                //foreach (var kv in APState.ITEM_CODE_TO_TECHTYPE)
                //{
                //    if (GUI.Button(new Rect(16 + i, j, 200, 25), kv.Value.ToString()))
                //    {
                //        APState.unlock(kv.Value);
                //    }
                //    j += 30;
                //    if (j + 30 >= Screen.height)
                //    {
                //        j = 100;
                //        i += 200 + 16;
                //    }
                //}
            }
        }
    }

    public static class APState
    {
        public static string host;
        public static string player_name;
        public static string password;

        public static Dictionary<int, TechType> ITEM_CODE_TO_TECHTYPE = new Dictionary<int, TechType>
        {
            {35000, TechType.Compass },
            {35001, TechType.PlasteelTank },
            {35002, TechType.BaseUpgradeConsole },
            {35003, TechType.UltraGlideFins },
            {35004, TechType.CyclopsSonarModule },
            {35005, TechType.ReinforcedDiveSuit },
            {35006, TechType.CyclopsThermalReactorModule },
            {35007, TechType.Stillsuit },
            {35008, TechType.BaseWaterPark },
            {35009, TechType.CyclopsDecoy },
            {35010, TechType.CyclopsFireSuppressionModule },
            {35011, TechType.SwimChargeFins },
            {35012, TechType.RepulsionCannon },
            {35013, TechType.CyclopsDecoyModule },
            {35014, TechType.CyclopsShieldModule },
            {35015, TechType.CyclopsHullModule1 },
            {35016, TechType.CyclopsSeamothRepairModule },
            {35017, TechType.BatteryCharger },
            {35018, TechType.Beacon },
            {35019, TechType.BaseBioReactor },
            {35020, TechType.CyclopsBridgeBlueprint },
            {35021, TechType.CyclopsEngineBlueprint },
            {35022, TechType.CyclopsHullBlueprint },
            {35023, TechType.GravSphereFragment },
            {35024, TechType.GravSphereFragment },
            {35025, TechType.LaserCutter },
            {35026, TechType.TechlightFragment },
            {35027, TechType.ConstructorFragment },
            {35028, TechType.ConstructorFragment },
            {35029, TechType.ConstructorFragment },
            {35030, TechType.WorkbenchFragment },
            {35031, TechType.WorkbenchFragment },
            {35032, TechType.WorkbenchFragment },
            {35033, TechType.MoonpoolFragment },
            {35034, TechType.MoonpoolFragment },
            {35035, TechType.BaseNuclearReactor },
            {35036, TechType.PowerCellCharger },
            {35037, TechType.PowerTransmitterFragment },
            {35038, TechType.Exosuit },
            {35039, TechType.ExosuitClawArmModule }, // This is by default, replace!
            {35040, TechType.ExosuitDrillArmModule },
            {35041, TechType.ExosuitGrapplingArmModule },
            {35042, TechType.ExosuitPropulsionArmModule },
            {35043, TechType.ExosuitTorpedoArmModule },
            {35044, TechType.ScannerRoomBlueprint }, //
            {35045, TechType.SeamothFragment },
            {35046, TechType.SeamothFragment },
            {35047, TechType.SeamothFragment },
            {35048, TechType.StasisRifleFragment },
            {35049, TechType.StasisRifleFragment },
            {35050, TechType.ThermalPlantFragment },
            {35051, TechType.ThermalPlantFragment }
        };

        public static Dictionary<int, string> LOCATION_ADDRESS_TO_CHECK_ID = new Dictionary<int, string>
        {
            { 33000, "(-1234.3, -349.7, -396.0)" },
            { 33001, "(-1208.0, -349.6, -383.0)" },
            { 33002, "(1327.1, -234.9, 575.8)" },
            { 33003, "(910.9, -201.8, 623.5)" },
            { 33004, "(903.8, -220.3, 590.9)" },
            { 33005, "(914.9, -202.1, 611.8)" },
            { 33006, "(-635.1, -502.7, -951.4)" },
            { 33007, "(-643.8, -509.9, -941.9)" },
            { 33008, "(-765.7, 17.6, -1116.4)" },
            { 33009, "(110.6, -264.9, -369.0)" },
            { 33010, "(-1393.9, -329.7, 733.5)" },
            { 33011, "(-1407.7, -344.2, 721.5)" },
            { 33012, "(-1196.3, -223.0, 12.5)" },
            { 33013, "(-1206.4, -225.6, 4.0)" },
            { 33014, "(-1626.2, -357.5, 99.5)" },
            { 33015, "(-850.9, -473.2, -1414.6)" },
            { 33016, "(-1407.7, -344.2, 721.5)" },
            { 33017, "(-862.4, -437.5, -1444.1)" },
            { 33018, "(-269.7, -262.8, -764.3)" },
            { 33019, "(-285.2, -262.4, -788.4)" },
            { 33020, "(-285.8, -240.2, -786.5)" },
            { 33021, "(-889.4, -433.8, -1424.8)" },
            { 33022, "(-23.3, -105.8, -604.2)" },
            { 33023, "(319.4, -104.3, 441.5)" },
            { 33024, "(313.9, -91.8, 432.6)" },
            { 33025, "(-421.4, -107.8, -266.5)" },
            { 33026, "(-317.6, -78.8, 247.4)" },
            { 33027, "(-483.6, -504.7, 1326.6)" },
            { 33028, "(-34.2, -22.4, 410.5)" },
            { 33029, "(712.4, -3.4, 160.8)" },
            { 33030, "(358.7, -117.1, 306.8)" },
            { 33031, "(1119.5, -271.7, 561.7)" },
            { 33032, "(-926.4, -185.2, 501.8)" },
            { 33033, "(-809.8, -302.2, -876.9)" },
            { 33034, "(1068.5, -283.4, 1345.3)" },
            { 33035, "(1075.7, -288.9, 1321.8)" },
            { 33036, "(676.3, -343.6, 1204.6)" },
            { 33037, "(740.3, -389.2, 1179.8)" },
            { 33038, "(698.2, -350.8, 1186.9)" },
            { 33039, "(-655.1, -109.6, 791.0)" },
            { 33040, "(-663.4, -111.9, 777.9)" },
            { 33041, "(-1115.9, -175.3, -724.5)" },
            { 33042, "(-1161.1, -191.7, -758.3)" },
            { 33043, "(-1129.5, -155.2, -729.3)" },
            { 33044, "(-795.5, -204.1, -774.7)" },
            { 33045, "(-810.7, -209.3, -685.5)" },
            { 33046, "(-789.8, -216.1, -711.0)" },
            { 33047, "(-126.8, -201.1, 852.1)" },
            { 33048, "(-138.4, -193.6, 888.7)" },
            { 33049, "(-137.8, -193.4, 879.4)" },
            { 33050, "(-124.4, -200.7, 853.0)" },
            { 33051, "(-170.8, -187.6, 880.7)" }
        };
        public static Dictionary<string, int> LOCATION_CHECK_ID_TO_ADDRESS = new Dictionary<string, int>();

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

        public static void InspectGameObject(GameObject gameObject)
        {
            string msg = gameObject.transform.position.ToString().Trim() + ": ";

            Component[] components = gameObject.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
            {
                msg += components[i].ToString() + ",";
            }

            ErrorMessage.AddMessage("GameObject:" + msg);
            Debug.Log("INSPECT GAME OBJECT: " + msg);
        }

        static public void Init()
        {
            foreach (var kv in LOCATION_ADDRESS_TO_CHECK_ID)
            {
                LOCATION_CHECK_ID_TO_ADDRESS[kv.Value] = kv.Key;
            }

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

        public static void unlock(TechType techType)
        {
            //Debug.Log("EntryData: " +
            //                            entryData.key.ToString() + ", " +
            //                            entryData.locked.ToString() + ", " +
            //                            entryData.totalFragments.ToString() + ", " +
            //                            entryData.destroyAfterScan.ToString() + ", " +
            //                            entryData.encyclopedia.ToString() + ", " +
            //                            entryData.blueprint.ToString() + ", " +
            //                            entryData.scanTime.ToString() + ", " +
            //                            entryData.isFragment.ToString());

            //EntryData: ConstructorFragment, True, 3, True, MVB, Constructor, 2, True
            //EntryData: MoonpoolFragment, False, 2, True, Moonpool, BaseMoonpool, 3, True
            //EntryData: PowerTransmitterFragment, False, 1, True, , PowerTransmitter, 2, True
            //EntryData: SeamothFragment, False, 3, True, Seamoth, Seamoth, 4, True
            //EntryData: StasisRifleFragment, False, 2, True, StasisRifle, StasisRifle, 4, True
            //EntryData: TechlightFragment, False, 1, False, Techlight, Techlight, 3, True
            //EntryData: ThermalPlantFragment, False, 2, True, ThermalPlant, ThermalPlant, 4, True
            //EntryData: WorkbenchFragment, False, 3, True, ModificationStation, Workbench, 3, True
            //EntryData: LaserCutter, False, 1, False, LaserCutter, None, 4, False
            //EntryData: CyclopsShieldModule, False, 1, False, , CyclopsShieldModule, 1, False
            //EntryData: CyclopsSonarModule, False, 1, False, , CyclopsSonarModule, 1, False
            //EntryData: CyclopsHullModule1, False, 1, False, , CyclopsHullModule1, 1, False
            //EntryData: CyclopsDecoyModule, False, 1, False, , CyclopsDecoyModule, 1, False
            //EntryData: CyclopsFireSuppressionModule, False, 1, False, , CyclopsFireSuppressionModule, 1, False
            //EntryData: GravSphereFragment, False, 2, True, , Gravsphere, 3, True
            //EntryData: BaseWaterPark, False, 1, False, , BaseWaterPark, 1, False


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
            //TechType.SeaglideFragment, //TODO Add later
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
            if ((TechType)techTypeMember.GetValue(__instance) == TechType.Fragment)
            {
                var techTag = __instance.GetComponent<TechTag>();
                if (techTag != null && tech_fragments_to_destroy.Contains(techTag.type))
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
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
        public static bool OpenDatabox(BlueprintHandTarget __instance)
        {
            if (!__instance.used)
            {
                var databox_id = __instance.gameObject.transform.position.ToString().Trim();
                var location_id = APState.LOCATION_CHECK_ID_TO_ADDRESS[databox_id];

                var location_packet = new LocationChecksPacket();
                location_packet.Locations = new List<int> { location_id };
                APState.session.SendPacket(location_packet);
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

    [HarmonyPatch(typeof(GenericHandTarget))]
    [HarmonyPatch("OnHandClick")]
    internal class GenericHandTarget_OnHandHover_Patch
    {
        [HarmonyPrefix]
        public static void OnHandClick(GenericHandTarget __instance)
        {
            APState.InspectGameObject(__instance.gameObject);
        }
    }
}
