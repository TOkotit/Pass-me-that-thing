using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [Serializable]
    public struct NetworkObjectEntry
    {
        public NetworkObjectsOnLevelType Type;
        public GameObject Prefab;
    }
    
    [CreateAssetMenu(fileName = "NetworkObjectsDatabase", menuName = "Level Generation/Network Objects Database")]
    public class NetworkObjectsDatabase : ScriptableObject
    { 
        [SerializeField] private List<NetworkObjectEntry> entries = new();
        
        public Dictionary<NetworkObjectsOnLevelType, GameObject> NetworkObjectsOnLevelSpots { get; private set; }

        private void OnEnable()
        {
            NetworkObjectsOnLevelSpots = new Dictionary<NetworkObjectsOnLevelType, GameObject>();
            
            foreach (var entry in entries)
            {
                if (!NetworkObjectsOnLevelSpots.ContainsKey(entry.Type))
                {
                    NetworkObjectsOnLevelSpots.Add(entry.Type, entry.Prefab);
                }
            }
        }
        
        public GameObject GetPrefab(NetworkObjectsOnLevelType type)
        {
            if (Application.isPlaying && NetworkObjectsOnLevelSpots != null && NetworkObjectsOnLevelSpots.TryGetValue(type, out var prefab))
            {
                return prefab;
            }
    
            foreach (var entry in entries)
            {
                if (entry.Type == type) return entry.Prefab;
            }
    
            return null;
        }
        
    }
}