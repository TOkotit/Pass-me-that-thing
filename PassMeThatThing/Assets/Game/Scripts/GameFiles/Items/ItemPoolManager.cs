using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemPoolManager : MonoBehaviour
    {
        [SerializeField] private ItemDatabase database;
        
        private Dictionary<string, List<NetworkItem>> _poolDict = new ();

        public void Start()
        {
            InitializePool();
        }

        public void InitializePool()
        {
            foreach (var item in database.allItems)
            {
                if (!_poolDict.ContainsKey(item.Id))
                    _poolDict[item.Id] = new List<NetworkItem>();

                NetworkClient.RegisterPrefab(item.WorldPrefab, 
                    (msg) => SpawnHandler(msg, item.Id), 
                    UnspawnHandler);
            }
        }

        public GameObject SpawnHandler(SpawnMessage msg, string itemId)
        {
            var obj = GetFromPool(itemId); 
            obj.transform.position = msg.position;
            obj.transform.rotation = msg.rotation;
            obj.SetActive(true);
            return obj;
        }

        public void UnspawnHandler(GameObject spawned)
        {
            spawned.SetActive(false);
            if (spawned.TryGetComponent<NetworkItem>(out var networkItem))
            {
                if (_poolDict.TryGetValue(networkItem.itemId, out var list))
                {
                    if (!list.Contains(networkItem)) list.Add(networkItem);
                    Debug.Log($"[IP] Убрано в пул: {networkItem.itemId} (Экземпляр: {networkItem.instanceId})");
                }
            }
        }
        
        public GameObject GetFromPool(string id, string requiredInstanceId = null)
        {
            if (_poolDict.TryGetValue(id, out var list) && list.Count > 0)
            {
                NetworkItem selectedObj = null;

                if (!string.IsNullOrEmpty(requiredInstanceId))
                {
                    selectedObj = list.Find(obj 
                        => obj.instanceId == requiredInstanceId);
                    Debug.Log($"[IP] GET {id} (Экземпляр: {requiredInstanceId}");
                }

                if (selectedObj == null)
                {
                    selectedObj = list[0];
                    Debug.Log($"[IP] GET RANDOM {id}");
                }

                list.Remove(selectedObj); 
                
                return selectedObj.gameObject;
            }
            
            var data = database.GetItem(id);
            var newObj = Instantiate(data.WorldPrefab);
            
            var netItem = newObj.GetComponent<NetworkItem>();
            netItem.itemId = id;
            
            if (string.IsNullOrEmpty(netItem.instanceId))
            {
                netItem.instanceId = System.Guid.NewGuid().ToString();
            }
            Debug.Log($"[IP] NEW {netItem.itemId} (Экземпляр: {netItem.instanceId}");
            return newObj;
        }
    }
}