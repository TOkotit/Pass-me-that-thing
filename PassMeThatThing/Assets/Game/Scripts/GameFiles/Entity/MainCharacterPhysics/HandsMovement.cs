using System;
using DI;
using Game.Scripts.Enums;
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
        [Header("Hand Joints")]
        [SerializeField] private ConfigurableJoint leftJoint;
        [SerializeField] private ConfigurableJoint rightJoint;

        [Header("Grab Joint (non‑aligned)")]
        [SerializeField] private ConfigurableJoint grabJoint;

        [Header("Joint Drive")]
        [SerializeField] private float jointSpring = 3000f;
        [SerializeField] private float jointDamper = 50f;
        [SerializeField] private float angularSpring = 500f;   
        [SerializeField] private float angularDamper = 10f;  

        [Header("Linear Hold (aligned)")]
        [SerializeField] private float baseHoldForce = 500f;
        [SerializeField] private float holdDamping = 50f;
        [SerializeField] private float maxHoldDistance = 1.5f;
        [SerializeField] private float maxLiftMass = 15f;

        [Header("Angular Hold – Aligned")]
        [SerializeField] private float maxAngularSpeed = 20f;
        [SerializeField] private float angularResponsiveness = 0.6f;

        [Header("Throwing")]
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;
        [SerializeField] private Camera camera;
        [SerializeField] private Transform animatorTransform;

        [Inject] private PlayerInventoryModel _playerInventoryModel;

        public Transform AnimatorTransform => animatorTransform;
        public float CurrentThrowForce => _throwForce;
        public Vector3 LocalPoint => _localPoint;

        public float JointSpring { get => jointSpring; set => jointSpring = value; }
        public float JointDamper { get => jointDamper; set => jointDamper = value; }
        
        private bool _isThrowing;
        private float _chargeStartTime;
        private float _throwForce;

        private PhysicalItem _heldItem;
        private Rigidbody _heldRb;
        private Transform _holdPivot;
        private bool _isHolding;
        private Quaternion _initialLocalRotation;
        private bool _shouldAlignRotation;
        private Vector3 _localPoint;

        private Vector3 _pivotDefaultLocalPos;

        private void Awake()
        {
            var gameplayScope = LifetimeScope.Find<GameplayScope>();
            if (gameplayScope) gameplayScope.Container.Inject(this);
            if (animatorTransform)
                _pivotDefaultLocalPos = animatorTransform.parent.localPosition;
            if (grabJoint) grabJoint.gameObject.SetActive(false);
        }

        [Server]
        public void GrabItem(PhysicalItem item, Vector3 localPoint)
        {
            _localPoint = localPoint;
            _heldItem = item;
            _heldRb = item.Rigidbody;
            _holdPivot = animatorTransform;
            _shouldAlignRotation = item.HasToBeAligned;

            if (_shouldAlignRotation)
            {
                _initialLocalRotation = Quaternion.Inverse(transform.rotation) * _heldRb.rotation;
            }
            else
            {
                SetupGrabJoint(item, localPoint);
            }

            AlignPivotForItem(item);
            MoveHands(item, localPoint);
            _isHolding = true;

            RpcGrabItem(item, _initialLocalRotation, _shouldAlignRotation, localPoint);
        }

        [ClientRpc]
        private void RpcGrabItem(PhysicalItem item, Quaternion initRot, bool align, Vector3 localPoint)
        {
            _heldItem = item;
            _heldRb = item.Rigidbody;
            _holdPivot = animatorTransform;
            _initialLocalRotation = initRot;
            _shouldAlignRotation = align;
            _localPoint = localPoint;

            if (!_shouldAlignRotation)
            {
                SetupGrabJoint(item, localPoint);
            }

            AlignPivotForItem(item);
            MoveHands(item, localPoint);
            _isHolding = true;
        }

        [Server]
        public void ReleaseItem(PhysicalItem item, float throwForce, bool canThrow)
        {
            if (!_isHolding || _heldItem != item) return;

            _isHolding = false;
            ResetPivot();
            ResetHands();

            if (!_shouldAlignRotation)
            {
                grabJoint.connectedBody = null;
                grabJoint.gameObject.SetActive(false);
            }

            if (canThrow && _heldRb)
            {
                item.IsThrown = true;
                var force = throwForce * camera.transform.forward;
                _heldRb.AddForce(force, ForceMode.Impulse);
                RpcApplyThrowForce(item, force);
            }

            _heldRb = null;
            _heldItem = null;
            _throwForce = 0;
            _isThrowing = false;
            RpcReleaseItem(item);
        }

        [ClientRpc]
        private void RpcApplyThrowForce(PhysicalItem item, Vector3 force)
        {
            if (item && item.Rigidbody)
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
        }

        [ClientRpc]
        private void RpcReleaseItem(PhysicalItem item)
        {
            _isHolding = false;
            if (!_shouldAlignRotation)
            {
                grabJoint.connectedBody = null;
                grabJoint.gameObject.SetActive(false);
            }
            _heldRb = null;
            _heldItem = null;
            ResetPivot();
            ResetHands();
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

            if (_isHolding && _heldRb && _holdPivot && isServer && _shouldAlignRotation)
            {
                ManualHoldUpdateAligned();
            }
        }

        private void ManualHoldUpdateAligned()
        {
            var pivotPos = _holdPivot.position;
            var holderCount = Mathf.Max(1, _heldItem.Holders.Count);
            var tooHeavy = _heldRb.mass > maxLiftMass / holderCount;

            var toTarget = pivotPos - _heldRb.position;
            var distance = toTarget.magnitude;
            var desiredVelocity = toTarget / Time.fixedDeltaTime;
            var linearForce = (desiredVelocity - _heldRb.linearVelocity) * _heldRb.mass / Time.fixedDeltaTime;
            if (linearForce.magnitude > baseHoldForce)
                linearForce = linearForce.normalized * baseHoldForce;
            linearForce -= _heldRb.linearVelocity * (holdDamping * _heldRb.mass);
            if (tooHeavy) linearForce.y = 0f;
            _heldRb.AddForce(linearForce, ForceMode.Force);

            if (distance > maxHoldDistance)
            {
                var correction = toTarget.normalized * (distance - maxHoldDistance);
                if (tooHeavy) correction.y = 0f;
                _heldRb.position += correction * 0.5f;
                _heldRb.linearVelocity = Vector3.zero;
            }

            var targetRot = _holdPivot.rotation;
            var rotDelta = targetRot * Quaternion.Inverse(_heldRb.rotation);
            rotDelta.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f) angle -= 360f;

            var desiredAngularVelocity = axis * (angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, maxAngularSpeed);
            _heldRb.angularVelocity = Vector3.Lerp(_heldRb.angularVelocity, desiredAngularVelocity,
                angularResponsiveness * Time.fixedDeltaTime * 60f);
        }

        private void SetupGrabJoint(PhysicalItem item, Vector3 localPoint)
        {
            grabJoint.connectedBody = _heldRb;
            grabJoint.connectedAnchor = item.CanBeOwned ? Vector3.zero : localPoint;

            ApplyJointDrive();
            grabJoint.gameObject.SetActive(true);
        }

        public void ApplyJointDrive()
        {
            var linearDrive = new JointDrive
            {
                positionSpring = jointSpring,
                positionDamper = jointDamper,
                maximumForce = float.MaxValue  
            };
            grabJoint.xDrive = linearDrive;
            grabJoint.yDrive = linearDrive;
            grabJoint.zDrive = linearDrive;
            var angularDrive = new JointDrive
            {
                positionSpring = angularSpring,
                positionDamper = angularDamper,
                maximumForce = float.MaxValue   
            };
            grabJoint.angularXDrive = angularDrive;
            grabJoint.angularYZDrive = angularDrive;
        }

        public void AlignPivotForItem(PhysicalItem item)
        {
            if (!item) return;
            animatorTransform.parent.localPosition = _pivotDefaultLocalPos + item.DefaultPosition;
        }

        public void ResetPivot()
        {
            animatorTransform.parent.localPosition = _pivotDefaultLocalPos;
        }

        public void MoveHands(PhysicalItem item, Vector3 localPoint)
        {
            var separation = 0.1f; 
            var worldPoint = item.transform.TransformPoint(localPoint);
            var pivotRight = animatorTransform.right;
            var rightWorld = worldPoint - pivotRight * separation;
            var leftWorld = worldPoint + pivotRight * separation;
            rightJoint.connectedAnchor = item.transform.InverseTransformPoint(rightWorld);
            leftJoint.connectedAnchor = item.transform.InverseTransformPoint(leftWorld);
            
            rightJoint.transform.localPosition = Vector3.zero;
            leftJoint.transform.localPosition = Vector3.zero;

            if (item.HandleType == HandleType.OneHanded)
            {
                rightJoint.gameObject.SetActive(true);
                rightJoint.connectedBody = item.UniversalPoint ? item.UniversalPoint : item.RightHandPoint;
            }
            else if (item.HandleType == HandleType.TwoHanded)
            {
                if (item.RightHandPoint && item.LeftHandPoint)
                {
                    rightJoint.gameObject.SetActive(true);
                    rightJoint.connectedBody = item.RightHandPoint;
                    leftJoint.gameObject.SetActive(true);
                    leftJoint.connectedBody = item.LeftHandPoint;
                }
            }
            else if (item.HandleType == HandleType.Free)
            {
                rightJoint.gameObject.SetActive(true);
                rightJoint.connectedBody = item.Rigidbody;
                leftJoint.gameObject.SetActive(true);
                leftJoint.connectedBody = item.Rigidbody;
            }
        }

        public void ResetHands()
        {
            ResetLeftHand();
            ResetRightHand();
        }

        public void ResetLeftHand()
        {
            if (leftJoint) leftJoint.connectedBody = null;
            leftJoint.gameObject.SetActive(false);
        }

        public void ResetRightHand()
        {
            if (rightJoint) rightJoint.connectedBody = null;
            rightJoint.gameObject.SetActive(false);
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