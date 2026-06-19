using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class ZombieView : EnemyView
    {
        private const string WalkKey = "Walk";
        private const string Attack1Key = "Attack1";
        private const string Attack2Key = "Attack2";
        private const string DeathKey = "Death";

        public void Walk() => animator.SetTrigger(WalkKey);
        public void Attack1() => animator.SetTrigger(Attack1Key);
        public void Attack2() => animator.SetTrigger(Attack2Key);
        public void Death() => animator.SetTrigger(DeathKey);
    }
}