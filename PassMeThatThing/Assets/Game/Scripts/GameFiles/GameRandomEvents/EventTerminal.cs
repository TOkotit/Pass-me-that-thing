using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class EventTerminal : NetworkBehaviour, Interactable
    {
        [SerializeField]
        protected Transform minigameContainer;

        protected NetworkConnectionToClient currentClient;
        private EventTerminalsRegistry _registry;

        [SerializeField] protected ItemData fixItem;

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

            InteractableRegistry.Instance.Register(gameObject, this);
            if (!InteractableRegistry.Instance.TryGetInteractable(gameObject, out Interactable interactable))
            {
                Debug.LogError("Interactable not found");
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();


            _registry.Unregister(this);

            InteractableRegistry.Instance.Unregister(gameObject);
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
                //description = gameEvent.description,
                //difficulty = gameEvent.difficulty,
                //timeLimit = gameEvent.TimeLimit,
                
                eventTerminal = this,
                position = minigameContainer 
                    == null ? transform.position : minigameContainer.transform.position,
                rotation = minigameContainer
                    == null ? transform.rotation : minigameContainer.transform.rotation,
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

        public virtual void Interact()
        {
            Debug.Log("[EVENT TERM] Interact");
        }

        public virtual void SrbToggle()
        {
            
        }

        [Command(requiresAuthority = false)]
        public virtual void InteractWithItem(PhysicalItem item)
        {
            Debug.Log("[EVENT TERM] InteractWithItem");

            if (item.Network.itemId == fixItem.Id)
            {
                TerminalAct(item.ConnectionToClient);
            }
        
        }
    }
}