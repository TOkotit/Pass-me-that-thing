using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants
{
    public class PlantSeed : MonoBehaviour
    {
        [SerializeField] private PlantSeedData _seedData;

        public PlantSeedData SeedData => _seedData;
    }
}