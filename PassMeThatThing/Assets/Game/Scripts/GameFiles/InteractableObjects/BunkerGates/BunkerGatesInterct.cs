using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

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
        
        // [SyncVar(OnChange = nameof(OnOpenStateChanged))]
        private readonly SyncVar<bool> isOpen = new();
        
        private float targetY;
        private bool targetYInitialized;


        private void Awake()
        {
            isOpen.OnChange += OnOpenStateChanged;
        }

        private void OnDestroy()
        {
            isOpen.OnChange -= OnOpenStateChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            isOpen.Value = false;
            targetY = closedY;
            targetYInitialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            targetY = isOpen.Value ? openY : closedY;
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

        [ServerRpc(RequireOwnership = false)] 
        private void CmdToggleGate() => isOpen.Value = !isOpen.Value;
        
        [Server]
        public override void SrbToggle() => isOpen.Value = !isOpen.Value;

        
        [Server]
        public void Open() => isOpen.Value = true;
        
        [Server]
        public void Close() => isOpen.Value = false;
        
        
        private void OnOpenStateChanged(bool oldValue, bool newValue, bool asServer)
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