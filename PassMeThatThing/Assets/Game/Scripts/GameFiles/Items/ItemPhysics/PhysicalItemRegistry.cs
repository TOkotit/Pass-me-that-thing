using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class PhysicalItemRegistry
    {
        private Dictionary<GameObject, PhysicalItem> _physicalItems = new Dictionary<GameObject, PhysicalItem>();
        
        public void Register(PhysicalItem item)
        {
            var itemObject = item.gameObject;
            if (!_physicalItems.ContainsKey(itemObject))
                _physicalItems.Add(itemObject, item); 
            Debug.Log($"{item.gameObject.name} has been registered");
        }
        
        
        public void Unregister(PhysicalItem item)
        {
            var itemObject = item.gameObject;
            if (_physicalItems.ContainsKey(itemObject))
                _physicalItems.Remove(itemObject);
            
        }

        public PhysicalItem TryGetCharacter(GameObject item)
        {
            if (_physicalItems.ContainsKey(item))
            {
                return _physicalItems[item];
            }
            return null;
        }
    }
}