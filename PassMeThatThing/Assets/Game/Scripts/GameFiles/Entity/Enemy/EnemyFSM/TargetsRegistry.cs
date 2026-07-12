using System;
using System.Collections.Generic;
using Entity;
using Enums;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using UnityEngine;

namespace Game.Entity
{
    /// <summary>
    /// Цели для мозгов нпс (враги, турели)
    /// </summary>
    public class TargetsRegistry
    {
        public static TargetsRegistry Instance { get; private set; }
        private Dictionary<DamagableType, Dictionary<GameObject, TargetObject>> _enemyTargetObjects = new();

        public Dictionary<DamagableType, Dictionary<GameObject, TargetObject>> EnemyTargetObjects => _enemyTargetObjects;
        
        public TargetsRegistry()
        {
            Instance = this;
            foreach (DamagableType type in Enum.GetValues(typeof(DamagableType)))
            {
                _enemyTargetObjects[type] = new();
            }
        }
        
        public void Register(TargetObject target)
        {
            var gameObject = target.gameObject;
            if (!_enemyTargetObjects[target.DamagableType].ContainsKey(gameObject))
                _enemyTargetObjects[target.DamagableType].Add(gameObject, target); 
            Debug.Log($"[EnemyTargetObject] {target.gameObject.name} has been registered");
        }
        
        
        public void Unregister(TargetObject target)
        {
            var gameObject = target.gameObject;
            if (_enemyTargetObjects[target.DamagableType].ContainsKey(gameObject))
                _enemyTargetObjects[target.DamagableType].Remove(gameObject);
        }

        public TargetObject TryGetTarget(GameObject gameObject, DamagableType type, out TargetObject target)
        {
            if (_enemyTargetObjects[type].ContainsKey(gameObject))
            {
                target = _enemyTargetObjects[type][gameObject];
                return target;
            }
            target = null;
            return target;
        }
    }
}