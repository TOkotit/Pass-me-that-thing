using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class EventTerminal : NetworkBehaviour
    {
        protected NetworkConnection currentClient;
        
        // [SyncVar] 
        private readonly SyncVar<bool> _isTerminalBusy = new();
        private EventTerminalsRegistry _registry;
        
        public bool IsTerminalBusy
        {
            get => _isTerminalBusy.Value;
            set => _isTerminalBusy.Value = value;
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
            if (Registry != null)
            {
                Registry.Unregister(this); 
            }
        } 
        
        [ServerRpc]
        public virtual void CmdMinigameClose() { }
        
        [ServerRpc]
        public virtual void CmdMinigameComplete() { }
        
        [Server]
        public virtual void TerminalAct(NetworkConnection conn) { }
        
        [Server]
        public bool ActivateMinigame(NetworkConnection senderConnection, BaseGameEvent gameEvent)
        {
            var parameters = new MinigameParameters
            {
                eventId = gameEvent.EventId,
                eventType = gameEvent.eventType.Value,
                description = gameEvent.description,
                difficulty = gameEvent.difficulty,
                timeLimit = gameEvent.timeLimit,
                
                eventTerminal = this
            };
            
            if (senderConnection.FirstObject != null 
                && senderConnection.FirstObject.TryGetComponent<PlayerMinigameHandler>(out var playerHandler))
            {
                if (playerHandler.IsClientBusy)
                {
                    Debug.Log($"[SERVER] client is busy {senderConnection.ClientId}");
                    return false;
                }
                
                Debug.Log($"[SERVER] send to {senderConnection.ClientId}");
                playerHandler.TargetOpenMinigame(senderConnection, parameters);
                
                return true;
            }
            
            return false;
        }
        
        [Server]
        public void CloseMinigame(NetworkConnection senderConnection)
        {
            if (senderConnection.FirstObject != null && senderConnection.FirstObject.TryGetComponent<PlayerMinigameHandler>(out var playerHandler))
            {
                Debug.Log($"[SERVER] closed to {senderConnection.ClientId}");
                playerHandler.TargetCloseMinigame(senderConnection);
            }
        }
    }
}