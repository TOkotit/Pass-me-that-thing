using System.Collections;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.Valve
{
    public class ValveInteract : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;
        [SerializeField] private float closedAngle = 0f;
        [SerializeField] private float openAngle = 360f;
        [SerializeField] private float moveSpeed = 100f;
        [SerializeField] private Transform pivot;
        
        [SyncVar(hook = nameof(OnOpenStateChanged))]
        private bool isOpen;

        private float currentAngle;
        private float targetAngle;
        private bool initialized;
        
        private Quaternion initialRotation;
        
        private void Awake()
        {
            if (pivot) initialRotation = pivot.localRotation;
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            targetAngle = isOpen ? openAngle : closedAngle;
            currentAngle = targetAngle;
            initialized = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            targetAngle = isOpen ? openAngle : closedAngle;
            currentAngle = targetAngle;
            initialized = true;
        }
        public void Interact()
        {
            Debug.Log("Interacting...");
            CmdToggleValve();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdToggleValve()
        {
            isOpen = !isOpen;
            UpdateTarget(isOpen);
        }
        
        
        private void UpdateTarget(bool open)
        {
            targetAngle = open ? openAngle : closedAngle;
        }
        
        
        private void FixedUpdate()
        {
            if (!initialized || !pivot ) return;
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, moveSpeed * Time.fixedDeltaTime);
            pivot.localRotation = initialRotation * Quaternion.Euler(rotationAxis * currentAngle);
            
        }
        
        private void OnOpenStateChanged(bool oldValue, bool newValue)
        {
            UpdateTarget(newValue);
        }

        [ServerCallback]
        public void SrbToggle() => isOpen = !isOpen;
        
        [Server]
        public void Open() => isOpen = true;

        [Server]
        public void Close() => isOpen = false;
    }
}