using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Archipelago
{

    public static class APState
    {
        public struct Location
        {
            public long ID;
            public Vector3 Position;
            public float distance_to_origin;
            public float distance_from_radiation;
        }

        public enum State
        {
            Menu,
            InGame,
            Cancelled
        }

        public enum Inclusion
        {
            Included,
            NotInLogic,
            Excluded
        }

        public enum SlipType
        {
            None,
            LaserCutter,
            PropulsionCannon,
            Both
        }

        public enum ReaperAggroRange
        {
            Normal,
            Double,
            Half,
            Quarter,
            None
        }

        public static Dictionary<string, string> GoalMapping = new Dictionary<string, string>()
        {
            { "free", "Goal_Disable_Gun" },
            { "drive", "AuroraRadiationFixed" },
            { "infected", "Infection_Progress4" },
        };

        public static int[] AP_VERSION = new int[] { 0, 6, 2 };
        public static APConnectInfo ServerConnectInfo = new APConnectInfo();
        public static DeathLinkService DeathLinkService = null;
        public static bool DeathLinkKilling = false; // indicates player is currently getting DeathLinked
        public static Dictionary<string, int> archipelago_indexes = new ();
        public static float unlock_dequeue_timeout = 0.0f;
        public static List<string> message_queue = new ();
        public static float message_dequeue_timeout = 0.0f;
        public static State state = State.Menu;
        public static bool Authenticated;
        public static string Goal = "launch";
        public static string GoalEvent = "";
        public static int SwimRule = 601;
        public static bool EmptyTanks = false;
        public static bool ConsiderItems = false;
        public static int SeaglideDepth = 200;
        public static int SeaglideDistance = 800;
        public static Inclusion SeamothState = Inclusion.Included;
        public static Inclusion PrawnState = Inclusion.Included;
        public static Inclusion CyclopsState = Inclusion.Included;
        public static bool IgnoreRadiation = false;
        public static SlipType CanSlipThrough = SlipType.None;
        public static bool FreeSamples = false;
        public static bool ReduceResourceClutter = false;
        public static ReaperAggroRange ReaperAggroDistance = ReaperAggroRange.Normal;
        public static bool WarperSpawn = true;
        public static bool Silent = false;
        public static Thread TrackerProcessing;
        public static long TrackedLocationsCount = 0;
        public static long TrackedFishCount = 0;
        public static string TrackedFish = "";
        public static int TrackedPlantCount = 0;
        public static string TrackedPlants = "";
        public static long TrackedLocation = -1;
        public static string TrackedLocationName;
        public static float TrackedDistance;
        public static float TrackedAngle;
        public static int TrackedDepth;

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
            TechType.StarshipChair2,
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
            TechType.PictureFrame,
            TechType.PlanterPot,
            TechType.PlanterShelf,
            TechType.PlanterPot2,
            TechType.PlanterPot3,
            TechType.LabTrashcan,
            TechType.BaseFiltrationMachine
        };

        public static TrackerMode TrackedMode = TrackerMode.Logical;
        public static HashSet<TechType> TechFragmentsToDestroy = new HashSet<TechType>();

        public static bool Connect()
        {
            if (Authenticated)
            {
                return true;
            }

            if (ServerConnectInfo.host_name is null || ServerConnectInfo.host_name.Length == 0)
            {
                return false;
            }

            // Start the archipelago session.
            Session = ArchipelagoSessionFactory.CreateSession(ServerConnectInfo.host_name);
            Session.MessageLog.OnMessageReceived += Session_MessageReceived;
            Session.Socket.ErrorReceived += Session_ErrorReceived;
            Session.Socket.SocketClosed += Session_SocketClosed;

            HashSet<TechType> vanillaTech = new HashSet<TechType>();
            LoginResult loginResult = Session.TryConnectAndLogin(
                "Subnautica", 
                ServerConnectInfo.slot_name,
                ItemsHandlingFlags.AllItems, 
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null, 
                "",
                ServerConnectInfo.password);

            if (loginResult is LoginFailure loginFailure)
            {
                Authenticated = false;
                Debug.LogError(String.Join("\n", loginFailure.Errors));
                Session = null;
                ErrorMessage.AddMessage("Connection Error: " + String.Join("\n", loginFailure.Errors));
                return false;
            }

            LoginSuccessful loginSuccess;
            if (loginResult is LoginSuccessful tmp)
            {
                loginSuccess = tmp;
            }
            else
            {
                Authenticated = false;
                Session = null;
                return false;
            }

            var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            var rawPath = storage?.GetType().GetField("savePath",
                    BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(storage);
            if (rawPath != null)
            {
                ServerConnectInfo.GetAsLastConnect().WriteToFile(rawPath + "/archipelago_last_connection.json");
            }
            else
            {
                Debug.LogError("Could not write most recent connect info to file.");
            }

            Authenticated = true;
            state = State.InGame;
            if (loginSuccess.SlotData.TryGetValue("swim_rule", out var swim_rule))
            {
                // Allow old-style SwimRule to be passed; parse it here, not in Tracker later.
                string DepthString = Convert.ToString(swim_rule);

                // if it's not all digits, assume it's an old style string
                // we'll take default here as an acceptable worst-case
                if (!DepthString.All(c => Char.IsDigit(c)))
                {
                    var nameParts = DepthString.Split('_');
                    ConsiderItems = nameParts.Length > 1;

                    switch (nameParts.GetLast())
                    {
                        case "easy":
                            SwimRule = 200;
                            break;
                        case "normal":
                            SwimRule = 400;
                            break;
                        default:
                            SwimRule = 600;
                            break;
                    }
                }
                else
                {
                    SwimRule = Convert.ToInt32(swim_rule);
                }

                if (SwimRule < 100)
                {
                    SwimRule = 100;
                }
                if (SwimRule > 600)
                {
                    SwimRule = 600;
                }
            }
            Debug.Log("Swim Rule: " + SwimRule);

            if (loginSuccess.SlotData.TryGetValue("consider_items", out var consider_items))
            {
                ConsiderItems = Convert.ToInt32(consider_items) > 0;
            }
            Debug.Log("Consider Items: " + ConsiderItems);

            if (loginSuccess.SlotData.TryGetValue("pre_seaglide_distance", out var seaglide_distance))
            {
                SeaglideDistance = Convert.ToInt32(seaglide_distance);
                if (SeaglideDistance < 600)
                {
                    SeaglideDistance = 600;
                }
                if (SeaglideDistance > 2500)
                {
                    SeaglideDistance = 2500;
                }
            }
            Debug.Log("PreSeaglideDistance: " + SeaglideDistance);

            if (loginSuccess.SlotData.TryGetValue("seaglide_depth", out var seaglide_depth))
            {
                SeaglideDepth = Convert.ToInt32(seaglide_depth);
                if (SeaglideDepth < 100)
                {
                    SeaglideDepth = 100;
                }
                if (SeaglideDepth > 400)
                {
                    SeaglideDepth = 400;
                }
            }
            Debug.Log("Seaglide Depth: " + SeaglideDepth);

            if (loginSuccess.SlotData.TryGetValue("include_seamoth", out var include_seamoth))
            {
                // This convoluted mess... works. It only has to run on load, so... good enough for now.
                if (Enum.TryParse(include_seamoth.ToString(), true, out Inclusion really_include_seamoth))
                {
                    SeamothState = really_include_seamoth;
                }
            }
            Debug.Log("Seamoth State: " + SeamothState);

            if (loginSuccess.SlotData.TryGetValue("include_prawn", out var include_prawn))
            {
                if (Enum.TryParse(include_prawn.ToString(), true, out Inclusion really_include_prawn))
                {
                    PrawnState = really_include_prawn;
                }
            }
            Debug.Log("Prawn State: " + PrawnState);

            if (loginSuccess.SlotData.TryGetValue("include_cyclops", out var include_cyclops))
            {
                if (Enum.TryParse(include_cyclops.ToString(), true, out Inclusion really_include_cyclops))
                {
                    CyclopsState = really_include_cyclops;
                }
            }
            Debug.Log("Cyclops State: " + CyclopsState);

            if (loginSuccess.SlotData.TryGetValue("free_samples", out var free_samples))
            {
                FreeSamples = Convert.ToInt32(free_samples) > 0;
            }
            Debug.Log("Free Samples: " + FreeSamples);

            if (loginSuccess.SlotData.TryGetValue("reduce_resource_clutter", out var reduce_resource_clutter))
            {
                ReduceResourceClutter = Convert.ToInt32(reduce_resource_clutter) > 0;
            }
            Debug.Log("Reduce Resource Clutter: " + ReduceResourceClutter);

            if (loginSuccess.SlotData.TryGetValue("reaper_aggro_distance", out var reaper_aggro_distance))
            {
                if (Enum.TryParse(reaper_aggro_distance.ToString(), true, out ReaperAggroRange real_reaper_aggro))
                {
                    ReaperAggroDistance = real_reaper_aggro;
                }
            }
            Debug.Log("Reaper Aggro Range: " + ReaperAggroDistance);

            if (loginSuccess.SlotData.TryGetValue("warper_spawn", out var warper_spawn))
            {
                WarperSpawn = Convert.ToInt32(warper_spawn) > 0;
            }
            Debug.Log("Warper Spawn: " + WarperSpawn);

            if (loginSuccess.SlotData.TryGetValue("empty_tanks", out var empty_tanks))
            {
                EmptyTanks = Convert.ToInt32(empty_tanks) > 0;
            }
            Debug.Log("Empty Tanks: " + EmptyTanks);

            if (loginSuccess.SlotData.TryGetValue("can_slip_through", out var can_slip_through))
            {
                if (Enum.TryParse(can_slip_through.ToString(), true, out SlipType really_slip))
                CanSlipThrough = really_slip;
                Debug.Log("Can Slip Through: " + CanSlipThrough);
            }
            if (loginSuccess.SlotData.TryGetValue("ignore_radiation", out var ignore_radiation))
            {
                IgnoreRadiation = Convert.ToInt32(ignore_radiation) > 0;
                Debug.Log("Ignore Radiation: " + IgnoreRadiation);
            }
            Goal = (string)loginSuccess.SlotData["goal"];
            GoalMapping.TryGetValue(Goal, out GoalEvent);
            if (loginSuccess.SlotData["vanilla_tech"] is JArray temp)
            {
                foreach (var tech in temp)
                {
                    vanillaTech.Add((TechType)Enum.Parse(typeof(TechType), tech.ToString()));
                }
            }

            Debug.Log("SlotData: " + JsonConvert.SerializeObject(loginSuccess.SlotData));
            ServerConnectInfo.death_link = Convert.ToInt32(loginSuccess.SlotData["death_link"]) > 0;
            set_deathlink();

            // all fragments
            TechFragmentsToDestroy = new HashSet<TechType>(APState.tech_fragments);

            // remove vanilla so it's scannable
            TechFragmentsToDestroy.ExceptWith(vanillaTech);
            Debug.Log("Preventing scanning of: " + string.Join(", ", TechFragmentsToDestroy));
            Debug.Log("Allowing scanning of: " + string.Join(", ", vanillaTech));
            return loginResult.Successful;
        }

        static void Session_SocketClosed(string reason)
        {
            message_queue.Add("Connection to Archipelago lost: " + reason);
            Debug.LogError("Connection to Archipelago lost: " + reason);
            Disconnect();
        }
        static void Session_MessageReceived(LogMessage message)
        {
            if (!Silent)
            {
                message_queue.Add(message.ToString());
            }
        }
        static void Session_ErrorReceived(Exception e, string message)
        {
            Debug.LogError(message);
            if (e != null) Debug.LogError(e.ToString());
            Disconnect();
        }

        public static void Disconnect()
        {
            Authenticated = false;
            state = State.Menu;
            if (Session != null && Session.Socket != null && Session.Socket.Connected)
            {
                Task.Run(() => {Session.Socket.DisconnectAsync(); }).Wait();
            }

            Session = null;
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

        public static bool CheckLocation(Vector3 position)
        {
            long closest_id = -1;
            float closestDist = 100000.0f;
            foreach (var location in ArchipelagoData.Locations)
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
                SendLocID(closest_id);
                return true;
            }
            return false;
        }

        public static void SendLocID(long id)
        {
            if (ServerConnectInfo.@checked.Add(id))
            {
                Task.Run(() => {Session.Locations.CompleteLocationChecksAsync(
                    ServerConnectInfo.@checked.Except(Session.Locations.AllLocationsChecked).ToArray()); }).ConfigureAwait(false);
            }
        }

        public static void Resync()
        {
            Debug.Log("Running Item resync with " + Session.Items.AllItemsReceived.Count + " items.");
            var done = new HashSet<long>();
            for (int i = 0; i < Session.Items.AllItemsReceived.Count; i++)
            {
                var itemID = Session.Items.AllItemsReceived[i].ItemId;
                if (ArchipelagoData.ItemCodeToItemType[itemID] == ArchipelagoItemType.Resource || !done.Contains(itemID))
                {
                    Unlock(itemID, i);
                    done.Add(itemID);
                }
            }
        }

        public static void Unlock(long apItemID, long index)
        {
            // WIP -- debug why interior growbed doesn't show up in group
            if (ArchipelagoData.GroupItems.TryGetValue(apItemID, out var groupUnlock))
            {
                foreach (var subUnlock in groupUnlock)
                {
                    Unlock(subUnlock, index);
                }
                return;
            }

            ArchipelagoData.ItemCodeToTechType.TryGetValue(apItemID, out var techType);
            if (ArchipelagoData.ItemCodeToItemType[apItemID] == ArchipelagoItemType.Resource)
            {
                HashSet<long> set;
                if (!ServerConnectInfo.resources_granted.TryGetValue(apItemID, out set))
                {
                    set = new();
                    ServerConnectInfo.resources_granted[apItemID] = set;
                }
                if (set.Contains(index))
                {
                    // already given resources
                    return;
                }

                set.Add(index);

                // If reducing resource clutter, only ever receive one (not N where N is the number receiverd thus far)
                if (ReduceResourceClutter)
                {
                    Inventory.main.StartCoroutine(PickUp(techType));
                }
                else
                {
                    for (int i = 0; i < set.Count; i++)
                    {
                        Inventory.main.StartCoroutine(PickUp(techType));
                    }
                }

                return;
            }

            if (techType == TechType.None || KnownTech.Contains(techType))
            {
                // Unknown item ID or already known technology.
                return;
            }

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
                    int newCount = Session.Items.AllItemsReceived.Count(networkItem => networkItem.ItemId == apItemID);
                    if (newCount == entry.unlocked)
                    {
                        return;
                    }
                    entry.unlocked = newCount;

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
                        if (FreeSamples)
                        {
                            GiveItem(entryData.blueprint);
                        }
                    }
                    else
                    {
                        int totalFragments = entryData.totalFragments;
                        if (totalFragments > 1)
                        {
                            float num2 = entry.unlocked / (float)totalFragments;
                            float arg = Mathf.RoundToInt(num2 * 100f);
                            ErrorMessage.AddError(Language.main.GetFormat(
                                "ScannerInstanceScanned", Language.main.Get(entry.techType.AsString()), 
                                arg, entry.unlocked, totalFragments));
                        }

                        MethodInfo methodNotifyProgress = typeof(PDAScanner).GetMethod(
                            "NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static, null,
                            new Type[] { typeof(PDAScanner.Entry) }, null);
                        methodNotifyProgress.Invoke(null, new object[] { entry });
                    }
                }
            }
            else
            {
                // Blueprint
                if (ArchipelagoPlugin.Zero)
                {
                    typeof(KnownTech).GetMethod("Add", BindingFlags.Public | BindingFlags.Static).Invoke(
                        null,
                        new object[] {techType, true, true});
                }
                else
                {
                    typeof(KnownTech).GetMethod("Add", BindingFlags.Public | BindingFlags.Static).Invoke(
                        null,
                        new object[] {techType, true});
                }

                if (FreeSamples)
                {
                    GiveItem(techType);
                }
            }
        }

        private static IEnumerator PickUp(TechType techType)
        {
            TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, prefabResult, false);

            GameObject gameObject = prefabResult.Get();
            if (gameObject == null)
            {
                Debug.LogError("Object " + techType + " failed to be created.");
                yield break;
            }
            // in case it can't be picked up, dump it right in front of player for obvious problem.
            gameObject.transform.position = MainCamera.camera.transform.position + 
                                            MainCamera.camera.transform.forward * 3f;
            // This seems to fill batteries to default values when crafted.
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();
            if (pickupable == null)
            {
                Debug.LogError("Object " + techType + " was not pickupable.");
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                Inventory.main.ForcePickup(pickupable);
            }
        }

        private static IEnumerator GiveItemAsync(TechType techType, bool giveLinked = false, bool filterCategory = true)
        {
            if (CraftData.GetBuilderIndex(techType, out TechGroup group, out TechCategory category, out int index))
            {
                if (filterCategory && (
                        category == TechCategory.Cyclops ||
                        category == TechCategory.BaseRoom ||
                        category == TechCategory.BasePiece ||
                        category == TechCategory.Misc ||
                        category == TechCategory.Constructor ||
                        category == TechCategory.BaseWall ||
                        category == TechCategory.ExteriorLight ||
                        category == TechCategory.ExteriorModule ||
                        category == TechCategory.ExteriorOther ||
                        category == TechCategory.InteriorModule ||
                        category == TechCategory.InteriorPiece ||
                        category == TechCategory.InteriorRoom
                    ))
                {
                    yield break;
                }

                yield return PickUp(techType);

                if (!giveLinked)
                {
                    yield break;
                }

                var techtypes = TechData.GetLinkedItems(techType);
                if (techtypes != null)
                {
                    foreach (var linkedItem in techtypes)
                    {
                        yield return PickUp(linkedItem);
                    }
                }
            }
        }

        public static void GiveItem(TechType techType)
        {
            Inventory.main.StartCoroutine(GiveItemAsync(techType, giveLinked: true));
        }

        public static void set_deathlink()
        {
            if (DeathLinkService == null)
            {
                DeathLinkService = Session.CreateDeathLinkService();
                DeathLinkService.OnDeathLinkReceived += DeathLinkReceived;
            }

            if (ServerConnectInfo.death_link)
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
