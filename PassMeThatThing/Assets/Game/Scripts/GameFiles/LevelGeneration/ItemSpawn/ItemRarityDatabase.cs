using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    [CreateAssetMenu(fileName = "RarityDatabase", menuName = "Scriptable Objects/RarityDatabase")]
    public class ItemRarityDatabase : ScriptableObject
    {
        [SerializedDictionary] public SerializedDictionary <ItemData, int> allItems;
    
        public int GetItemRarity(ItemData id)
        {
            return allItems[id];
        }
    }
}