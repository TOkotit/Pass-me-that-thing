using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class EventTerminal : NetworkBehaviour
    {
        protected NetworkConnectionToClient currentClient;
        private EventTerminalsRegistry _registry;

        [SyncVar] 
        private bool _isTerminalBusy;

        [SyncVar(hook = nameof(OnFixedChanged))]
        private bool _isFixed = true;


        public bool IsFixed { get => _isFixed; set => _isFixed = value; }

        public bool IsTerminalBusy { get => _isTerminalBusy; set => _isTerminalBusy = value; }
        

        public override void OnStartClient()
        {
            base.OnStartClient();
            _registry = EventTerminalsRegistry.Instance;
            _registry.Register(this); 
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _registry.Unregister(this); 
        } 

        public virtual void OnFixedChanged(bool oldValue, bool newValue) { }
        
        [Command(requiresAuthority = false)]
        public virtual void CmdMinigameClose() { }

        [Command(requiresAuthority = false)]
        public virtual void CmdMinigameComplete() { }
        
        [Server]
        public virtual void TerminalAct(NetworkConnectionToClient conn) { }
        
        
        
        [Server]
        public bool ActivateMinigame(NetworkConnectionToClient senderConnection, BaseGameEvent gameEvent)
        {
            var parameters = new MinigameParameters
            {
                eventId = gameEvent.EventId,
                eventType = gameEvent.EventType,
                description = gameEvent.description,
                difficulty = gameEvent.difficulty,
                timeLimit = gameEvent.timeLimit,
                
                eventTerminal = this
            };
            
            if (senderConnection.identity.TryGetComponent<PlayerMinigameHandler>(out var playerHandler))
            {
                if (playerHandler.IsClientBusy)
                {
                    Debug.Log($"[EVENT] client is busy {senderConnection.connectionId}");
                    return false;
                }
                
                Debug.Log($"[EVENT] send to {senderConnection.connectionId}");
                playerHandler.TargetOpenMinigame(parameters);
                
                return true;
            }
            
            return false;
        }
        
        [Server]
        public void CloseMinigame(NetworkConnectionToClient senderConnection)
        {
            if (senderConnection.identity.TryGetComponent<PlayerMinigameHandler>(out var playerHandler))
            {
                Debug.Log($"[EVENT] closed to {senderConnection.connectionId}");
                playerHandler.TargetCloseMinigame();
            }
        }
    }
}