using System;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Entity;
using Enums;
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

        [SerializeField] private Transform attackCubeCenter;
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;

        public Transform AttackCubeCenter => attackCubeCenter;

        public event Action OnAttackMelee;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            _targetLayer = LayerMask.GetMask("Player", "BunkerDoor", "Building");
            
            if (attackCubeCenter == null) 
                attackCubeCenter = transform;
        }

        [Server]
        public void AttackMelee(Vector3 halfExtents, float damage)
        {

            var size = Physics.OverlapBox(
                attackCubeCenter.position,
                halfExtents,
                transform.rotation, 
                _targetLayer
            );

            foreach (var col in size)
            {
                DamagableRegistry.Instance.TryGetDamagable(col.gameObject, out var dam);
                _damageSystem.TakeDamage(damage, dam, damageTypes);
            }

            OnAttackMelee?.Invoke();
            
            Debug.Log("Zombie AttackMelee");
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.softRed;
            Gizmos.DrawCube(attackCubeCenter.position, new Vector3(0.2f, 0.2f, 0.2f));
        }
    }
}