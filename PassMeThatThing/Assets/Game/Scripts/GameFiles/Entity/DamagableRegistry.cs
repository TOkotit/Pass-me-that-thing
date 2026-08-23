using System.Collections.Generic;
using System.Linq;
using Entity;
using UnityEngine;

namespace Entity
{
    public class DamagableRegistry
    {
        public static DamagableRegistry Instance { get; private set; }
        private Dictionary<GameObject, Damageable> _damagableObjects = new Dictionary<GameObject, Damageable>();
        public List<Damageable> GetDamageables() => _damagableObjects.Values.ToList();
        public DamagableRegistry()
        {
            Instance = this;
        }
        
        public void Register(Damageable damageable)
        {
            var damagableObject = damageable.gameObject;
            if (!_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Add(damagableObject, damageable); 
            Debug.Log($"Damageable: {damageable.gameObject.name} has been registered");
        }
        public void Register(GameObject gameObject,Damageable damageable)
        {
            if (!_damagableObjects.ContainsKey(gameObject))
                _damagableObjects.Add(gameObject, damageable); 
            Debug.Log($"Damageable: {damageable.gameObject.name} has been registered");
        }
        
        public void Unregister(Damageable damageable)
        {
            var damagableObject = damageable.gameObject;
            if (_damagableObjects.ContainsKey(damagableObject))
                _damagableObjects.Remove(damagableObject);
            
        }

        public Damageable TryGetDamagable(GameObject damagable, out Damageable item)
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