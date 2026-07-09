using System;
using System.Linq;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class SpiderView : EnemyView
    {
        [SerializeField] protected LayerMask groundMask;
        
        private const string WalkKey = "Walk";
        private const string Attack1Key = "Attack1";
        private const string Attack2Key = "Attack2";
        private const string DeathKey = "Death";

        public void Walk() => base.animator.SetTrigger(WalkKey);
        public void Attack1() => base.animator.SetTrigger(Attack1Key);
        public void Attack2() => base.animator.SetTrigger(Attack2Key);
        public void Death() => base.animator.SetTrigger(DeathKey);
        
        
        private const string IdleClipName = "";

       
    }
}