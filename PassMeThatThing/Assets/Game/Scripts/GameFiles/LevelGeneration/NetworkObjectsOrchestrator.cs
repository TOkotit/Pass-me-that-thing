using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class NetworkObjectsOrchestrator : NetworkBehaviour
    {
        [SerializeField] NetworkObjectsDatabase _networkObjectsDatabase;
        
        public void SpawnNetworkObjects(List<NetworkObjectSpot> levelSpots)
        {
            if(!isServer) return;
            foreach (var spot in levelSpots)
            {
                var type = spot.NetworkObjectsOnLevelType;
                if (_networkObjectsDatabase.NetworkObjectsOnLevelSpots.TryGetValue(type, out var prefab))
                {
                    var netObject = Instantiate(prefab, spot.transform.position, spot.transform.rotation);
                    NetworkServer.Spawn(netObject);
                }
                else
                {
                    Debug.LogWarning($"[NETWORK] Prefab for type {type} not found in NetworkObjectsDatabase.");
                }
            }
        }
    }
}