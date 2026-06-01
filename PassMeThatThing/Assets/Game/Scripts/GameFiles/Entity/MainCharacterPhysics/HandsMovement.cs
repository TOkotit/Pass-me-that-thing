using System;
using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class HandsMovement : NetworkBehaviour
    {
        [Header("Hands")]
        [SerializeField] private ConfigurableJoint leftJoint;
        [SerializeField] private ConfigurableJoint rightJoint;

        [Header("Throwing")]
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;
        private bool _isThrowing;
        private float _chargeStartTime;
        private float _throwForce;
        private JointDrive _originalXDrive;
        private JointDrive _originalZDrive;
        private JointDrive _originalYDrive;
        private JointDrive _originalAngularXDrive;
        private JointDrive _originalAngularYZDrive;
        private Vector3 _pivotDefaultLocalPos;

        [Header("Grabbing")] 
        [SerializeField] private FixedJoint collarbone;
        [SerializeField] private Rigidbody torso;
        [SerializeField] private ConfigurableJoint grabJoint;   
        [SerializeField] private Rigidbody pivot;

        [Inject] private PlayerInventoryModel _playerInventoryModel;
        public Rigidbody Pivot => pivot;
        public float CurrentThrowForce => _throwForce;

        private void Awake()
        {
            _originalXDrive = grabJoint.xDrive;
            _originalZDrive = grabJoint.zDrive;
            _originalYDrive = grabJoint.yDrive;
            _originalAngularXDrive = grabJoint.angularXDrive;
            _originalAngularYZDrive = grabJoint.angularYZDrive;
            if (grabJoint)
                grabJoint.gameObject.SetActive(false);
            if (pivot)
                _pivotDefaultLocalPos = pivot.transform.localPosition;
        }


        public void MoveHands( PhysicalItem item)
        {
            if (item.HandleType == HandleType.OneHanded)
            {
                rightJoint.gameObject.SetActive(true);
                if (item.UniversalPoint)
                {
                    
                    rightJoint.connectedBody = item.UniversalPoint;
                }
                else
                {
                    rightJoint.connectedBody = item.RightHandPoint;
                }
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

            if (item.HandleType == HandleType.Free)
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
            if (leftJoint)
                leftJoint.connectedBody = null;
            leftJoint.gameObject.SetActive(false);
        }

        public void ResetRightHand()
        {
            if (rightJoint)
                rightJoint.connectedBody = null;
            rightJoint.gameObject.SetActive(false);
        }
        public void EnableHorizontalWeakDrive()
        {
            JointDrive weakXDrive = _originalXDrive;
            weakXDrive.positionSpring = _originalXDrive.positionSpring/4;
            JointDrive weakZDrive = _originalZDrive;
            weakZDrive.positionSpring = _originalZDrive.positionSpring/4;
            JointDrive weakYDrive = _originalYDrive;
            weakZDrive.positionSpring = _originalZDrive.positionSpring/1;
            grabJoint.xDrive = weakXDrive;
            grabJoint.zDrive = weakZDrive;
            grabJoint.yDrive = weakYDrive;
        }
        
        public void DisableHorizontalWeakDrive()
        {
            grabJoint.xDrive = _originalXDrive;
            grabJoint.zDrive = _originalZDrive;
            grabJoint.yDrive = _originalYDrive;
        }

        [Server]
        public void GrabItem(PhysicalItem item)
        {
            if (item.CanBeOwned)
            {
                grabJoint.angularXDrive = _originalAngularXDrive;
                grabJoint.angularYZDrive = _originalAngularYZDrive;
            }
            else
            {
                var zeroDrive = new JointDrive { positionSpring = 0f, positionDamper = 0f, maximumForce = float.MaxValue };
                grabJoint.angularXDrive = zeroDrive;
                grabJoint.angularYZDrive = zeroDrive;
            }
            if (item.UniversalPoint)
            {
                grabJoint.connectedAnchor = item.UniversalPoint.transform.localPosition;
            }
            else
            {
                grabJoint.connectedAnchor = Vector3.zero; 
            }
            grabJoint.gameObject.SetActive(true);
            grabJoint.connectedBody = null;
            grabJoint.connectedBody = item.Rigidbody;
            AlignJointToPivot();
            
            AlignPivotForItem(item);
            ClientGrabItem(item);
            MoveHands(item);
        }

        [ClientRpc]
        private void ClientGrabItem(PhysicalItem item)
        {
            if (item.UniversalPoint)
            {
                grabJoint.connectedAnchor = item.UniversalPoint.transform.localPosition;
            }
            else
            {
                grabJoint.connectedAnchor = Vector3.zero; 
            }
            grabJoint.gameObject.SetActive(true);
            grabJoint.connectedBody = null;
            grabJoint.connectedBody = item.Rigidbody;
            
            AlignJointToPivot();
            AlignPivotForItem(item);
            MoveHands(item);
        }

        [Server]
        public void ReleaseItem(PhysicalItem item, float throwForce, bool canThrow)
        {
            grabJoint.connectedBody = null;
            grabJoint.gameObject.SetActive(false);
    
            if (canThrow)
            {
                item.IsThrown = true;
                Vector3 force = throwForce * pivot.transform.forward;
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
                ClientApplyThrowForce(item, force);
            }
    
            _throwForce = 0;
            _isThrowing = false;
            ResetHands();
            ClientReleaseItem();
        }

        [ClientRpc]
        private void ClientApplyThrowForce(PhysicalItem item, Vector3 force)
        {
            if (item)
            {
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
        
        [ClientRpc]
        private void ClientReleaseItem()
        {
            ResetHands();
            grabJoint.connectedBody = null;
            grabJoint.gameObject.SetActive(false);
        }

        public void ChargeThrow()
        {
            _isThrowing = true;
            _chargeStartTime = Time.time;
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
        
        public void ResetPivot()
        {
            collarbone.connectedBody = null;
            pivot.transform.localPosition = _pivotDefaultLocalPos;
            collarbone.connectedBody = torso;
        }
        
        public void AlignPivotForItem(PhysicalItem item)
        {
            ResetPivot();
            collarbone.connectedBody = null;
            pivot.transform.localPosition = _pivotDefaultLocalPos + item.DefaultPosition;
            collarbone.connectedBody = torso;
        }
        
        private void AlignJointToPivot()
        {
            var currentRelRot = Quaternion.Inverse(transform.rotation) * grabJoint.connectedBody.rotation;
            var desiredRelRot = Quaternion.Inverse(transform.rotation) * pivot.rotation;
            grabJoint.targetRotation = Quaternion.Inverse(currentRelRot) * desiredRelRot;
        }
    }
}