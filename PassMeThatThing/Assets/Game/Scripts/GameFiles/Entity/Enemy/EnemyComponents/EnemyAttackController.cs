using System;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Entity;
using Enums;
using FishNet.Object;
using Game.Scripts.Systems;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    
    public class EnemyAttackController : NetworkBehaviour
    {
        private LayerMask _targetLayer;

        [Inject] private DamageSystem _damageSystem;
        
        [SerializeField] protected Transform attackCubeCenter;
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;
        
        public event Action OnAttackMelee;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            _targetLayer = LayerMask.GetMask("Player", "BunkerDoor");
            
            if (attackCubeCenter == null) 
                attackCubeCenter = transform;
        }

        [Server]
        public void AttackMelee(Vector3 halfExtents, float damage)
        {
            if (DamagableRegistry.Instance == null) return;

            var size = Physics.OverlapBox(
                attackCubeCenter.position,
                halfExtents,
                // colliders, 
                transform.rotation, 
                _targetLayer
            );

            foreach (var col in size)
            {
                _damageSystem.TakeDamage(damage, col.gameObject, damageTypes);
            }

            OnAttackMelee?.Invoke();
        }
        
        private Damagable FindDamagableInHierarchy(GameObject obj)
        {
            var t = obj.transform;
            while (t)
            {
                if (DamagableRegistry.Instance.TryGetDamagable(t.gameObject, out var damagable))
                    return damagable;
                t = t.parent;
            }
            return null;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.softRed;
            Gizmos.DrawCube(attackCubeCenter.position, new Vector3(0.2f, 0.2f, 0.2f));
        }
    }
}