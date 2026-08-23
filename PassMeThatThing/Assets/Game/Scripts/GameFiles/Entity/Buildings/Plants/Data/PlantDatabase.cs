using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data
{
    [CreateAssetMenu(fileName = "PlantDatabase", menuName = "Scriptable Objects/PlantDatabase")]
    public class PlantDatabase : ScriptableObject
    {
        public List<PlantData> allPlants;
        public List<PlantSeedData> allSeeds;

        public PlantData GetPlant(string id)
        {
            return allPlants.Find(b => b.id == id);
        }
        public PlantSeedData GetSeed(string id)
        {
            return allSeeds.Find(b => b.id == id);
        }
    }
}