using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Enums;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    public class NetworkRarityItemSpot : MonoBehaviour
    {
        [SerializeField] private Vector3 previewGizmosSize = new Vector3(1f,1f,1f);

        [Header("специальные настройки (в процессе работы)")]
        [SerializeField] private bool useConstSpawnChance;
        [SerializeField] private float constSpawnChance;

        [SerializeField] private bool useConstRarityType;
        [SerializeField] private ItemRarityType constRarityType;

        [SerializeField] private bool useConstItem;
        [SerializeField] private ItemRarityData constItem;

        public Vector3 Position => transform.position;

        public bool UseConstSpawnChance => useConstSpawnChance;
        public float ConstSpawnChance => constSpawnChance;
        public bool UseConstRarityType => useConstRarityType;
        public ItemRarityType ConstRarityType => constRarityType;
        public bool UseConstItem => useConstItem;
        public ItemRarityData ConstItem  => constItem;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
            Gizmos.DrawWireCube(transform.position, previewGizmosSize);
        }
    }
}