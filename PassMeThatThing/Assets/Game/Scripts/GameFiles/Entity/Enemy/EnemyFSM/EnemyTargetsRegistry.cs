using System.Collections.Generic;
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using UnityEngine;

namespace Game.Entity
{
    public class EnemyTargetsRegistry
    {
        public static EnemyTargetsRegistry Instance { get; private set; }
        private Dictionary<GameObject, EnemyTargetObject> _enemyTargetObjects = new Dictionary<GameObject, EnemyTargetObject>();

        public Dictionary<GameObject, EnemyTargetObject> EnemyTargetObjects => _enemyTargetObjects;
        
        public EnemyTargetsRegistry()
        {
            Instance = this;
        }
        
        public void Register(EnemyTargetObject damagable)
        {
            var damagableObject = damagable.gameObject;
            if (!_enemyTargetObjects.ContainsKey(damagableObject))
                _enemyTargetObjects.Add(damagableObject, damagable); 
            Debug.Log($"[EnemyTargetObject] {damagable.gameObject.name} has been registered");
        }
        
        public void Register(GameObject gameObject, EnemyTargetObject damagable)
        {
            if (!_enemyTargetObjects.ContainsKey(gameObject))
                _enemyTargetObjects.Add(gameObject, damagable); 
            Debug.Log($"[EnemyTargetObject] {damagable.gameObject.name} has been registered");
        }
        
        public void Unregister(EnemyTargetObject damagable)
        {
            var damagableObject = damagable.gameObject;
            if (_enemyTargetObjects.ContainsKey(damagableObject))
                _enemyTargetObjects.Remove(damagableObject);
            
        }

        public EnemyTargetObject TryGetTargetDamagable(GameObject damagable, out EnemyTargetObject item)
        {
            if (_enemyTargetObjects.ContainsKey(damagable))
            {
                item = _enemyTargetObjects[damagable];
                return item;
            }
            item = null;
            return item;
        }
    }
}