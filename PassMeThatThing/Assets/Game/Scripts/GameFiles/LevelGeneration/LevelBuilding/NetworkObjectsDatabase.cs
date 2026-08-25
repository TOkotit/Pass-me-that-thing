using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [CreateAssetMenu(fileName = "NetworkObjectsDatabase", menuName = "Level Generation/Network Objects Database")]
    public class NetworkObjectsDatabase : ScriptableObject
    { 
        [SerializeField] private SerializedDictionary<NetworkObjectsOnLevelType, GameObject> entries;

        public bool TryGetPrefab(
            NetworkObjectsOnLevelType type,
            out GameObject prefab)
        {
            return entries.TryGetValue(type, out prefab);
        }
        
        public GameObject GetPrefab(NetworkObjectsOnLevelType type)
        {
            return entries.GetValueOrDefault(type);
        }
        
    }
}