using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data
{
    [CreateAssetMenu(fileName = "PlantSeedData", menuName = "Scriptable Objects/PlantSeedData")]
    public class PlantSeedData : ScriptableObject
    {
        public string id;
        public string seedName;
        public GameObject seedPrefab;
        public Sprite seedImage;

        public List<PlantData> plants;
    }
}