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

        //private Dictionary<LevelRoom, List<NetworkRarityItemSpot>> _levelSpots = new();
        //private readonly List<NetworkObjectPlacement> _pendingObjects = new();

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
                var itemCount = r.Key.GetRandomItemCount();
                Debug.Log($"SpawnNetworkRarityItem ItemCount {itemCount}");
                for (var i = 0; i < r.Value.Count && i < itemCount; i++)
                {
                    var spot = r.Value[i];

                    if (spot == null)
                        continue;

                    //var rarity = GetRandomRarity(depth, maxDepth);
                    var rarity = GetRandomRarity();

                    var itemsForRarity = _rarityDatabase.GetItemsByRarity(rarity);
                    
                    if (itemsForRarity.Count() == 0) 
                        return; //временно пока у некоторых редкостей нет предметов

                    var randomIndex = Random.Range(0, itemsForRarity.Count());
                    var itemId = itemsForRarity.ElementAt(randomIndex).ItemData.Id;

                    ServerSpawnItem(itemId, spot.Position);

                    Debug.Log($"Spawning item: {itemId}");



                    //var createdObject = Instantiate(prefab);

                    //createdObject.transform.position = spot.transform.position;
                    //createdObject.transform.rotation = spot.transform.rotation;
                    //var placement = createdObject.GetComponent<NetworkObjectPlacement>();

                    //if (placement == null)
                    //{
                    //    Debug.LogError(
                    //        $"[NETWORK] {createdObject.name} has no NetworkObjectPlacement component.",
                    //        createdObject);

                    //    Destroy(createdObject);
                    //    continue;
                    //}

                    //placement.SetSpotIndex(i);

                    //NetworkServer.Spawn(createdObject);

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


        }


        //public void SetLevelSpots(List<NetworkRarityItemSpot> levelSpots)
        //{
        //    _levelSpots = levelSpots ?? new List<NetworkRarityItemSpot>();

        //    //ProcessPendingObjects();
        //}

        //public bool TryAttachObject(NetworkObjectPlacement placement)
        //{
        //    if (placement == null)
        //        return false;

        //    if (_levelSpots == null || _levelSpots.Count == 0)
        //        return false;

        //    var spotIndex = placement.SpotIndex;

        //    if (spotIndex < 0 || spotIndex >= _levelSpots.Count)
        //    {
        //        Debug.LogWarning(
        //            $"[NETWORK] Invalid spot index {spotIndex} for {placement.name}.");

        //        return false;
        //    }

        //    var spot = _levelSpots[spotIndex];

        //    if (spot == null)
        //    {
        //        Debug.LogWarning(
        //            $"[NETWORK] Spot {spotIndex} is null.");

        //        return false;
        //    }

        //    var container = spot.SpawnContainer;

        //    if (container == null)
        //    {
        //        Debug.LogWarning(
        //            $"[NETWORK] SpawnContainer for spot {spotIndex} is null.");

        //        return false;
        //    }

        //    var objectTransform = placement.transform;

        //    objectTransform.SetParent(container, false);

        //    objectTransform.localPosition =
        //        container.InverseTransformPoint(spot.transform.position);

        //    objectTransform.localRotation =
        //        Quaternion.Inverse(container.rotation) *
        //        spot.transform.rotation;
        //    return true;
        //}

        //public void RegisterPendingObject(NetworkObjectPlacement placement)
        //{
        //    if (placement == null)
        //        return;

        //    if (TryAttachObject(placement))
        //        return;

        //    if (!_pendingObjects.Contains(placement))
        //        _pendingObjects.Add(placement);
        //}

        //public void UnregisterPendingObject(NetworkObjectPlacement placement)
        //{
        //    if (placement == null)
        //        return;

        //    _pendingObjects.Remove(placement);
        //}

        //private void ProcessPendingObjects()
        //{
        //    if (_pendingObjects.Count == 0)
        //        return;

        //    for (var i = _pendingObjects.Count - 1; i >= 0; i--)
        //    {
        //        var placement = _pendingObjects[i];

        //        if (placement == null)
        //        {
        //            _pendingObjects.RemoveAt(i);
        //            continue;
        //        }

        //        if (TryAttachObject(placement))
        //        {
        //            _pendingObjects.RemoveAt(i);
        //        }
        //    }
        //}
    }
}