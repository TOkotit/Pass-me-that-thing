using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants
{
    public class Plant : MonoBehaviour
    {
        [SerializeField] private PlantData _plantData;

        public PlantData PlantData => _plantData;
    }
}