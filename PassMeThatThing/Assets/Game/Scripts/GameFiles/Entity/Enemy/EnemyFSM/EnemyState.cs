using System;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public abstract class EnemyState
    {
        protected EnemyStateMachine StateMachine;
        protected Enemy Enemy;

        public virtual bool AbleToInterupt => true;
        
        public event Action OnEnter;
        public event Action OnExit;
        
        public EnemyState(Enemy enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            OnEnter?.Invoke();
            // Debug.Log($"Enter {EnemyStateType}");
        }
        
        /// <summary>
        /// Вызывается раз в SMInterval
        /// </summary>
        public virtual void LogicUpdate() { }
        
        /// <summary>
        /// Вызывается каждый FixedUpdate
        /// </summary>
        public virtual void PhysicsUpdate() { }

        public virtual void Exit()
        {
            OnExit?.Invoke();
        }
    }
}