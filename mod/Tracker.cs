using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Archipelago
{
    public class TrackerThread
    {
        public static void DoWork()
        {
            float closestDist;
            float dist;
            long closestID;
            long scanCutOff = 33999;
            long maxFish = 7;

            Vector3 playerPos;
            
            while (true)
            {
                // locations
                long tracking_count = 0;
                if (APState.state == APState.State.InGame && APState.Session != null && Player.main != null)
                {
                    playerPos = Player.main.gameObject.transform.position;
                    
                    closestDist = 100000.0f;
                    closestID = -1;
                    foreach (var locID in APState.Session.Locations.AllMissingLocations)
                    {
                        // Check that it's a static location
                        if (locID < scanCutOff)
                        {
                            tracking_count++;
                            dist = Vector3.Distance(playerPos, APState.LOCATIONS[locID].Position);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closestID = locID;
                            }
                        }
                    }

                    APState.TrackedLocationsCount = tracking_count;
                    APState.TrackedDistance = closestDist;
                    APState.TrackedLocation = closestID;
                    if (closestID != -1)
                    {
                        APState.TrackedLocationName =
                            APState.Session.Locations.GetLocationNameFromId(APState.TrackedLocation);
                        Vector3 directionVector = APState.LOCATIONS[closestID].Position - Player.main.gameObject.transform.position;
                        directionVector.Normalize();
                        APState.TrackedAngle = Vector3.Angle(directionVector, Player.main.viewModelCamera.transform.forward);
                    }
                }
                else
                {
                    APState.TrackedLocationsCount = 0;
                    APState.TrackedLocation = -1;
                }
                // fish
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
                            display_fish.Add(APState.Session.Locations.GetLocationNameFromId(remainingFish[i]).Replace(" Scan", ""));
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

                Thread.Sleep(100);
            }
        }
    }
}