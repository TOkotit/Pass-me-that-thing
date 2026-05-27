using DG.Tweening;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyAttackController : NetworkBehaviour
    {
        [Server]
        public void Attack(Transform target)
        {
            // заглушка
            gameObject.transform.DOScale(1.5f, 0.5f).From(1f).SetLoops(1, LoopType.Yoyo);
        }
    }
}