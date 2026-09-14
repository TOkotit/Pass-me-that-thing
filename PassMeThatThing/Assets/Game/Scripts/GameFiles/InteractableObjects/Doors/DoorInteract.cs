using System.Collections;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.GameFiles.InteractableObjects.Doors
{
    public class DoorInteract : NetworkBehaviour, Interactable
    {
        [Header("Movement")]
        [SerializeField] private float closedYRotation = 0f;
        [SerializeField] private float openYRotation = 90f;
        [SerializeField] private float moveSpeed = 2f;
        [SyncVar(hook = nameof(OnOpenStateChanged))]
        private bool isOpen;
        
        
        private float targetRotationY;
        private float baseYRotation;
        private bool initialized;

        
        public override void OnStartServer()
        {
            baseYRotation = transform.localEulerAngles.y;
            isOpen = false;
            targetRotationY = baseYRotation + closedYRotation;
            initialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
            baseYRotation = transform.localEulerAngles.y;
            targetRotationY = baseYRotation + (isOpen ? openYRotation : closedYRotation);
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

        public void Interact()
        {
            Debug.Log("Interact with door");
            CmdToggleDoor();
        }

        [Command(requiresAuthority = false)] 
        private void CmdToggleDoor() 
        {
            isOpen = !isOpen;
            UpdateTarget(isOpen); 
        }
        
        private void UpdateTarget(bool open)
        {
            targetRotationY = baseYRotation + (open ? openYRotation : closedYRotation);
            initialized = true;
        }
        
        [ServerCallback]
        public void SrbToggle() => isOpen = !isOpen;

        public void InteractWithItem(PhysicalItem item)
        {
            
        }

        [Server]
        public void Open() => isOpen = true;

        [Server]
        public void Close() => isOpen = false;
        

        private void OnOpenStateChanged(bool oldValue, bool newValue)
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