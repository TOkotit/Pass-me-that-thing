using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    [CreateAssetMenu(fileName = "RarityDatabase", menuName = "Scriptable Objects/RarityDatabase")]
    public class ItemRarityDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemRarityData> allRarityItems;
        [SerializeField] private SerializedDictionary<ItemRarityType, float> baseChancesToRarity;

        public SerializedDictionary<ItemRarityType, float> BaseChancesToRarity => baseChancesToRarity;

        public List<ItemRarityData> AllRarityItems => allRarityItems;

        public ItemRarityData GetItemRarity(string id)
        {
            return allRarityItems.Find(b => b.ItemData.Id == id);
        }
    }
}