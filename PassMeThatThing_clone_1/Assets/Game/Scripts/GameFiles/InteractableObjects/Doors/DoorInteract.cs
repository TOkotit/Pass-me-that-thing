using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.GameFiles.InteractableObjects.Doors
{
    public class DoorInteract : NetworkBehaviour, IInteractable
    {
        [Header("Movement")]
        [SerializeField] private float closedYRotation = 0f;
        [SerializeField] private float openYRotation = 90f;
        [SerializeField] private float moveSpeed = 2f;

        [SyncVar(hook = nameof(OnOpenStateChanged))]
        private bool isOpen;
        
        
        private float targetRotationY;
        private bool initialized;

        public override void OnStartServer()
        {
            base.OnStartServer();
            isOpen = false;
            targetRotationY = closedYRotation;
            initialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            targetRotationY = isOpen ? openYRotation : closedYRotation;
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
            targetRotationY = open ? openYRotation : closedYRotation;
            initialized = true;
        }
        
        [ServerCallback]
        public void SrbToggle() => isOpen = !isOpen;

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