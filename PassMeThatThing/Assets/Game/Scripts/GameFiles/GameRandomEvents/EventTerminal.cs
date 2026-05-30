using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class EventTerminal : NetworkBehaviour
    {
        protected EventTerminalsRegistry Registry { get; private set; }

        public bool IsTerminalBusy
        {
            get => _isTerminalBusy;
            set => _isTerminalBusy = value;
        }

        private bool _isTerminalBusy;


        [Server]
        public virtual void TerminalAct(NetworkConnectionToClient conn)
        {

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
        
        [Server]
        public void ActivateMinigame(NetworkConnectionToClient senderConnection, BaseGameEvent gameEvent)
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
                Debug.Log($"[SERVER] send to {senderConnection.connectionId}");
                playerHandler.TargetOpenMinigame(parameters);
            }
        }
    }
}