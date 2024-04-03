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
        public static bool ExtGrowbedRelevant = false;
        public static int BaseDepth           = 200;
        public static int SeaglideDepth       = 200;
        public static int nonSeaglideDistance = 800;
        public static int LogicSwimDepth      = BaseDepth;
        public static int LogicItemDepth      = 0;
        public static int LogicVehicleDepth   = 0;
        public static bool HasSeaglide        = false;
        public static bool HasRadiationSuit   = false;
        public static bool IgnoreRadiation    = false;
        public static bool ElidePrawn         = false;
        public static string LogicVehicle     = "Vehicle";
        public static long scanCutOff         = 33999;

        public static bool InLogic(long locID)
        {
            // Gating items
            foreach (var logic in ArchipelagoData.LogicDict)
            {
                if (!KnownTech.Contains(logic.Key) && logic.Value.Contains(locID))
                {
                    return false;
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
            if (! (HasSeaglide || LogicVehicle == "Seamoth" || LogicVehicle == "Cyclops") && ArchipelagoData.Locations[locID].distance_to_origin > nonSeaglideDistance)
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
            ExtGrowbedRelevant  = APState.ConsiderExtGrowbed;
            nonSeaglideDistance = APState.SeaglideDistance;
            IgnoreRadiation     = APState.IgnoreRadiation;
            ElidePrawn          = APState.ElidePrawn;
        }

        public static bool UpdateRadiationSuit()
        {
            return KnownTech.Contains(TechType.RadiationSuit);
        }

        public static int UpdateLogicDepth()
        {
            int itemdepth = 0;
            if (! ItemsRelevant)
            {
                return itemdepth;
            }

            if (ExtGrowbedRelevant && KnownTech.Contains(TechType.FarmingTray))
            {
                itemdepth += 500;
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
            if (! KnownTech.Contains(TechType.Seamoth))
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
            if (ElidePrawn || ! KnownTech.Contains(TechType.Exosuit))
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
            if (! KnownTech.Contains(TechType.Cyclops))
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

        public static void UpdateTrackedLocation()
        {
            float closestDist = 100000.0f;
            long closestID = -1;
            long trackingCount = 0;
            Vector3 playerPos = Player.main.gameObject.transform.position;

            foreach (var locID in APState.Session.Locations.AllMissingLocations)
            {
                // Check that it's a static location
                if (locID < scanCutOff)
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
                if (locID > scanCutOff)
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
                UpdateVehicleDepth();
                UpdateTrackedLocation();
                UpdateTrackedFish();
                // UpdateTrackedPlants();
            }
        }
    }
}
