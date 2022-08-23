using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Archipelago
{
    public static class APState
    {
        public struct Location
        {
            public long ID;
            public Vector3 Position;
        }

        public enum State
        {
            Menu,
            InGame
        }

        public static Dictionary<string, string> GoalMapping = new Dictionary<string, string>()
        {
            { "free", "Goal_Disable_Gun" },
            { "drive", "AuroraRadiationFixed" },
            { "infected", "Infection_Progress4" },
        };

        public static HashSet<string> scannable = new HashSet<string>();
        public static int[] AP_VERSION = new int[] { 0, 3, 5 };
        public static APData ServerData = new APData();
        public static DeathLinkService DeathLinkService = null;
        public static Dictionary<long, TechType> ITEM_CODE_TO_TECHTYPE = new Dictionary<long, TechType>();
        public static Dictionary<long, Location> LOCATIONS = new Dictionary<long, Location>();
        public static bool DeathLinkKilling = false; // indicates player is currently getting DeathLinked
        public static Dictionary<string, int> archipelago_indexes = new Dictionary<string, int>();
        public static float unlock_dequeue_timeout = 0.0f;
        public static List<string> message_queue = new List<string>();
        public static float message_dequeue_timeout = 0.0f;
        public static State state = State.Menu;
        public static bool Authenticated;
        public static string Goal = "launch";
        public static string GoalEvent = "";
        public static bool Silent = false;
        public static Thread TrackerProcessing;
        public static long TrackedLocationsCount = 0;
        public static long TrackedFishCount = 0;
        public static string TrackedFish = "";
        public static long TrackedLocation = -1;
        public static string TrackedLocationName;
        public static float TrackedDistance;
        public static float TrackedAngle;

        public static ArchipelagoSession Session;
        public static ArchipelagoUI ArchipelagoUI = null;
        
        public static HashSet<TechType> tech_fragments = new HashSet<TechType>
        {
            // scannable
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
            // non-destructive scanning
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
            TechType.BaseFiltrationMachine
        };

        public static Dictionary<string, long> Encyclopdia;

        public static HashSet<TechType> TechFragmentsToDestroy = new HashSet<TechType>();

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

        public static void Init()
        {
            // Load items.json
            {
                var reader = File.OpenText("QMods/Archipelago/items.json");
                var content = reader.ReadToEnd();
                reader.Close();
                var data = JsonConvert.DeserializeObject<Dictionary<int, string>>(content);
                foreach (var itemJson in data)
                {
                    ITEM_CODE_TO_TECHTYPE[itemJson.Key] =
                        (TechType)Enum.Parse(typeof(TechType), itemJson.Value);
                }
            }
            // Load locations.json
            {
                var reader = File.OpenText("QMods/Archipelago/locations.json");
                var content = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(content);
                reader.Close();

                foreach (var locationJson in data)
                {
                    Location location = new Location();
                    location.ID = locationJson.Key;
                    var vec = locationJson.Value;
                    location.Position = new Vector3(
                        vec["x"],
                        vec["y"],
                        vec["z"]
                    );
                    LOCATIONS.Add(location.ID, location);
                }
            }
            // Load encyclopedia.json
            {
                var reader = File.OpenText("QMods/Archipelago/encyclopedia.json");
                var content = reader.ReadToEnd();
                Encyclopdia = JsonConvert.DeserializeObject<Dictionary<string, long>>(content);
                reader.Close();

            }
            // launch thread
            TrackerProcessing = new Thread(TrackerThread.DoWork);
            TrackerProcessing.IsBackground = true;
            TrackerProcessing.Start();
        }
        public static bool Connect()
        {
            if (Authenticated)
            {
                return true;
            }
            // Start the archipelago session.
            var url = ServerData.host_name;
            int port = 38281;
            if (url.Contains(":"))
            {
                var splits = url.Split(new char[] { ':' });
                url = splits[0];
                if (!int.TryParse(splits[1], out port)) port = 38281;
            }

            Session = ArchipelagoSessionFactory.CreateSession(url, port);
            Session.Socket.PacketReceived += Session_PacketReceived;
            Session.Socket.ErrorReceived += Session_ErrorReceived;
            Session.Socket.SocketClosed += Session_SocketClosed;

            HashSet<TechType> vanillaTech = new HashSet<TechType>();
            
            LoginResult loginResult = Session.TryConnectAndLogin(
                "Subnautica", 
                ServerData.slot_name,
                ItemsHandlingFlags.AllItems, 
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null, 
                "",
                ServerData.password == "" ? null : ServerData.password);

            if (loginResult is LoginSuccessful loginSuccess)
            {
                Authenticated = true;
                state = State.InGame;
                if (loginSuccess.SlotData.ContainsKey("goal"))
                {
                    Goal = (string)loginSuccess.SlotData["goal"];
                    GoalMapping.TryGetValue(Goal, out GoalEvent);
                    if (loginSuccess.SlotData["vanilla_tech"] is JArray temp)
                    {
                        foreach (var tech in temp)
                        {
                            vanillaTech.Add((TechType)Enum.Parse(typeof(TechType), tech.ToString()));
                        }
                    }
                    
                }

                if (loginSuccess.SlotData.TryGetValue("deathlink", out var deathlink))
                {
                    var bdeathlink = (int)deathlink;
                    ServerData.death_link = bdeathlink > 0;
                }
                set_deathlink();

            }
            else if (loginResult is LoginFailure loginFailure)
            {
                Authenticated = false;
                ErrorMessage.AddMessage("Connection Error: " + String.Join("\n", loginFailure.Errors));
                Debug.LogError(String.Join("\n", loginFailure.Errors));
                Session.Socket.Disconnect();
                Session = null;
            }
            // all fragments
            TechFragmentsToDestroy = new HashSet<TechType>(APState.tech_fragments);
            // remove vanilla so it's scannable
            TechFragmentsToDestroy.ExceptWith(vanillaTech);
            Debug.LogError("Preventing scanning of: " + string.Join(", ", TechFragmentsToDestroy));
            Debug.LogError("Allowing scanning of: " + string.Join(", ", vanillaTech));
            return loginResult.Successful;
        }
        
        public static void Session_SocketClosed(string reason)
        {
            message_queue.Add("Connection to Archipelago lost: " + reason);
            Debug.LogError("Connection to Archipelago lost: " + reason);
            Disconnect();
        }
        public static void Session_ErrorReceived(Exception e, string message)
        {
            Debug.LogError(message);
            if (e != null) Debug.LogError(e.ToString());
            Disconnect();
        }

        public static void Disconnect()
        {
            if (Session != null && Session.Socket != null)
            {
                Session.Socket.Disconnect();
            }
            Session = null;
            Authenticated = false;
            state = State.Menu;
        }
        public static void DeathLinkReceived(DeathLink deathLink)
        {
            if (!(bool) (UnityEngine.Object) Player.main.liveMixin)
                return;
            Debug.Log("Received DeathLink");
            DeathLinkKilling = true;
            Player.main.liveMixin.Kill();
            message_queue.Add(deathLink.Cause);
        }
        public static void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            Debug.Log("Incoming Packet: " + packet.PacketType.ToString());
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.Print:
                {
                    if (!Silent)
                    {
                        var p = packet as PrintPacket;
                        message_queue.Add(p.Text);
                    }
                    break;
                }

                case ArchipelagoPacketType.PrintJSON:
                {
                    if (!Silent)
                    {
                        var p = packet as PrintJsonPacket;
                        string text = "";
                        foreach (var messagePart in p.Data)
                        {
                            switch (messagePart.Type)
                            {
                                case "player_id":
                                    text += int.TryParse(messagePart.Text, out var playerSlot)
                                        ? Session.Players.GetPlayerAlias(playerSlot) ?? $"Slot: {playerSlot}"
                                        : messagePart.Text;
                                    break;
                                case "item_id":
                                    text += int.TryParse(messagePart.Text, out var itemId)
                                        ? Session.Items.GetItemName(itemId) ?? $"Item: {itemId}"
                                        : messagePart.Text;
                                    break;
                                case "location_id":
                                    text += int.TryParse(messagePart.Text, out var locationId)
                                        ? Session.Locations.GetLocationNameFromId(locationId) ?? $"Location: {locationId}"
                                        : messagePart.Text;
                                    break;
                                default:
                                    text += messagePart.Text;
                                    break;
                            }
                        }
                        message_queue.Add(text);
                    }
                    break;
                }
            }
        }
        
        public static bool checkLocation(Vector3 position)
        {
            long closest_id = -1;
            float closestDist = 100000.0f;
            foreach (var location in LOCATIONS)
            {
                var dist = Vector3.Distance(location.Value.Position, position);
                if (dist < closestDist && dist < 1.0f)
                {
                    closestDist = dist;
                    closest_id = location.Key;
                }
            }

            if (closest_id != -1)
            {
                ServerData.@checked.Add(closest_id);
                Session.Locations.CompleteLocationChecks(closest_id);
                return true;
            }
#if DEBUG
            ErrorMessage.AddError("Tried to check unregistered Location at: " + position);
            Debug.LogError("Tried to check unregistered Location at: " + position);
            foreach (var location in LOCATIONS)
            {
                var dist = Vector3.Distance(location.Value.Position, position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest_id = location.Key;
                }
            }
            ErrorMessage.AddError("Could it be Location ID " + closest_id + " with a distance of "+closestDist + "?");
            Debug.LogError("Could it be Location ID " + closest_id + " with a distance of "+closestDist + "?");
#endif
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

        public static void set_deathlink()
        {
            if (DeathLinkService == null)
            {
                DeathLinkService = Session.CreateDeathLinkService();
                DeathLinkService.OnDeathLinkReceived += DeathLinkReceived;
            }
            
            if (ServerData.death_link)
            {
                DeathLinkService.EnableDeathLink();
            }
            else
            {
                DeathLinkService.DisableDeathLink();
            }
        }
        
        public static void send_completion()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            Session.Socket.SendPacket(statusUpdatePacket);
        }
    }
}