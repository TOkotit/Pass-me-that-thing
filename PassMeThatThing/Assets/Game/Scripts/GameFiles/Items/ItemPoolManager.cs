using VContainer;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemPoolManager : MonoBehaviour
    {
        [SerializeField] private ItemDatabase database;
        
        private Dictionary<string, Stack<GameObject>> _poolDict = new ();

        public void Start()
        {
            InitializePool();
        }
        
        public void InitializePool()
        {
            foreach (var item in database.allItems)
            {
                _poolDict[item.Id] = new Stack<GameObject>();

                // Регистрируем префаб в Mirror
                NetworkClient.RegisterPrefab(item.WorldPrefab, 
                    (msg) => SpawnHandler(msg, item.Id), 
                    UnspawnHandler);
            }
        }

        // Обработчик появления
        public GameObject SpawnHandler(SpawnMessage msg, string itemId)
        {
            var obj = GetFromPool(itemId);
            obj.transform.position = msg.position;
            obj.transform.rotation = msg.rotation;
            obj.SetActive(true);
            return obj;
        }

        // Обработчик исчезновения
        public void UnspawnHandler(GameObject spawned)
        {
            var id = spawned.GetComponent<NetworkItem>().itemId;
            spawned.SetActive(false);
            _poolDict[id].Push(spawned);
        }
        
        public GameObject GetFromPool(string id)
        {
            if (_poolDict.ContainsKey(id) && _poolDict[id].Count > 0)
            {
                return _poolDict[id].Pop();
            }
            
            var data = database.GetItem(id);
            var obj = Instantiate(data.WorldPrefab);
            obj.GetComponent<NetworkItem>().itemId = id;
            return obj;
        }
    }
}