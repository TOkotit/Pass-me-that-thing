using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingsDatabase", menuName = "Scriptable Objects/BuildingsDatabase")]
public class BuildingsDatabase : ScriptableObject
{
    public List<BuildingData> allBuildings;
    
    public BuildingData GetBuilding(string id)
    {
        return allBuildings.Find(b => b.id == id);
    }
}
