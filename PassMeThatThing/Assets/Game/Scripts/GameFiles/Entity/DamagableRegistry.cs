using System.Collections.Generic;
using Entity.Entity;
using UnityEngine;

namespace Entity
{
    public class DamagableRegistry
    {
        public static DamagableRegistry Instance { get; private set; }
        private Dictionary<GameObject, Damagable> _damagableObjects = new Dictionary<GameObject, Damagable>();
        public DamagableRegistry()
        {
            Instance = this;
        }
        
        public void Register(Damagable damagable)
        {
            var damagableObject = damagable.gameObject;
            if (!_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Add(damagableObject, damagable); 
            Debug.Log($"Damagable: {damagable.gameObject.name} has been registered");
        }
        public void Register(GameObject gameObject,Damagable damagable)
        {
            if (!_damagableObjects.ContainsKey(gameObject))
                _damagableObjects.Add(gameObject, damagable); 
            Debug.Log($"Damagable: {damagable.gameObject.name} has been registered");
        }
        
        public void Unregister(Damagable damagable)
        {
            var damagableObject = damagable.gameObject;
            if (_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Remove(damagableObject);
            
        }

        public Damagable TryGetDamagable(GameObject damagable)
        {
            if (_damagableObjects.ContainsKey(damagable))
            {
                return _damagableObjects[damagable];
            }
            return null;
        }
    }
}