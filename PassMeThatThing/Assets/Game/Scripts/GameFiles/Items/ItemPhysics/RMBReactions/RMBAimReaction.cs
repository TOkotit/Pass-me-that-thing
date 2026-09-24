using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class RmbAimReaction : ItemReaction
    {
        [Header("Zoom")]
        [SerializeField] private float aimFov = 40f;
        [SerializeField] private float zoomSpeed = 12f;

        [Header("Accuracy")]
        [SerializeField, Range(0.01f, 1f)] private float spreadMultiplier = 0.25f;


        public override bool IsContinuous => true;

        private bool _isAiming;
        private float _originalFov;
        private Vector3 _originalDefaultPosition;
        private float _originalBaseSpread;
        private float _originalSpreadPerStability;

        // ------------------------------------------------------------------ //

        public override void Act()
        {
            if (_isAiming) return;
            StartAim();
        }

        public override void DeAct()
        {
            if (!_isAiming) return;
            StopAim();
        }

        private void Update()
        {
            if (!_isAiming) return;
            ApplySmoothTransforms();
            if (!Item || !Item.Owner)
            {
                StopAim();
                return;
            }
        }

        // ------------------------------------------------------------------ //

        private void StartAim()
        {
            _isAiming = true;

            if (Item)
                _originalDefaultPosition = Item.DefaultPosition;

            var cam = Item?.Owner?.MCamera?.Camera;
            if (cam)
                _originalFov = cam.fieldOfView;
            
            CacheAndApplySpread(spreadMultiplier);
        }

        private void StopAim()
        {
            _isAiming = false;

            RestoreSpread();

            if (Item)
                Item.DefaultPosition = _originalDefaultPosition;

            var cam = Item?.Owner?.MCamera?.Camera;
            if (cam)
                cam.fieldOfView = _originalFov;
        }

        private void ApplySmoothTransforms()
        {
            var owner = Item?.Owner;
            if (!owner) return;

            var cam = owner.MCamera?.Camera;
            if (cam)
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, aimFov, Time.deltaTime * zoomSpeed);
        }

        // ------------------------------------------------------------------ //

        private void CacheAndApplySpread(float multiplier)
        {
            var gun = Item?.LmbReaction as LmbMachineGun;
            if (!gun) return;

            _originalBaseSpread = gun.BaseSpread;
            _originalSpreadPerStability = gun.SpreadPerStability;

            gun.SetSpread(_originalBaseSpread * multiplier,
                          _originalSpreadPerStability * multiplier);
        }

        private void RestoreSpread()
        {
            var gun = Item?.LmbReaction as LmbMachineGun;
            if (!gun) return;

            gun.SetSpread(_originalBaseSpread, _originalSpreadPerStability);
        }
    }
}