using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class DamagableRegistry
    {
        private Dictionary<GameObject, DamagableModel> _damagableObjects = new Dictionary<GameObject, DamagableModel>();
        
        public void Register(Character character)
        {
            var damagable = character.gameObject;
            if (!_damagableObjects.ContainsKey(damagable))
                _damagableObjects.Add(damagable, character.Damagable); 
            Debug.Log($"{character.gameObject.name} has been registered");
        }
        
        
        public void Unregister(Character character)
        {
            var damagable = character.gameObject;
            if (_damagableObjects.ContainsKey(damagable))
                _damagableObjects.Remove(damagable);
            
        }

        public DamagableModel TryGetCharacter(GameObject character)
        {
            if (_damagableObjects.ContainsKey(character))
            {
                return _damagableObjects[character];
            }
            return null;
        }
    }
}