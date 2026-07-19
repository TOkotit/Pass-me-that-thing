using System.Collections.Generic;
using System.Linq;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Game.Scripts.Utils;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    public class RoomItemSpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPositions;
        [SerializeField] private ItemSpawner itemSpawner;
        [SerializeField] private float spawnRate = 0.7f;          
        [SerializeField] private float spawnGrowthRate = 0.6f;
        [SerializeField] private LevelGraphBuilder graphBuilder;
        [Inject] ItemRarityDatabase _rarityDatabase;

        private int _maxItemCount;
        private float[] _weights;      
        private float _totalWeight;    

        private void Awake()
        {
            _maxItemCount = spawnPositions.Count;
            if (_maxItemCount > 0)
                CacheWeights();
        }

        private void CacheWeights()
        {
            _weights = new float[_maxItemCount + 1]; 
            _weights[0] = spawnRate;
            for (int i = 1; i <= _maxItemCount; i++)
            {
                _weights[i] = _weights[i - 1] * spawnGrowthRate;
            }
            _totalWeight = 0f;
            foreach (float w in _weights) _totalWeight += w;
        }

        public void SpawnItems(int depth)
        {
            var itemCount = GetRandomItemCount();
            var itemsToSpawn = new List<ItemData>();
            for (int i = 0; i < itemCount; i++)
            {
                var rarity = GetRandomRarity(depth, graphBuilder.MaxDepth);
                var itemsForRarity = _rarityDatabase.allItems.Where(item => item.Value == rarity);
                var randomIndex = Random.Range(0, itemsForRarity.Count());
                itemsToSpawn.Add(itemsForRarity.ElementAt(randomIndex).Key);
            }
            RandomUtilities.Shuffle(spawnPositions);
            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                itemSpawner.Item = itemsToSpawn[i];
                itemSpawner.PointToSpawn = spawnPositions[i];
                itemSpawner.Interact();
            }
            
        }

        private int GetRandomRarity(int depth, int maxDepth)
        {
            var availableRarities = _rarityDatabase.allItems.Values.Distinct().OrderBy(r => r).ToList();
            int maxRarity = availableRarities.Max();

            int targetRarity = RandomUtilities.RandomWeightedByParameter(depth, maxDepth, maxRarity,
                baseWeight: 1f, falloff: 0.1f);

            if (availableRarities.Contains(targetRarity))
                return targetRarity;

            return availableRarities.OrderBy(r => Mathf.Abs(r - targetRarity)).First();
        }
        private int GetRandomItemCount()
        {
            if (_maxItemCount == 0) return 0;
            return RandomUtilities.RandomWeightedIndex(_weights, _totalWeight);
        }
    }
}