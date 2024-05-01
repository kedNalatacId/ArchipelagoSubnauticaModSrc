using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Archipelago
{
    public enum TrackerMode
    {
        Disabled,
        Closest,
        Logical,
    }

    public class TrackerThread
    {
        public static bool ItemsRelevant      = false;
        public static int BaseDepth           = 601;
        public static int SeaglideDepth       = 200;
        public static int nonSeaglideDistance = 800;
        public static int LogicSwimDepth      = BaseDepth;
        public static int LogicItemDepth      = 0;
        public static int LogicVehicleDepth   = 0;
        public static bool HasSeaglide        = false;
        public static bool HasRadiationSuit   = false;
        public static bool IgnoreRadiation    = false;
        public static bool CanSlipThrough     = false;
        public static bool IncludeSeamoth     = true;
        public static bool IncludePrawn       = true;
        public static bool IncludeCyclops     = true;
        public static string LogicVehicle     = "Vehicle";
        public static long creatureScanCutOff = 33999;
        public static long plantScanCutOff    = 34099;

        public static bool InLogic(long locID)
        {
            // special case; if we can slip through, then either laser cutter or propulsion; otherwise just propulsion
            // Allows these 2x locations to be available via laser cutter (if no radiation) where they otherwise wouldn't.
            if (locID == 33107 || locID == 33108)
            {
                if (CanSlipThrough)
                {
                    if (!KnownTech.Contains(TechType.LaserCutter) && !KnownTech.Contains(TechType.PropulsionCannon))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!KnownTech.Contains(TechType.PropulsionCannon))
                    {
                        return false;
                    }
                }
            }

            // Gating items; now with less gating
            foreach (var logic in ArchipelagoData.LogicDict)
            {
                // Don't check these here (they're checked above)
                if (locID == 33107 || locID == 33108)
                {
                    continue;
                }

                // Propulsion Cannon
                if (logic.Key == (TechType)757)
                {
                    // The only 2x hard locked propulsion cannon locations (all others "can slip")
                    // This is a little "magic number"y... but it works.
                    if (locID == 33053 || locID == 33054)
                    {
                        if (!KnownTech.Contains(logic.Key))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!CanSlipThrough && !KnownTech.Contains(logic.Key))
                        {
                            return false;
                        }
                    }
                }
                // Laser Cutter -- hard stop
                else if (logic.Key == (TechType)761)
                {
                    if (!KnownTech.Contains(logic.Key))
                    {
                        return false;
                    }
                }
            }

            // Is the area irradiated?
            if (! IgnoreRadiation)
            {
                if (!HasRadiationSuit && ArchipelagoData.Locations[locID].distance_from_radiation < 950)
                {
                    return false;
                }
            }

            // Distance -- if we don't have the seaglide (or appropriate vehicle), too far away isn't in logic
            if (ArchipelagoData.Locations[locID].distance_to_origin > nonSeaglideDistance && !HasSeaglide && LogicVehicle != "Seamoth" && LogicVehicle != "Cyclops")
            {
                return false;
            }

            // Depth
            return -(LogicVehicleDepth + LogicItemDepth + LogicSwimDepth) < ArchipelagoData.Locations[locID].Position.y;
        }

        public static void PrimeDepthSystem()
        {
            BaseDepth           = APState.SwimRule;
            LogicSwimDepth      = APState.SwimRule;
            SeaglideDepth       = APState.SeaglideDepth;
            ItemsRelevant       = APState.ConsiderItems;
            nonSeaglideDistance = APState.SeaglideDistance;
            CanSlipThrough      = APState.CanSlipThrough;
            IgnoreRadiation     = APState.IgnoreRadiation;
            IncludeSeamoth      = APState.SeamothState == APState.Inclusion.Included;
            IncludePrawn        = APState.PrawnState == APState.Inclusion.Included;
            IncludeCyclops      = APState.CyclopsState == APState.Inclusion.Included;
        }

        public static bool UpdateRadiationSuit()
        {
            return KnownTech.Contains(TechType.RadiationSuit);
        }

        public static int getTheoreticalLogicDepth(int candidateVehicleDepth)
        {
            int coreDepth = BaseDepth + candidateVehicleDepth;
            if (ItemsRelevant)
            {
                return coreDepth + SeaglideDepth + 150;
            }

            return coreDepth;
        }

        public static int UpdateLogicDepth()
        {
            int itemdepth = 0;
            if (! ItemsRelevant)
            {
                return itemdepth;
            }

            bool hasModStation = KnownTech.Contains(TechType.Workbench);

            if (KnownTech.Contains(TechType.Seaglide))
            {
                HasSeaglide = true;
                itemdepth += SeaglideDepth;
                // Ultra High Capacity Tank
                if (hasModStation && KnownTech.Contains(TechType.HighCapacityTank))
                {
                    itemdepth += 150;
                }
            }
            else if (hasModStation && KnownTech.Contains(TechType.UltraGlideFins))
            {
                itemdepth += 50;
                if (KnownTech.Contains(TechType.HighCapacityTank))
                {
                    itemdepth += 100;
                }
                else if (KnownTech.Contains(TechType.PlasteelTank))
                {
                    itemdepth += 25;
                }
            }
            else if (hasModStation && KnownTech.Contains(TechType.HighCapacityTank))
            {
                itemdepth += 100;
            }
            else if (hasModStation && KnownTech.Contains(TechType.PlasteelTank))
            {
                itemdepth += 25;
            }

            return itemdepth;
        }

        public static int getSeamothDepth(bool has_mod, bool has_upg)
        {
            if (! (IncludeSeamoth && KnownTech.Contains(TechType.Seamoth)))
            {
                return 0;
            }

            int depth = 200;
            if (has_upg && KnownTech.Contains(TechType.VehicleHullModule1))
            {
                depth = 300;
                if (has_mod && KnownTech.Contains(TechType.VehicleHullModule2))
                {
                    depth = 500;
                    if (KnownTech.Contains(TechType.VehicleHullModule3))
                    {
                        depth = 900;
                    }
                }
            }

            return depth;
        }

        public static int getPrawnDepth(bool has_mod, bool has_upg)
        {
            if (! (IncludePrawn && KnownTech.Contains(TechType.Exosuit)))
            {
                return 0;
            }

            int depth = 900;
            if (has_upg && KnownTech.Contains(TechType.ExoHullModule1))
            {
                depth = 1300;
                if (has_mod && KnownTech.Contains(TechType.ExoHullModule2))
                {
                    depth = 1700;
                }
            }

            return depth;
        }

        public static int getCyclopsDepth(bool has_mod, bool has_upg)
        {
            if (! (IncludeCyclops && KnownTech.Contains(TechType.Cyclops)))
            {
                return 0;
            }

            int depth = 500;
            if (KnownTech.Contains(TechType.CyclopsHullModule1))
            {
                depth = 900;
                if (has_mod && KnownTech.Contains(TechType.CyclopsHullModule2))
                {
                    depth = 1300;
                    if (KnownTech.Contains(TechType.CyclopsHullModule3))
                    {
                        depth = 1700;
                    }
                }
            }

            return depth;
        }

        public static void UpdateVehicleDepth()
        {
            int maxDepth = 0;
            string logicVehicleName = "Vehicle";

            if (!KnownTech.Contains(TechType.Constructor))
            {
                LogicVehicleDepth = 0;
                LogicVehicle = logicVehicleName;
                return;
            }

            bool hasModStation = KnownTech.Contains(TechType.Workbench);
            bool hasUpgradeConsole = KnownTech.Contains(TechType.BaseUpgradeConsole) &&
                                     KnownTech.Contains(TechType.BaseMoonpool);

            int candidateDepth = getSeamothDepth(hasModStation, hasUpgradeConsole);
            if (candidateDepth > maxDepth)
            {
                logicVehicleName = "Seamoth";
                maxDepth = candidateDepth;
            }

            candidateDepth = getPrawnDepth(hasModStation, hasUpgradeConsole);
            if (candidateDepth > maxDepth)
            {
                logicVehicleName = "Prawn";
                maxDepth = candidateDepth;
            }

            candidateDepth = getCyclopsDepth(hasModStation, hasUpgradeConsole);
            if (candidateDepth > maxDepth)
            {
                logicVehicleName = "Cyclops";
                maxDepth = candidateDepth;
            }

            LogicVehicle = logicVehicleName;
            LogicVehicleDepth = maxDepth;
        }

        public static void UpdateAdvancedDepth()
        {
            int maxDepth = 0;
            string logicVehicleName = "Vehicle";

            if (KnownTech.Contains(TechType.FarmingTray))
            {
                maxDepth += 500;
                logicVehicleName = "Advanced";
            }

            bool hasReactor = false;
            bool hasReactorCapableRoom = KnownTech.Contains(TechType.BaseRoom) || KnownTech.Contains(TechType.BaseLargeRoom);

            // Uraninite spawns at ~500m; don't assume any drops from AP
            if (BaseDepth + maxDepth > 500 && hasReactorCapableRoom && KnownTech.Contains(TechType.BaseNuclearReactor))
            {
                hasReactor = true;
                logicVehicleName = "Advanced";
            }

            // Bio fuel is easy to come by at all depths
            if (hasReactorCapableRoom && KnownTech.Contains(TechType.BaseBioReactor))
            {
                hasReactor = true;
                logicVehicleName = "Advanced";
            }

            // Need the transmitter to get the electricity back to the base
            if (KnownTech.Contains(TechType.ThermalPlant) && KnownTech.Contains(TechType.PowerTransmitter))
            {
                hasReactor = true;
                logicVehicleName = "Advanced";
            }
            if (hasReactor)
            {
                maxDepth += 1500;
            }

            LogicVehicle = logicVehicleName;
            LogicVehicleDepth = maxDepth;
        }

        public static void UpdateTrackedLocation()
        {
            float closestDist = 100000.0f;
            long closestID = -1;
            long trackingCount = 0;
            Vector3 playerPos = Player.main.gameObject.transform.position;

            foreach (var locID in APState.Session.Locations.AllMissingLocations)
            {
                // Check that it's a static location
                if (locID < creatureScanCutOff)
                {
                    trackingCount++;
                    // Skip locations not in logic
                    if (APState.TrackedMode == TrackerMode.Logical && !InLogic(locID))
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(playerPos, ArchipelagoData.Locations[locID].Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestID = locID;
                    }
                }
            }

            APState.TrackedLocationsCount = trackingCount;
            APState.TrackedDistance = closestDist;
            APState.TrackedLocation = closestID;
            if (closestID != -1)
            {
                APState.TrackedDepth = Convert.ToInt32(ArchipelagoData.Locations[closestID].Position.y);
                APState.TrackedLocationName = APState.Session.Locations.GetLocationNameFromId(APState.TrackedLocation);
                Vector3 directionVector = ArchipelagoData.Locations[closestID].Position - Player.main.gameObject.transform.position;
                directionVector.Normalize();
                APState.TrackedAngle = Vector3.Angle(directionVector, Player.main.viewModelCamera.transform.forward);
            }
        }

        public static void UpdateTrackedFish()
        {
            long maxFish = 7;
            var remainingFish = new List<long>();
            foreach (var locID in APState.Session.Locations.AllMissingLocations)
            {
                // Check that it's a static location
                if (locID > creatureScanCutOff && locID <= plantScanCutOff)
                {
                    remainingFish.Add(locID);
                }
            }

            APState.TrackedFishCount = remainingFish.Count;
            if (APState.TrackedFishCount == 0)
            {
                APState.TrackedFish = "";
                return;
            }

            remainingFish.Sort();
            var display_fish = new List<string>();
            for (int i = 0; i < Math.Min(APState.TrackedFishCount, maxFish); i++)
            {
                display_fish.Add(
                    APState.Session.Locations.GetLocationNameFromId(
                        remainingFish[i]).Replace(
                        " Scan", ""));
            }
            APState.TrackedFish = String.Join(", ", display_fish);
        }

        public static void UpdateTrackedPlants()
        {
            int maxPlants = 7;
            var remainingPlants = new List<long>();
            foreach (var locID in APState.Session.Locations.AllMissingLocations)
            {
                if (locID > plantScanCutOff)
                {
                    remainingPlants.Add(locID);
                }
            }

            APState.TrackedPlantCount = remainingPlants.Count;
            if (APState.TrackedPlantCount == 0)
            {
                APState.TrackedPlants = "";
                return;
            }

            remainingPlants.Sort();
            var display_plants = new List<string>();
            for (int i = 0; i < Math.Min(APState.TrackedPlantCount, maxPlants); i++)
            {
                display_plants.Add(
                    APState.Session.Locations.GetLocationNameFromId(
                        remainingPlants[i]).Replace(
                            " Scan", ""));
            }
            APState.TrackedPlants = String.Join(", ", display_plants);
        }

        // Debug.Log doesn't want to work in the thread, despite documentation saying it is threadsafe.
        // So this is the solution for now, and probably ever.
        // 
        // Example of logging for future use
        // Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " -- Doing Thing");
        public static void Log(string text)
        {
            using (StreamWriter sw = File.AppendText("TrackerThread.txt"))
            {
                sw.WriteLine(text);
            }
        }

        public static void DoWork()
        {
            while (true)
            {
                Thread.Sleep(150);

                if (APState.state != APState.State.InGame || APState.Session == null || Player.main == null)
                {
                    APState.TrackedFishCount = 0;
                    APState.TrackedPlantCount = 0;
                    APState.TrackedLocationsCount = 0;
                    APState.TrackedLocation = -1;
                    continue;
                }

                if (APState.SwimRule != BaseDepth)
                {
                    PrimeDepthSystem();
                }

                HasRadiationSuit = UpdateRadiationSuit();
                LogicItemDepth = UpdateLogicDepth();

                bool seamothCanMakeIt = false;
                if (IncludeSeamoth && getTheoreticalLogicDepth(900) > 1444)
                {
                    seamothCanMakeIt = true;
                }

                if (!seamothCanMakeIt && !IncludePrawn && !IncludeCyclops)
                {
                    UpdateAdvancedDepth();
                }
                else
                {
                    UpdateVehicleDepth();
                }

                UpdateTrackedLocation();
                UpdateTrackedFish();
                UpdateTrackedPlants();
            }
        }
    }
}
