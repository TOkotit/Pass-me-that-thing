using System;
using System.Collections;
using DI;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class HandsMovement : NetworkBehaviour
    {
        [SerializeField] private MainCharacter character;

        [Header("Smooth Grab")]
        [SerializeField] private float grabDuration = 0.25f;
        [SerializeField] private AnimationCurve grabCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Transform rightHandIKTarget;
        [SerializeField] private Transform leftHandIKTarget;
        [SerializeField] private TwoBoneIKConstraint rightArmIK;
        [SerializeField] private TwoBoneIKConstraint leftArmIK;

        [Header("Grab Joint (non-aligned)")]
        [SerializeField] private ConfigurableJoint grabJoint;

        [Header("Throwing")]
        [SerializeField] private Camera camera;
        [SerializeField] private Transform animatorTransform;

        [Inject] private PlayerInventoryModel _playerInventoryModel;

        private MainCharacterModel _model;

        public Transform AnimatorTransform => animatorTransform;
        public float CurrentThrowForce => _throwForce;
        public Vector3 LocalPoint => _localPoint;

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

        private Transform _rightHandTargetPoint;
        private Transform _leftHandTargetPoint;

        private void Awake()
        {
            if (animatorTransform)
                _pivotDefaultLocalPos = animatorTransform.parent.localPosition;
            if (grabJoint) grabJoint.gameObject.SetActive(false);
            DisableIK();
        }

        private void DisableIK()
        {
            if (rightArmIK) rightArmIK.weight = 0f;
            if (leftArmIK) leftArmIK.weight = 0f;
            if (rightHandIKTarget) rightHandIKTarget.gameObject.SetActive(false);
            if (leftHandIKTarget) leftHandIKTarget.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (!character) throw new NullReferenceException($"[{gameObject.name}] HandsMovement: character is not assigned!");
            _model = character.MainCharacterModel;
            if (_model == null) throw new NullReferenceException($"[{gameObject.name}] HandsMovement: MainCharacterModel is not available!");
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
                _initialLocalRotation = Quaternion.Inverse(transform.rotation) * _heldRb.rotation;
            else
                SetupGrabJoint(item, localPoint);

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
                SetupGrabJoint(item, localPoint);

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
            if (_model == null) return;

            if (_isThrowing && Time.time - _chargeStartTime >= _model.MinChargeTime)
            {
                if (_throwForce < _model.MaxThrowForce)
                {
                    _throwForce += Time.fixedDeltaTime * _model.ThrowForceGrow;
                    UpdateModel();
                }
            }

            if (_isHolding && _heldRb && _holdPivot && isServer)
            {
                if (_shouldAlignRotation)
                    ManualHoldUpdateAligned();
                else
                    ManualHoldUpdateJoint();
            }
        }

        private void Update()
        {
            if (!_isHolding || !_heldItem) return;

            if (_heldItem.HandleType == HandleType.Free)
            {
                PositionFreeHandTargets(_heldItem, _localPoint);
            }
            else if (_heldItem.HandleType == HandleType.OneHanded)
            {
                if (_rightHandTargetPoint)
                {
                    rightHandIKTarget.position = _rightHandTargetPoint.position;
                    rightHandIKTarget.rotation = _rightHandTargetPoint.rotation;
                }
                else
                {
                    var worldPos = _heldItem.transform.TransformPoint(_localPoint);
                    rightHandIKTarget.position = worldPos;
                    rightHandIKTarget.rotation = _heldItem.transform.rotation;
                }
            }
            else if (_heldItem.HandleType == HandleType.TwoHanded)
            {
                if (_rightHandTargetPoint)
                {
                    rightHandIKTarget.position = _rightHandTargetPoint.position;
                    rightHandIKTarget.rotation = _rightHandTargetPoint.rotation;
                }
                if (_leftHandTargetPoint)
                {
                    leftHandIKTarget.position = _leftHandTargetPoint.position;
                    leftHandIKTarget.rotation = _leftHandTargetPoint.rotation;
                }
            }
        }

        private void ManualHoldUpdateAligned()
        {
            if (_model == null) return;

            var pivotPos = _holdPivot.position;
            var holderCount = Mathf.Max(1, _heldItem.Holders.Count);
            var tooHeavy = _heldRb.mass > _model.Strength / holderCount;

            var toTarget = pivotPos - _heldRb.position;
            var distance = toTarget.magnitude;
            var desiredVelocity = toTarget / Time.fixedDeltaTime;
            var linearForce = (desiredVelocity - _heldRb.linearVelocity) * _heldRb.mass / Time.fixedDeltaTime;
            if (linearForce.magnitude > _model.BaseHoldForce)
                linearForce = linearForce.normalized * _model.BaseHoldForce;
            linearForce -= _heldRb.linearVelocity * (_model.HoldDamping * _heldRb.mass);
            if (tooHeavy) linearForce.y = 0f;
            _heldRb.AddForce(linearForce, ForceMode.Force);

            if (distance > _model.MaxHoldDistance)
            {
                var correction = toTarget.normalized * (distance - _model.MaxHoldDistance);
                if (tooHeavy) correction.y = 0f;
                _heldRb.position += correction * 0.5f;
                _heldRb.linearVelocity = Vector3.zero;
            }

            var targetRot = _holdPivot.rotation;
            var rotDelta = targetRot * Quaternion.Inverse(_heldRb.rotation);
            rotDelta.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f) angle -= 360f;

            var desiredAngularVelocity = axis * (angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, _model.MaxAngularSpeed);
            _heldRb.angularVelocity = Vector3.Lerp(_heldRb.angularVelocity, desiredAngularVelocity,
                _model.AngularResponsiveness * Time.fixedDeltaTime * 60f);
        }

        private void ManualHoldUpdateJoint()
        {
            if (_model == null) return;

            var pivotPos = _holdPivot.position;
            var toTarget = pivotPos - _heldRb.position;
            var distance = toTarget.magnitude;

            var forward = _holdPivot.forward;
            var right = _holdPivot.right;
            var up = _holdPivot.up;

            var forwardOffset = Vector3.Dot(toTarget, forward);
            var sideOffset = Vector3.Dot(toTarget, right);
            var upOffset = Vector3.Dot(toTarget, up);

            var force = (_model.JointSpring * toTarget - _model.HoldDamping * _heldRb.linearVelocity) * _heldRb.mass;
            force = Vector3.ClampMagnitude(force, _model.BaseHoldForce);
            _heldRb.AddForce(force, ForceMode.Force);

            if (forwardOffset < 0f)
            {
                var pushForce = forward * (-forwardOffset * _model.JointSpring * 2f);
                pushForce = Vector3.ClampMagnitude(pushForce, _model.BaseHoldForce * 2f);
                _heldRb.AddForce(pushForce, ForceMode.Force);
            }

            if (distance > _model.MaxHoldDistance)
            {
                var correction = toTarget.normalized * (distance - _model.MaxHoldDistance);
                _heldRb.position += correction * 0.5f;
                _heldRb.linearVelocity = Vector3.zero;
            }
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
            if (_model == null) return;

            var linearDrive = new JointDrive
            {
                positionSpring = _model.JointSpring,
                positionDamper = _model.JointDamper,
                maximumForce = float.MaxValue
            };
            grabJoint.xDrive = linearDrive;
            grabJoint.yDrive = linearDrive;
            grabJoint.zDrive = linearDrive;

            var angularDrive = new JointDrive
            {
                positionSpring = _model.AngularSpring,
                positionDamper = _model.AngularDamper,
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
            StopAllCoroutines();

            _rightHandTargetPoint = null;
            _leftHandTargetPoint = null;

            if (item.HandleType == HandleType.OneHanded)
            {
                var target = item.UniversalPoint ? item.UniversalPoint : item.RightHandPoint;
                if (target)
                {
                    _rightHandTargetPoint = target.transform;
                    rightHandIKTarget.position = target.position;
                    rightHandIKTarget.rotation = target.rotation;
                }
                else
                {
                    var worldPos = item.transform.TransformPoint(localPoint);
                    rightHandIKTarget.position = worldPos;
                    rightHandIKTarget.rotation = item.transform.rotation;
                    _rightHandTargetPoint = null;
                }

                rightHandIKTarget.gameObject.SetActive(true);
                if (leftHandIKTarget) leftHandIKTarget.gameObject.SetActive(false);
                StartCoroutine(FadeIKWeight(1f, 0f, grabDuration));
            }
            else if (item.HandleType == HandleType.TwoHanded)
            {
                if (item.RightHandPoint)
                {
                    _rightHandTargetPoint = item.RightHandPoint.transform;
                    rightHandIKTarget.position = _rightHandTargetPoint.position;
                    rightHandIKTarget.rotation = _rightHandTargetPoint.rotation;
                    rightHandIKTarget.gameObject.SetActive(true);
                }
                if (item.LeftHandPoint)
                {
                    _leftHandTargetPoint = item.LeftHandPoint.transform;
                    leftHandIKTarget.position = _leftHandTargetPoint.position;
                    leftHandIKTarget.rotation = _leftHandTargetPoint.rotation;
                    leftHandIKTarget.gameObject.SetActive(true);
                }
                StartCoroutine(FadeIKWeight(1f, 1f, grabDuration));
            }
            else if (item.HandleType == HandleType.Free)
            {
                PositionFreeHandTargets(item, localPoint);
                rightHandIKTarget.gameObject.SetActive(true);
                leftHandIKTarget.gameObject.SetActive(true);
                StartCoroutine(FadeIKWeight(1f, 1f, grabDuration));
            }
        }

        public void ResetHands()
        {
            StopAllCoroutines();
            StartCoroutine(ReleaseHandsSequence());
        }

        private IEnumerator ReleaseHandsSequence()
        {
            yield return FadeIKWeight(0f, 0f, grabDuration * 0.5f);

            if (rightHandIKTarget) rightHandIKTarget.gameObject.SetActive(false);
            if (leftHandIKTarget) leftHandIKTarget.gameObject.SetActive(false);
            _rightHandTargetPoint = null;
            _leftHandTargetPoint = null;
        }

        private IEnumerator FadeIKWeight(float targetRight, float targetLeft, float duration)
        {
            var startRight = rightArmIK ? rightArmIK.weight : 0f;
            var startLeft = leftArmIK ? leftArmIK.weight : 0f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var curvedT = grabCurve.Evaluate(t);

                if (rightArmIK)
                    rightArmIK.weight = Mathf.Lerp(startRight, targetRight, curvedT);
                if (leftArmIK)
                    leftArmIK.weight = Mathf.Lerp(startLeft, targetLeft, curvedT);

                yield return null;
            }

            if (rightArmIK) rightArmIK.weight = targetRight;
            if (leftArmIK) leftArmIK.weight = targetLeft;
        }

        private void PositionFreeHandTargets(PhysicalItem item, Vector3 localPoint)
        {
            var worldPoint = item.transform.TransformPoint(localPoint);
            var rightDir = animatorTransform.right;
            var separation = 0.1f;

            rightHandIKTarget.position = worldPoint - rightDir * separation;
            leftHandIKTarget.position = worldPoint + rightDir * separation;
            rightHandIKTarget.rotation = item.transform.rotation;
            leftHandIKTarget.rotation = item.transform.rotation;
        }

        public void ChargeThrow()
        {
            _isThrowing = true;
            _chargeStartTime = Time.time;
        }

        public bool CanThrow => _model != null && Time.time - _chargeStartTime >= _model.MinChargeTime;

        public void ResetCharge()
        {
            _isThrowing = false;
            _throwForce = 0;
            UpdateModel();
        }

        private void UpdateModel()
        {
            if (_model != null)
                _playerInventoryModel.ThrowCharge = (int)(_throwForce / _model.MaxThrowForce * 100);
        }
    }
}