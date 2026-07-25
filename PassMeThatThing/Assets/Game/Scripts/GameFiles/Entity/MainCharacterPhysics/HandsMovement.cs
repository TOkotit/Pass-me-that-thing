using System;
using DI;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class HandsMovement : NetworkBehaviour
    {
        [Header("Linear Hold")]
        [SerializeField] private float baseHoldForce = 500f;
        [SerializeField] private float holdDamping = 50f;
        [SerializeField] private float maxHoldDistance = 1.5f;

        [Header("Angular Hold (only for aligned items)")]
        [SerializeField] private float maxAngularSpeed = 15f;
        [SerializeField] private float angularResponsiveness = 0.5f;

        [Header("Throwing")]
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;
        [SerializeField] private Camera camera;
        [SerializeField] private Transform animatorTransform;   
        
        [Inject] private PlayerInventoryModel _playerInventoryModel;

        public Transform AnimatorTransform => animatorTransform;
        public float CurrentThrowForce => _throwForce;

        private bool _isThrowing;
        private float _chargeStartTime;
        private float _throwForce;

        private PhysicalItem _heldItem;
        private Rigidbody _heldRb;
        private Transform _holdPivot;
        private bool _isHolding;
        private Vector3 _grabOffset;          
        private bool _shouldAlignRotation;    
        
        private void Awake()
        {
            var gameplayScope = LifetimeScope.Find<GameplayScope>();
            if (gameplayScope) gameplayScope.Container.Inject(this);
        }

        [Server]
        public void GrabItem(PhysicalItem item)
        {
            if (_isHolding)
                ReleaseItem(_heldItem, 0, false);

            _heldItem = item;
            _heldRb = item.Rigidbody;
            _holdPivot = animatorTransform;

            var grabPointWorld = _heldRb.position;
            if (item.UniversalPoint)
                grabPointWorld = item.UniversalPoint.position;
            
            _grabOffset = _heldRb.position - grabPointWorld;   _shouldAlignRotation = item.HasToBeAligned;

            _heldRb.useGravity = true;
            _heldRb.isKinematic = false;
            _heldRb.linearDamping = 1f;
            _heldRb.angularDamping = 2f;

            _isHolding = true;
            RpcGrabItem(item.netIdentity, _grabOffset, _shouldAlignRotation);
        }

        [ClientRpc]
        private void RpcGrabItem(NetworkIdentity itemId, Vector3 grabOffset, bool alignRotation)
        {
            if (itemId && itemId.TryGetComponent<PhysicalItem>(out var item))
            {
                _heldItem = item;
                _heldRb = item.Rigidbody;
                _holdPivot = animatorTransform;
                _grabOffset = grabOffset;
                _shouldAlignRotation = alignRotation;
                _isHolding = true;
            }
        }

        [Server]
        public void ReleaseItem(PhysicalItem item, float throwForce, bool canThrow)
        {
            if (!_isHolding || _heldItem != item) return;

            _isHolding = false;

            if (canThrow && _heldRb)
            {
                item.IsThrown = true;
                var force = throwForce * camera.transform.forward;
                _heldRb.AddForce(force, ForceMode.Impulse);
                RpcApplyThrowForce(item.netIdentity, force);
            }

            _heldRb = null;
            _heldItem = null;
            _throwForce = 0;
            _isThrowing = false;
            RpcReleaseItem(item.netIdentity);
        }

        [ClientRpc]
        private void RpcApplyThrowForce(NetworkIdentity itemId, Vector3 force)
        {
            if (itemId && itemId.TryGetComponent<PhysicalItem>(out var item) && item.Rigidbody)
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
        }

        [ClientRpc]
        private void RpcReleaseItem(NetworkIdentity itemId)
        {
            _isHolding = false;
            _heldRb = null;
            _heldItem = null;
        }

        private void FixedUpdate()
        {
            if (_isThrowing && Time.time - _chargeStartTime >= minChargeTime)
            {
                if (_throwForce < maxThrowForce)
                {
                    _throwForce += Time.fixedDeltaTime * throwForceGrow;
                    UpdateModel();
                }
            }

            if (_isHolding && _heldRb && _holdPivot && isServer)
            {
                ManualHoldUpdate();
            }
        }

        private void ManualHoldUpdate()
        {
            var pivotPos = _holdPivot.position;
            var pivotRot = _holdPivot.rotation;
            var targetCenterPos = pivotPos + _grabOffset;

            var toTarget = targetCenterPos - _heldRb.position;
            var distance = toTarget.magnitude;
            var desiredVelocity = toTarget / Time.fixedDeltaTime;
            var force = (desiredVelocity - _heldRb.linearVelocity) * _heldRb.mass / Time.fixedDeltaTime;
            if (force.magnitude > baseHoldForce)
                force = force.normalized * baseHoldForce;
            force -= _heldRb.linearVelocity * (holdDamping * _heldRb.mass);
            _heldRb.AddForce(force, ForceMode.Force);

            if (distance > maxHoldDistance)
            {
                var correction = toTarget.normalized * (distance - maxHoldDistance);
                _heldRb.position += correction * 0.5f;
                _heldRb.linearVelocity = Vector3.zero;
            }

            if (_shouldAlignRotation)
            {
                var targetRot = pivotRot;
                var rotDelta = targetRot * Quaternion.Inverse(_heldRb.rotation);
                rotDelta.ToAngleAxis(out var angle, out var axis);
                if (angle > 180f) angle -= 360f;

                var desiredAngularVelocity = axis * (angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
                desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, maxAngularSpeed);

                _heldRb.angularVelocity = Vector3.Lerp(
                    _heldRb.angularVelocity,
                    desiredAngularVelocity,
                    angularResponsiveness * Time.fixedDeltaTime * 60f
                );
            }
        }

        public void ChargeThrow()
        {
            _isThrowing = true;
            _chargeStartTime = Time.time;
        }

        public bool CanThrow => Time.time - _chargeStartTime >= minChargeTime;

        public void ResetCharge()
        {
            _isThrowing = false;
            _throwForce = 0;
            UpdateModel();
        }

        private void UpdateModel()
        {
            _playerInventoryModel.ThrowCharge = (int)(_throwForce / maxThrowForce * 100);
        }
    }
}