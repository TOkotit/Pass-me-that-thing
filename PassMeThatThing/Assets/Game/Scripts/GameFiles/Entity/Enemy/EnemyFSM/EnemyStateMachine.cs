namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class EnemyStateMachine
    {
        private EnemyState _currentState;
        
        public EnemyState CurrentState => _currentState;

        public void Initialize(EnemyState startingState)
        {
            _currentState = startingState;
            _currentState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            _currentState.Exit();
            _currentState = newState;
            _currentState.Enter();
        }
    }
}