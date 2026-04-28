using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.GameFiles.InteractableObjects.Doors
{
    public class DoorInteract : NetworkBehaviour
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

        public void ToggleFromClient()
        {
            if (!isLocalPlayer)
                return;

            CmdToggleDoor();
        }

        [Command]
        private void CmdToggleDoor() => isOpen = !isOpen;
        
        [Server]
        private void SrvToggleDoor() => isOpen = !isOpen;

        [Server]
        public void OpenDoor() => isOpen = true;

        [Server]
        public void CloseDoor() => isOpen = false;

        private void OnOpenStateChanged(bool oldValue, bool newValue)
        {
            targetRotationY = newValue ? openYRotation : closedYRotation;
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