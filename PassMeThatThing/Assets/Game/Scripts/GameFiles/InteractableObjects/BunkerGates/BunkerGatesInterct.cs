using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Scripts.GameFiles.InteractableObjects.BunkerGates
{
    public class BunkerGates : Interactable
    {
        [Header("Movement")]
        [SerializeField] private Transform gateVisual;
        [SerializeField] private float closedY = 0f;
        [SerializeField] private float openY = 4f;
        [SerializeField] private float moveSpeed = 2f;
        
        [SyncVar(hook = nameof(OnOpenStateChanged))]
        private bool isOpen;
        
        private float targetY;
        private bool targetYInitialized;

        public override void OnStartServer()
        {
            base.OnStartServer();
            isOpen = false;
            targetY = closedY;
            targetYInitialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            targetY = isOpen ? openY : closedY;
            targetYInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!targetYInitialized || !gateVisual)
                return;

            var position = gateVisual.localPosition;
            var newY = Mathf.MoveTowards(position.y, targetY, moveSpeed * Time.fixedDeltaTime);

            if (Mathf.Approximately(position.y, newY)) return;
            
            position.y = newY;
            gateVisual.localPosition = position;
        }

        public override void Interact()
        {
            CmdToggleGate();
        }

        [Command(requiresAuthority = false)] 
        private void CmdToggleGate() => isOpen = !isOpen;
        
        [ServerCallback]
        public override void SrbToggle() => isOpen = !isOpen;

        
        [Server]
        public void Open() => isOpen = true;
        
        [Server]
        public void Close() => isOpen = false;
        
        
        private void OnOpenStateChanged(bool oldValue, bool newValue)
        {
            targetY = newValue ? openY : closedY;
        }
        
        
        // Отладочный метод для того, чтобы смотреть работу без интеракции
        // [ServerCallback]
        // private void Update()
        // {
        //     if (Time.time % 5f < 0.02f)
        //     {
        //         SrbToggleGate();
        //         Debug.Log($"[SERVER] Gate state: {isOpen}");
        //     }
        // }
        
    }
}