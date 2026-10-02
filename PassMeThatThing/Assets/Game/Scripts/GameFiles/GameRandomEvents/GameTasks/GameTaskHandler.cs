
using Game.Scripts.GameFiles.GameRandomEvents;
using Mirror;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks
{
    public class GameTaskHandler : NetworkBehaviour
    {
        [Inject] private GameRandomEventManager _gameRandomEventManager;

        [SyncVar]
        private int _handlerId;

        [Server]
        public GameTask CreateGameTask(GameTaskParameters gameTaskParameters)
        {
            //var testPars = new GameTaskParameters();
            //testPars.gameTaskType = GameTaskType.GameEvent;
            //testPars.timeLimit = 6;
            //testPars.taskField = GameEventsType.BlackoutCutWires.ToString();
            //testPars.cost = 100;

            var t = _gameRandomEventManager.CreateGameTask(gameTaskParameters);
            return t;
        }

        [Server]
        public void OverdueGameTask(int taskId)
        {
            _gameRandomEventManager.OverdueGameTask(taskId);
        }

        [Server]
        public void CompleteAndDestroyGameTask(int taskId)
        {
            _gameRandomEventManager.CompleteAndDestroyGameTask(taskId);
        }

        [Server]
        public void DestroyGameTask(int taskId)
        {
            _gameRandomEventManager.DestroyGameTask(taskId);
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
            if (_gameRandomEventManager != null)
            {
                _handlerId = _gameRandomEventManager.RegisterSceneTaskHandler(this);
            }
        }

        private void UnRegisterTaskHandler()
        {
            if (_gameRandomEventManager != null)
            {
                _gameRandomEventManager.UnregisterTaskHandler(_handlerId);
            }
        }
    }
}
