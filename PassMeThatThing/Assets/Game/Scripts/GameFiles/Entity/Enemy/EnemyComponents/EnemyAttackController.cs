using DG.Tweening;
using Entity;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyAttackController : NetworkBehaviour
    {
        private LayerMask _targetLayer;
        
        [SerializeField] protected Transform attackCubeCenter;

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            _targetLayer = LayerMask.GetMask("Player", "BunkerDoor");
            
            if (attackCubeCenter == null) 
                attackCubeCenter = transform;
        }

        [Server]
        public void AttackMelee(Transform target, Vector3 halfExtents)
        {
            // заглушка
            gameObject.transform.DOScale(1.5f, 0.1f).From(1f).SetLoops(2, LoopType.Yoyo);
            
            var colliders = new Collider[15];
            var size = Physics.OverlapBoxNonAlloc(
                attackCubeCenter.position,
                halfExtents,
                colliders, 
                transform.rotation, 
                _targetLayer
            );
    
            DamagableModel playerModel = null;
            bool hitPlayer = false;
            foreach (var col in colliders)
            {
                if (!col.CompareTag("Player")) continue;
                // playerModel = _registry.TryGetCharacter(col.gameObject);
                // if (playerModel != null && playerModel.Team != Teams.Enemy)
                // {
                //     hitPlayer = true;
                //     break;
                // }
            }

            if (hitPlayer)
            {
                // playerModel.Health.TakeDamage(closeAttackData.Damage, closeAttackData.DamageType);
            }
        }
    }
}