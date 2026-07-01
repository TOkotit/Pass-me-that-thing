using FishNet.Object;

using UnityEngine;

namespace MainCharacter_old
{
    public class LocalVisionShaderApplier : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float radius = 10f;

        [Header("References")]
        [SerializeField] private Transform targetTransform;

        private bool _isActive;
        

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner) return;
            EnableVision();
        }
        
        private void Update()
        {
            if (!_isActive || !IsOwner) return;
            if (GlobalVisionShaderManager.Instance == null) return;

            var pos = targetTransform ? targetTransform.position : transform.position;
            GlobalVisionShaderManager.Instance.AddZone(pos, radius);
        }

        public void EnableVision()
        {
            _isActive = true;
        }

        public void DisableVision()
        {
            _isActive = false;
        }

        private void OnDisable() => DisableVision();
    }
}