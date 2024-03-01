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
        public static string LogicVehicle     = "Vehicle";

        public static bool InLogic(long locID)
        {
            // Gating items
            foreach (var logic in ArchipelagoData.LogicDict)
            {
                bool hasItem;
                try
                {
                    hasItem = KnownTech.Contains(logic.Key);
                }
                catch (NullReferenceException)
                {
                    hasItem = false;
                }

                if (!hasItem && logic.Value.Contains(locID))
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

            // Distance -- if we don't have the seaglide, too far away isn't in logic
            if (! HasSeaglide && ArchipelagoData.Locations[locID].distance_to_origin > nonSeaglideDistance)
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
        }

        public static bool UpdateRadiationSuit()
        {
            bool hasRadSuit = false;
            try
            {
                hasRadSuit = KnownTech.Contains(TechType.RadiationSuit);
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return hasRadSuit;
        }

        public static int UpdateLogicDepth()
        {
            int itemdepth = 0;
            if (! ItemsRelevant)
            {
                return itemdepth;
            }

            try
            {
                bool hasExteriorGrowbed = KnownTech.Contains(TechType.FarmingTray);
                if (hasExteriorGrowbed && ExtGrowbedRelevant)
                {
                    itemdepth += 500;
                }
            }
            catch (NullReferenceException) { }

            bool hasModStation = false;
            try
            {
                hasModStation = KnownTech.Contains(TechType.Workbench);
            }
            catch (NullReferenceException)
            {
                return itemdepth;
            }

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

        public static void UpdateVehicleDepth()
        {
            bool hasBay;
            int maxDepth = 0;
            string logicVehicleName = "Vehicle";

            try
            {
                hasBay = KnownTech.Contains(TechType.Constructor);
            }
            catch (Exception)
            {
                return;
            }

            if (!hasBay)
            {
                LogicVehicleDepth = 0;
                LogicVehicle = logicVehicleName;
                return;
            }

            bool hasModStation = KnownTech.Contains(TechType.Workbench);
            bool hasUpgradeConsole = KnownTech.Contains(TechType.BaseUpgradeConsole) &&
                                     KnownTech.Contains(TechType.BaseMoonpool);
            int oldDepth = maxDepth;

            if (KnownTech.Contains(TechType.Seamoth))
            {
                maxDepth = Math.Max(maxDepth, 200);
                if (hasUpgradeConsole && KnownTech.Contains(TechType.VehicleHullModule1))
                {
                    maxDepth = Math.Max(maxDepth, 300);
                    if (hasModStation && KnownTech.Contains(TechType.VehicleHullModule2))
                    {
                        maxDepth = Math.Max(maxDepth, 500);
                        if (KnownTech.Contains(TechType.VehicleHullModule3))
                        {
                            maxDepth = Math.Max(maxDepth, 900);
                        }
                    }
                }
                if (Math.Abs(oldDepth - maxDepth) > 1)
                {
                    logicVehicleName = "Seamoth";
                }
            }
            oldDepth = maxDepth;
            if (KnownTech.Contains(TechType.Exosuit))
            {
                maxDepth = Math.Max(maxDepth, 900);
                if (hasUpgradeConsole && KnownTech.Contains(TechType.ExoHullModule1))
                {
                    maxDepth = Math.Max(maxDepth, 1300);
                    if (hasModStation && KnownTech.Contains(TechType.ExoHullModule2))
                    {
                        maxDepth = Math.Max(maxDepth, 1700);
                    }
                }
                if (Math.Abs(oldDepth - maxDepth) > 1)
                {
                    logicVehicleName = "Prawn Suit";
                }
            }
            oldDepth = maxDepth;
            if (KnownTech.Contains(TechType.Cyclops))
            {
                maxDepth = Math.Max(maxDepth, 500);
                if (KnownTech.Contains(TechType.CyclopsHullModule1))
                {
                    maxDepth = Math.Max(maxDepth, 900);
                    if (hasModStation && KnownTech.Contains(TechType.CyclopsHullModule2))
                    {
                        maxDepth = Math.Max(maxDepth, 1300);
                        if (KnownTech.Contains(TechType.CyclopsHullModule3))
                        {
                            maxDepth = Math.Max(maxDepth, 1700);
                        }
                    }
                }
                if (Math.Abs(oldDepth - maxDepth) > 1)
                {
                    logicVehicleName = "Cyclops";
                }
            }

            LogicVehicle = logicVehicleName;
            LogicVehicleDepth = maxDepth;
        }

        public static void UpdateTrackedLocation()
        {
            float closestDist;
            float dist;
            long closestID;
            long trackingCount = 0;
            long scanCutOff = 33999;

            if (APState.state == APState.State.InGame && APState.Session != null && Player.main != null)
            {
                Vector3 playerPos = Player.main.gameObject.transform.position;

                closestDist = 100000.0f;
                closestID = -1;
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
                        dist = Vector3.Distance(playerPos, ArchipelagoData.Locations[locID].Position);
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
                APState.TrackedDepth = Convert.ToInt32(Math.Round(ArchipelagoData.Locations[closestID].Position.y));
                if (closestID != -1)
                {
                    APState.TrackedLocationName =
                        APState.Session.Locations.GetLocationNameFromId(APState.TrackedLocation);
                    Vector3 directionVector = ArchipelagoData.Locations[closestID].Position -
                                              Player.main.gameObject.transform.position;
                    directionVector.Normalize();
                    APState.TrackedAngle = Vector3.Angle(directionVector,
                        Player.main.viewModelCamera.transform.forward);
                }
            }
            else
            {
                APState.TrackedLocationsCount = 0;
                APState.TrackedLocation = -1;
            }
        }

        public static void UpdateTrackedFish()
        {
            long scanCutOff = 33999;
            long maxFish = 7;

            if (APState.Session != null)
            {
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
                if (APState.TrackedFishCount != 0)
                {
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
                else
                {
                    APState.TrackedFish = "";
                }
            }
            else
            {
                APState.TrackedFishCount = 0;
            }
        }

        // Debug.Log doesn't want to work for me in the thread, despite documentation saying it is threadsafe.
        // So this is the solution for now, and probably ever.
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
                if (APState.SwimRule != BaseDepth)
                {
                    PrimeDepthSystem();
                }

                HasRadiationSuit = UpdateRadiationSuit();
                LogicItemDepth = UpdateLogicDepth();
                UpdateVehicleDepth();
                UpdateTrackedLocation();
                UpdateTrackedFish();

                Thread.Sleep(150);
            }
        }
    }
}
