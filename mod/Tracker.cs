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

                    APState.TrackedLocation = closestID;
                    APState.TrackedDistance = closestDist;
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