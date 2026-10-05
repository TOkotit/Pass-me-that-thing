
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Mirror;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks
{
    public class GameTaskHandler : NetworkBehaviour
    {
        [Inject] private GameTasksManager _gameTasksManager;

        [SyncVar]
        private int _handlerId;

        public GameTasksManager GameTasksManager => _gameTasksManager;

        [Server]
        public GameTask CreateGameTask(GameTaskParameters gameTaskParameters)
        {

            var t = _gameTasksManager.CreateGameTask(gameTaskParameters);
            return t;
        }

        [Server]
        public void OverdueGameTask(int taskId)
        {
            _gameTasksManager.OverdueGameTask(taskId);
        }

        [Server]
        public void CompleteAndDestroyGameTask(int taskId)
        {
            _gameTasksManager.CompleteAndDestroyGameTask(taskId);
        }

        [Server]
        public void DestroyGameTask(int taskId)
        {
            _gameTasksManager.DestroyGameTask(taskId);
        }




        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            RegisterTaskHandler();
        }

        [Server]
        public override void OnStopServer()
        {
            base.OnStartServer();
            UnRegisterTaskHandler();
        }


        private void RegisterTaskHandler()
        {
            if (_gameTasksManager != null)
            {
                _handlerId = _gameTasksManager.RegisterSceneTaskHandler(this);
            }
        }

        private void UnRegisterTaskHandler()
        {
            if (_gameTasksManager != null)
            {
                _gameTasksManager.UnregisterTaskHandler(_handlerId);
            }
        }
    }
}
