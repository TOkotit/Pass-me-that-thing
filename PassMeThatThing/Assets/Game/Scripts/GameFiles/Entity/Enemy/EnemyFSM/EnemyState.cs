using System;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public abstract class EnemyState
    {
        protected abstract EnemyStates EnemyStateType { get; }
        protected EnemyStateMachine StateMachine;
        protected Enemy Enemy;
        
        public event Action<EnemyStates> OnEnter;
        public event Action<EnemyStates> OnExit;
        
        public EnemyState(Enemy enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            OnEnter?.Invoke(EnemyStateType);
            // Debug.Log($"Enter {EnemyStateType}");
        }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }

        public virtual void Exit()
        {
            OnExit?.Invoke(EnemyStateType);
        }
    }
}