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
            
            _targetLayer = LayerMask.GetMask("ServerCollider", "BunkerDoor");
            
            if (attackCubeCenter == null) 
                attackCubeCenter = transform;
        }

        [Server]
        public void AttackMelee(Vector3 halfExtents, float damage)
        {
            // заглушка
            gameObject.transform.DOScale(1.5f, 0.1f).From(1f).SetLoops(2, LoopType.Yoyo);
            
            var colliders = new Collider[100];
            var size = Physics.OverlapBoxNonAlloc(
                attackCubeCenter.position,
                halfExtents,
                colliders, 
                transform.rotation, 
                _targetLayer
            );
    
            // DamagableModel playerModel = null;
            // bool hitPlayer = false;
            // for (var i = 0; i < size; i++)
            // {
            //     if (!colliders[i].CompareTag("Player")) continue;
            //     // playerModel = _registry.TryGetCharacter(col.gameObject);
            //     // if (playerModel != null && playerModel.Team != Teams.Enemy)
            //     // {
            //     //     hitPlayer = true;
            //     //     break;
            //     // }
            // }
            //
            // if (hitPlayer)
            // {
            //     // playerModel.Health_old.TakeDamage(closeAttackData.Damage, closeAttackData.DamageType);
            // }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.softRed;
            Gizmos.DrawCube(attackCubeCenter.position, new Vector3(1f, 1f, 1f));
        }
    }
}