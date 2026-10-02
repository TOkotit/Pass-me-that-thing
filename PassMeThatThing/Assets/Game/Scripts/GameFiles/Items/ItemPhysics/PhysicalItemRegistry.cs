using System.Collections.Generic;
using System.Linq;
using Entity;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class PhysicalItemEntry
    {
        public PhysicalItem Item;
        public Rigidbody Body;

        public PhysicalItemEntry(PhysicalItem item, Rigidbody body)
        {
            Item = item;
            Body = body;
        }
    }
    public class PhysicalItemRegistry
    {
        public static PhysicalItemRegistry Instance { get; private set; }
        private Dictionary<GameObject, PhysicalItemEntry> _physicalItems = new Dictionary<GameObject, PhysicalItemEntry>();
        public List<PhysicalItem> GetItems()
            => _physicalItems.Values.Select(e => e.Item).Distinct().ToList();
        public List<PhysicalItemEntry> GetItemsEntries() => _physicalItems.Values.ToList();

        public PhysicalItemRegistry()
        {
            Instance = this;
        }
        public void Register(PhysicalItem item)
        {
            if (!item) return;
            Register(item, item.Rigidbody);
        }
        public void Register(PhysicalItem item, Rigidbody body)
        {
            if (!item || !body)
            {
                Debug.LogError($"[Registry] Register: item={(item ? item.name : "NULL")}, " +
                               $"body={(body ? body.name : "NULL")}");
                return;
            }
            var itemObject = body.gameObject;
            if (!_physicalItems.ContainsKey(itemObject))
                _physicalItems.Add(itemObject, new PhysicalItemEntry(item,body)); 
            Debug.Log($"{item.gameObject.name} has been registered");
        }
        
        public void Unregister(PhysicalItem item)
        {
            if (!item) return;

            var keysToRemove = new List<GameObject>();
            foreach (var kvp in _physicalItems)
                if (kvp.Value.Item == item)
                    keysToRemove.Add(kvp.Key);

            foreach (var key in keysToRemove)
                _physicalItems.Remove(key);
        }

        public PhysicalItem GetItem(GameObject itemObject)
        {
            if (_physicalItems.ContainsKey(itemObject))
            {
                return _physicalItems[itemObject].Item;
            }
            return null;
        }
        public bool TryGetItem(GameObject itemObject, out PhysicalItem item)
        {
            if (_physicalItems.ContainsKey(itemObject))
            {
                item = _physicalItems[itemObject].Item;
                return item;
            }
            item = null;
            return item;
        }
        public PhysicalItemEntry GetItemEntry(GameObject itemObject)
        {
            if (_physicalItems.ContainsKey(itemObject))
            {
                return _physicalItems[itemObject];
            }
            return null;
        }
        public bool TryGetItemEntry(GameObject itemObject, out PhysicalItemEntry item)
        {
            if (_physicalItems.ContainsKey(itemObject))
            {
                item = _physicalItems[itemObject];
                return item != null;
            }
            item = null;
            return item != null;
        }
    }
}