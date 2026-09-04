using Game.Scripts.Enums;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    [CreateAssetMenu(fileName = "ItemRarityData", menuName = "Scriptable Objects/ItemRarityData")]
    public class ItemRarityData : ScriptableObject
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private ItemRarityType rarityType;

        public ItemData ItemData => itemData;
        public ItemRarityType RarityType => rarityType;
    }
}