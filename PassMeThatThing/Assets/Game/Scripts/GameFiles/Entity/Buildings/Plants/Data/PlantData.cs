using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data
{
    [CreateAssetMenu(fileName = "PlantData", menuName = "Scriptable Objects/PlantData")]
    public class PlantData : ScriptableObject
    {
        public string id;
        public string plantName;
        public GameObject saplingPrefab;
        public GameObject fruitPrefab;
        public Sprite plantImage;

        public float growTime;
        public float fruitsAmount;
    }
}