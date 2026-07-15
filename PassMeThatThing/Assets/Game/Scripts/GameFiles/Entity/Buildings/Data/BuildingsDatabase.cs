using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "BuildingsDatabase", menuName = "Scriptable Objects/BuildingsDatabase")]
public class BuildingsDatabase : ScriptableObject
{
    public List<BuildingData> buildings;
    
    public List<BuildingData> miniBuildings;

    public List<BuildingData> AllBuildings { get; set; } = new();

    public BuildingData GetBuildingFromAll(string id)
    {
        if (AllBuildings.Count == 0)
        {
            AllBuildings = buildings.Concat(miniBuildings).ToList();
        }
        return AllBuildings.Find(b => b.id == id);
    }
    
    public BuildingData GetMiniBuilding(string id)
    {
        return miniBuildings.Find(b => b.id == id);
    }
}
