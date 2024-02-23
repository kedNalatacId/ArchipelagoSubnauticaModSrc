using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Archipelago;

public enum ArchipelagoItemType
{
    Technology = 1,
    Resource = 2,
    Group = 3,
}

// Data coming Archipelago Export
public static class ArchipelagoData
{
    public static bool Initialized;
    public static Dictionary<string, long> Encyclopdia;
    public static Dictionary<TechType, List<long>> LogicDict;
    public static Dictionary<long, TechType> ItemCodeToTechType = new ();
    public static Dictionary<long, APState.Location> Locations = new ();
    public static Dictionary<long, List<long>> GroupItems = new ();
    public static Dictionary<long, ArchipelagoItemType> ItemCodeToItemType = new();

    public static T ReadJSON<T>(string filename)
    {
        try
        {
            var reader = File.OpenText(BepInEx.Paths.PluginPath+"/Archipelago/" + filename + ".json");
            var content = reader.ReadToEnd();
            reader.Close();
            var data = JsonConvert.DeserializeObject<T>(content);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("Could not read " + filename + ".json\n" + e);
            return default;
        }
    }

    public static void Init()
    {
        if (Initialized)
        {
            return;
        }
        
        // Load items.json
        var itemsData = ReadJSON<Dictionary<long, string>>("items");
        foreach (var itemJson in itemsData)
        {
            // not all tech types exist in both games
            var success = Enum.TryParse(itemJson.Value, out TechType tech);
            if (success)
            {
                ItemCodeToTechType[itemJson.Key] = tech;
            }
        }
        
        // Load group_items.json
        GroupItems = ReadJSON<Dictionary<long, List<long>>>("group_items");

        // Load locations.json
        var locationsData = ReadJSON<Dictionary<long, Dictionary<string, float>>>("locations");
        foreach (var locationJson in locationsData)
        {
            APState.Location location = new APState.Location();
            location.ID = locationJson.Key;
            var vec = locationJson.Value;
            location.Position = new Vector3(
                vec["x"],
                vec["y"],
                vec["z"]
            );

            // json file might not be updated with new values; fake it if not
            if (vec.TryGetValue("distance_to_origin", out var orig_dist))
            {
                location.distance_to_origin = (float)orig_dist;
            }
            else
            {
                /*
                    # Value not in json, pre-calculate by hand, based on python:
                    map_center_dist = math.sqrt(pos_x ** 2 + pos_z ** 2)
                */
                location.distance_to_origin = (float)Math.Sqrt(Math.Pow(vec["x"], 2) + Math.Pow(vec["z"], 2));
            }

            if (vec.TryGetValue("distance_from_radiation", out var rad_dist))
            {
                location.distance_from_radiation = (float)rad_dist;
            }
            else
            {
                /*
                    # Value not in json, pre-calculate by hand, based on python:
                    aurora_dist = math.sqrt((x - 1038.0) ** 2 + y ** 2 + (z - -163.1) ** 2)
                    return aurora_dist < 950
                */
                location.distance_from_radiation = (float)Math.Sqrt(
                    Math.Pow(vec["x"] - 1038, 2) + Math.Pow(vec["y"], 2) + Math.Pow(vec["z"] + 163.1, 2)
                );
            }

            Locations.Add(location.ID, location);
        }

        // Load encyclopedia.json
        Encyclopdia = ReadJSON<Dictionary<string, long>>("encyclopedia");
        
        // Load logic.json
        LogicDict = ReadJSON<Dictionary<TechType, List<long>>>("logic");
        
        // Load item_types.json
        var itemTypesData = ReadJSON<Dictionary<ArchipelagoItemType, List<long>>>("item_types");

        foreach (var type in itemTypesData)
        {
            foreach (var itemCode in type.Value)
            {
                ItemCodeToItemType[itemCode] = type.Key;
            }
        }

        Debug.Log("ItemCodeToItemType " + JsonConvert.SerializeObject(ItemCodeToItemType));
        Initialized = true;
    }
}
