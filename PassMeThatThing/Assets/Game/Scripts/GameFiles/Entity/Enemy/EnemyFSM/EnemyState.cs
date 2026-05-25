using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public abstract class EnemyState
    {
        protected abstract EnemyStates EnemyStateType { get; }
        protected EnemyStateMachine StateMachine;
        protected Enemy Enemy;
        
        public EnemyState(Enemy enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
        public virtual void Exit() { }
    }
}