using VContainer;
using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using FishNet;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemPoolManager : MonoBehaviour
    {
        [SerializeField] private ItemDatabase database;
        
        private readonly Dictionary<string, Stack<NetworkObject>> _poolDict = new();

        public void Start()
        {
            InitializePool();
        }

        public void InitializePool()
        {
            foreach (var item in database.allItems)
            {
                _poolDict[item.Id] = new Stack<NetworkObject>();
            }
        }

        // --- Взятие из пула ---
        public NetworkObject GetFromPool(string id, Vector3 position, Quaternion rotation)
        {
            NetworkObject netObj = null;

            if (_poolDict.TryGetValue(id, out var stack) && stack.Count > 0)
            {
                netObj = stack.Pop();
                netObj.transform.position = position;
                netObj.transform.rotation = rotation;
                netObj.gameObject.SetActive(true);
            }
            else
            {
                var data = database.GetItem(id);
                var go = Instantiate(data.WorldPrefab, position, rotation);
                netObj = go.GetComponent<NetworkObject>();
                
                if (go.TryGetComponent<NetworkItem>(out var networkItem))
                {
                    networkItem.itemId.Value = id;
                }
            }

            return netObj;
        }

        // --- Возврат в пул ---
        public void ReturnToPool(NetworkObject spawned)
        {
            if (spawned.TryGetComponent<NetworkItem>(out var networkItem) && !string.IsNullOrEmpty(networkItem.itemId.Value))
            {
                var id = networkItem.itemId;
                
                spawned.gameObject.SetActive(false);
                spawned.transform.SetParent(null);

                if (!_poolDict.ContainsKey(id.Value))
                {
                    _poolDict[id.Value] = new Stack<NetworkObject>();
                }

                _poolDict[id.Value].Push(spawned);
            }
            else
            {
                // Если это не наш предмет, просто уничтожаем
                Destroy(spawned.gameObject);
            }
        }
    }
}