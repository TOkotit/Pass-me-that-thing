using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class NetworkObjectsOrchestrator : NetworkBehaviour
    {
        
        public static NetworkObjectsOrchestrator Instance { get; private set; }
        
        [SerializeField] NetworkObjectsDatabase _networkObjectsDatabase;
        private List<NetworkObjectSpot> _levelSpots = new();
        private readonly List<NetworkObjectPlacement> _pendingObjects = new();
            
        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        
        public void SpawnNetworkObjects(List<NetworkObjectSpot> levelSpots)
        { 
            SetLevelSpots(levelSpots);
            
            if(!isServer) return;
            
            for (var i = 0; i < levelSpots.Count; i++)
            {
                var spot = levelSpots[i];

                if (spot == null)
                    continue;

                var type = spot.NetworkObjectsOnLevelType;
                if (!_networkObjectsDatabase.NetworkObjectsOnLevelSpots
                        .TryGetValue(type, out var prefab))
                {
                    Debug.LogWarning(
                        $"[NETWORK] Prefab for type {type} not found in NetworkObjectsDatabase.");

                    continue;
                }
                var createdObject = Instantiate(prefab);
                
                createdObject.transform.position = spot.transform.position;
                createdObject.transform.rotation = spot.transform.rotation;
                createdObject.transform.localScale = spot.transform.localScale;

                var placement = createdObject.GetComponent<NetworkObjectPlacement>();

                if (placement == null)
                {
                    Debug.LogError(
                        $"[NETWORK] {createdObject.name} has no NetworkObjectPlacement component.",
                        createdObject);

                    Destroy(createdObject);
                    continue;
                }

                placement.SetSpotIndex(i);

                NetworkServer.Spawn(createdObject);

            }
        }
        
        public void SetLevelSpots(List<NetworkObjectSpot> levelSpots)
        {
            _levelSpots = levelSpots ?? new List<NetworkObjectSpot>();

            ProcessPendingObjects();
        }
        
        public bool TryAttachObject(NetworkObjectPlacement placement)
        {
            if (placement == null)
                return false;

            if (_levelSpots == null || _levelSpots.Count == 0)
                return false;

            var spotIndex = placement.SpotIndex;

            if (spotIndex < 0 || spotIndex >= _levelSpots.Count)
            {
                Debug.LogWarning(
                    $"[NETWORK] Invalid spot index {spotIndex} for {placement.name}.");

                return false;
            }

            var spot = _levelSpots[spotIndex];

            if (spot == null)
            {
                Debug.LogWarning(
                    $"[NETWORK] Spot {spotIndex} is null.");

                return false;
            }

            var container = spot.SpawnContainer;

            if (container == null)
            {
                Debug.LogWarning(
                    $"[NETWORK] SpawnContainer for spot {spotIndex} is null.");

                return false;
            }

            var objectTransform = placement.transform;

            // Переносим network object в локальный контейнер клиента.
            objectTransform.SetParent(container, false);

            // Позиция спота может быть локальной относительно его parent,
            // поэтому сначала получаем его world transform,
            // а затем переводим его в координаты контейнера.
            objectTransform.localPosition =
                container.InverseTransformPoint(spot.transform.position);

            objectTransform.localRotation =
                Quaternion.Inverse(container.rotation) *
                spot.transform.rotation;

            // В твоей текущей структуре spot обычно является дочерним
            // контейнера, поэтому localScale можно сохранить напрямую.
            objectTransform.localScale = spot.transform.localScale;

            return true;
        }
        
        public void RegisterPendingObject(NetworkObjectPlacement placement)
        {
            if (placement == null)
                return;

            if (TryAttachObject(placement))
                return;

            if (!_pendingObjects.Contains(placement))
                _pendingObjects.Add(placement);
        }

        public void UnregisterPendingObject(NetworkObjectPlacement placement)
        {
            if (placement == null)
                return;

            _pendingObjects.Remove(placement);
        }

        private void ProcessPendingObjects()
        {
            if (_pendingObjects.Count == 0)
                return;

            for (var i = _pendingObjects.Count - 1; i >= 0; i--)
            {
                var placement = _pendingObjects[i];

                if (placement == null)
                {
                    _pendingObjects.RemoveAt(i);
                    continue;
                }

                if (TryAttachObject(placement))
                {
                    _pendingObjects.RemoveAt(i);
                }
            }
        }
    }
}