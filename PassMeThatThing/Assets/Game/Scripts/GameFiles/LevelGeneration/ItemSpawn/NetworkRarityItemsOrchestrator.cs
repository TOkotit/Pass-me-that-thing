using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Utils;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;

namespace Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    public class NetworkRarityItemsOrchestrator : NetworkBehaviour
    {
        public static NetworkRarityItemsOrchestrator Instance { get; private set; }

        private ItemRarityDatabase _rarityDatabase;
        private ItemPoolManager _itemPoolManager;
        private PhysicalItemRegistry _physicalItemRegistry;

        private List<ItemRarityType> _availableRarities;
        private float[] _availableRaritiesWeights;
        private float _totalWeight;

        //Приходят из контейнера из LevelOrchestrator Construct
        public void Init(ItemRarityDatabase rarityDatabase,
            ItemPoolManager itemPoolManager,
            PhysicalItemRegistry physicalItemRegistry)
        {
            _rarityDatabase = rarityDatabase;
            _itemPoolManager = itemPoolManager;
            _physicalItemRegistry = physicalItemRegistry;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SpawnNetworkRarityItem(Dictionary<LevelRoom, List<NetworkRarityItemSpot>> levelSpots)
        {
            if (!isServer) return;

            _availableRarities = _rarityDatabase.BaseChancesToRarity.Keys.ToList();
            _availableRaritiesWeights = _rarityDatabase.BaseChancesToRarity.Values.ToArray();
            _totalWeight = _rarityDatabase.BaseChancesToRarityTotal;



            foreach (var r in levelSpots)
            {
                r.Key.CacheItemCountWeights();
                
                //не учитываются споты с UseConstSpawnChance
                var maxRandomItemCount = r.Key.GetRandomItemCount();
                var itemCount = 0;

                for (var i = 0; i < r.Value.Count; i++)
                {
                    var spot = r.Value[i];

                    if (spot == null)
                        continue;

                    if (spot.UseConstSpawnChance)
                    {
                        if (!(Random.Range(0, 100) <= spot.ConstSpawnChance * 100))
                            continue;
                    }
                    else
                    {
                        if (itemCount >= maxRandomItemCount)
                            continue;
                    }

                    if (spot.UseConstItem)
                    {
                        ServerSpawnItem(spot.ConstItem.ItemData.Id, spot.Position);
                        if (!spot.UseConstSpawnChance)
                            itemCount++;
                    }
                    else
                    {
                        ItemRarityType rarity;
                        if (spot.UseConstRarityType)
                        {
                            rarity = spot.ConstRarityType;
                        }
                        else
                        {
                            rarity = GetRandomRarity();
                            //var rarity = GetRandomRarity(depth, maxDepth);
                        }

                        var itemsForRarity = _rarityDatabase.GetItemsByRarity(rarity);

                        if (itemsForRarity.Count() == 0)
                            continue; //временно пока у некоторых редкостей нет предметов

                        var randomItemIndex = Random.Range(0, itemsForRarity.Count());
                        var itemId = itemsForRarity.ElementAt(randomItemIndex).ItemData.Id;

                        ServerSpawnItem(itemId, spot.Position);
                        if (!spot.UseConstSpawnChance)
                            itemCount++;
                    }
                }
            }
        }

        private ItemRarityType GetRandomRarity()
        {
            return _availableRarities[
                RandomUtilities.RandomWeightedIndex(_availableRaritiesWeights, _totalWeight)];
        }

        private int GetRandomRarity(int depth, int maxDepth)
        {
            var availableRarities = _rarityDatabase.AllRarityItems.Select(r => (int)r.RarityType).Distinct().OrderBy(r => r).ToList();
            int maxRarity = availableRarities.Max();

            int targetRarity = RandomUtilities.RandomWeightedByParameter(depth, maxDepth, maxRarity,
                baseWeight: 1f, falloff: 0.1f);

            if (availableRarities.Contains(targetRarity))
                return targetRarity;

            return availableRarities.OrderBy(r => Mathf.Abs(r - targetRarity)).First();
        }


        [Server]
        public void ServerSpawnItem(string itemId, Vector3 pos)
        {
            var itemToDrop = _itemPoolManager.CreateNewObject(itemId);
            itemToDrop.transform.position = pos;

            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            _physicalItemRegistry.Register(physItem);

            Debug.Log($"Spawning item: {itemId}");
        }
    }
}