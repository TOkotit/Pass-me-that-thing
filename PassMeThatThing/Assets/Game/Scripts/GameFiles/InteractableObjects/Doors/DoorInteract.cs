using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.GameFiles.InteractableObjects.Doors
{
    public class DoorInteract : Interactable
    {
        [Header("Movement")]
        [SerializeField] private float closedYRotation = 0f;
        [SerializeField] private float openYRotation = 90f;
        [SerializeField] private float moveSpeed = 2f;

        // [SyncVar(OnChange = nameof(OnOpenStateChanged))]
        private readonly SyncVar<bool> isOpen = new();
        
        
        private float targetRotationY;
        private bool initialized;


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
            targetRotationY = closedYRotation;
            initialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            targetRotationY = isOpen.Value ? openYRotation : closedYRotation;
            initialized = true;
        }

        private void FixedUpdate()
        {
            if (!initialized)
                return;

            var rotation = transform.localEulerAngles;
            var newY = Mathf.MoveTowardsAngle(rotation.y, targetRotationY, moveSpeed * Time.fixedDeltaTime);

            if (Mathf.Approximately(rotation.y, newY)) return;

            rotation.y = newY;
            transform.localEulerAngles = rotation;
        }

        public override void Interact()
        {
            Debug.Log("Interact with door");
            CmdToggleDoor();
        }

        [ServerRpc(RequireOwnership = false)] 
        private void CmdToggleDoor() 
        {
            isOpen.Value = !isOpen.Value;
            UpdateTarget(isOpen.Value); 
        }
        
        private void UpdateTarget(bool open)
        {
            targetRotationY = open ? openYRotation : closedYRotation;
            initialized = true;
        }
        
        [Server]
        public override void SrbToggle() => isOpen.Value = !isOpen.Value;

        [Server]
        public void Open() => isOpen.Value = true;

        [Server]
        public void Close() => isOpen.Value = false;
        

        private void OnOpenStateChanged(bool oldValue, bool newValue, bool asServer)
        {
            UpdateTarget(newValue);
            
        }
        
        // Отладочный метод для того, чтобы смотреть работу без интеракции
        // [ServerCallback]
        // private void Update()
        // {
        //     if (Time.time % 5f < 0.02f)
        //     {
        //         SrvToggleDoor();
        //         Debug.Log($"[SERVER] Door state: {isOpen}");
        //     }
        // }
        
    }
}