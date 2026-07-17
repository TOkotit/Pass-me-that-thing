using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemPoolManager : NetworkBehaviour
    {
        [SerializeField] private ItemDatabase database;
        
        private Dictionary<string, NetworkItem> _poolDict = new ();

        public void Start()
        {
            InitializePool();
        }

        
        public void InitializePool()
        {
            foreach (var item in database.allItems)
            {
                NetworkClient.RegisterPrefab(item.WorldPrefab);
                
                if (isServer)
                    _poolDict = new Dictionary<string, NetworkItem>();
            }
        }
        
        [Server]
        public GameObject CreateNewObject(string id, Vector3 position = new())
        {
            var data = database.GetItem(id);
            var newObj = Instantiate(data.WorldPrefab, position, Quaternion.identity);
            
            var netItem = newObj.GetComponent<NetworkItem>();
            
            netItem.itemId = id;
            netItem.instanceId = Guid.NewGuid().ToString();
            
            NetworkServer.Spawn(newObj);
            
            Debug.Log($"[IP] CreateNewObject {netItem.itemId} instanceId: {netItem.instanceId}");
            return newObj;
        }
        
        [Server]
        public GameObject GetFromPool(string requiredInstanceId)
        {
            if (!string.IsNullOrEmpty(requiredInstanceId)
                && _poolDict.Remove(requiredInstanceId, out var selectedObj))
            {
                RpcActivateItem(selectedObj);
                
                Debug.Log($"[IP] GetFromPool {selectedObj.itemId} instanceId: {selectedObj.instanceId}");
                return selectedObj.gameObject;
            }

            return null;
        }

        [Server]
        public void ReturnToPool(NetworkItem networkItem)
        {
            _poolDict.TryAdd(networkItem.instanceId, networkItem);
            
            RpcDeactivateItem(networkItem);
            
            Debug.Log($"[IP] ReturnToPool {networkItem.itemId} instanceId: {networkItem.instanceId}");
        }

        [ClientRpc]
        public void RpcDeactivateItem(NetworkItem item)
        {
            item.gameObject.SetActive(false);
        }
        
        [ClientRpc]
        public void RpcActivateItem(NetworkItem item)
        {
            item.gameObject.SetActive(true);
        }
        
    }
}