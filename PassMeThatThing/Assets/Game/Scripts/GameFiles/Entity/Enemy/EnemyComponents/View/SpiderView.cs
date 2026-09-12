using System;
using System.Linq;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class SpiderView : EnemyView
    {
        //[SerializeField] protected LayerMask groundMask;
        
        private const string WalkKey = "isWalking";
        private const string GroupUpKey = "GroupUp";
        private const string UnGroupKey = "UnGroup";
        private const string JumpAttackKey = "JumpAttack";

        public void SetWalk(bool value) => animator.SetBool(WalkKey, value);
        public void GroupUp() => netAnimator.SetTrigger(GroupUpKey);
        public void UnGroup() => netAnimator.SetTrigger(UnGroupKey);
        public void JumpAttack() => netAnimator.SetTrigger(JumpAttackKey);
        
        
        //private const string IdleClipName = "";

        
    }
}