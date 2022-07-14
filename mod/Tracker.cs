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

            Vector3 playerPos;
            
            while (true)
            {
                
                if (APState.state == APState.State.InGame && APState.Session != null && Player.main != null)
                {
                    playerPos = Player.main.gameObject.transform.position;
                    
                    closestDist = 100000.0f;
                    closestID = -1;
                    foreach (var locID in APState.Session.Locations.AllMissingLocations)
                    {
                        dist = Vector3.Distance(playerPos, APState.LOCATIONS[locID].Position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestID = locID;
                        }
                    }

                    if (closestID != APState.TrackedLocation)
                    {
                        APState.TrackedLocation = closestID;
                        APState.TrackedDistance = closestDist;
                        APState.TrackedLocationName =
                            APState.Session.Locations.GetLocationNameFromId(APState.TrackedLocation);
                        Vector3 directionVector = APState.LOCATIONS[closestID].Position - Player.main.gameObject.transform.position;
                        directionVector.Normalize();
                        APState.TrackedAngle = Vector3.Angle(directionVector, Player.main.viewModelCamera.transform.forward);
                    }
                }
                else
                {
                    APState.TrackedLocation = -1;
                }
                Thread.Sleep(100);
            }
        }
    }
}