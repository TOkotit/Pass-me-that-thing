using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class EventTerminal : NetworkBehaviour
    {
        protected NetworkConnectionToClient currentClient;
        
        [SyncVar] private bool _isTerminalBusy;
        private EventTerminalsRegistry _registry;
        
        public bool IsTerminalBusy
        {
            get => _isTerminalBusy;
            set => _isTerminalBusy = value;
        }
        
        protected EventTerminalsRegistry Registry
        {
            get => _registry;
            private set => _registry = value;
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            Registry = EventTerminalsRegistry.Instance;
            Registry.Register(this); 
        }

        protected virtual void OnDestroy()
        {
            Registry.Unregister(this); 
        } 
        
        [Command]
        public virtual void CmdMinigameClose() { }
        
        [Command]
        public virtual void CmdMinigameComplete() { }
        
        [Server]
        public virtual void TerminalAct(NetworkConnectionToClient conn) { }
        
        
        
        [Server]
        public bool ActivateMinigame(NetworkConnectionToClient senderConnection, BaseGameEvent gameEvent)
        {
            var parameters = new MinigameParameters
            {
                eventId = gameEvent.EventId,
                eventType = gameEvent.eventType,
                description = gameEvent.description,
                difficulty = gameEvent.difficulty,
                timeLimit = gameEvent.timeLimit,
                
                eventTerminal = this
            };
            
            if (senderConnection.identity.TryGetComponent<PlayerMinigameHandler>(out var playerHandler))
            {
                if (playerHandler.IsClientBusy)
                {
                    Debug.Log($"[SERVER] client is busy {senderConnection.connectionId}");
                    return false;
                }
                
                Debug.Log($"[SERVER] send to {senderConnection.connectionId}");
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
                Debug.Log($"[SERVER] closed to {senderConnection.connectionId}");
                playerHandler.TargetCloseMinigame();
            }
        }
    }
}