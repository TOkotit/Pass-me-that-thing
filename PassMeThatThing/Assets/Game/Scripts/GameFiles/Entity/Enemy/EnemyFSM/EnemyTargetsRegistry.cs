using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Game.Entity
{
    public class EnemyTargetsRegistry
    {
        public static EnemyTargetsRegistry Instance { get; private set; }
        private Dictionary<GameObject, Damagable> _damagableObjects = new Dictionary<GameObject, Damagable>();

        public Dictionary<GameObject, Damagable> DamagableObjects => _damagableObjects;
        
        public EnemyTargetsRegistry()
        {
            Instance = this;
        }
        
        public void Register(Damagable damagable)
        {
            var damagableObject = damagable.gameObject;
            if (!_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Add(damagableObject, damagable); 
            Debug.Log($"[EnemyTarget] {damagable.gameObject.name} has been registered");
        }
        
        public void Register(GameObject gameObject, Damagable damagable)
        {
            if (!_damagableObjects.ContainsKey(gameObject))
                _damagableObjects.Add(gameObject, damagable); 
            Debug.Log($"[EnemyTarget] {damagable.gameObject.name} has been registered");
        }
        
        public void Unregister(Damagable damagable)
        {
            var damagableObject = damagable.gameObject;
            if (_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Remove(damagableObject);
            
        }

        public Damagable TryGetTargetDamagable(GameObject damagable, out Damagable item)
        {
            if (_damagableObjects.ContainsKey(damagable))
            {
                item = _damagableObjects[damagable];
                return item;
            }
            item = null;
            return item;
        }
    }
}