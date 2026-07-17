using System;
using System.Collections;
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
        [Header("Hands")]
        [SerializeField] private ConfigurableJoint leftJoint;
        [SerializeField] private ConfigurableJoint rightJoint;

        [Header("Throwing")]
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;
        [SerializeField] private Camera camera;
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
        //[SerializeField] private FixedJoint baseJoint;
        [SerializeField] private ConfigurableJoint grabJoint;   
        [SerializeField] private Transform animatorTransform;

        [Inject] private PlayerInventoryModel _playerInventoryModel;
        public Transform AnimatorTransform => animatorTransform;
        public float CurrentThrowForce => _throwForce;

        private void Awake()
        {
            var gameplayScope = LifetimeScope.Find<GameplayScope>();
            if (gameplayScope) gameplayScope.Container.Inject(this);
            _originalXDrive = grabJoint.xDrive;
            _originalZDrive = grabJoint.zDrive;
            _originalYDrive = grabJoint.yDrive;
            _originalAngularXDrive = grabJoint.angularXDrive;
            _originalAngularYZDrive = grabJoint.angularYZDrive;
            if (grabJoint)
                grabJoint.gameObject.SetActive(false);
            if (animatorTransform)
                _pivotDefaultLocalPos = animatorTransform.parent.localPosition;
        }


        public void MoveHands( PhysicalItem item)
        {
            Debug.Log("Moving hands");
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
        

        [Server]
        public void GrabItem(PhysicalItem item)
        {
            if (item.HasToBeAligned)
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
            if (item.HasToBeAligned)
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
                Vector3 force = throwForce * camera.transform.forward;
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

        public void FixGrab(Rigidbody body)
        {
            grabJoint.connectedBody = null;
            grabJoint.connectedBody = body;
            grabJoint.xMotion = ConfigurableJointMotion.Locked;
            grabJoint.yMotion = ConfigurableJointMotion.Locked;
            grabJoint.zMotion = ConfigurableJointMotion.Locked;
            grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
            grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
            grabJoint.angularZMotion = ConfigurableJointMotion.Locked;
            
            animatorTransform.localPosition = new Vector3(0,-1f, 0);
        }

        public void ReleaseGrab()
        {
            AlignJointToPivot();
            grabJoint.xMotion = ConfigurableJointMotion.Free;
            grabJoint.yMotion = ConfigurableJointMotion.Free;
            grabJoint.zMotion = ConfigurableJointMotion.Free;
            grabJoint.angularXMotion = ConfigurableJointMotion.Free;
            grabJoint.angularYMotion = ConfigurableJointMotion.Free;
            grabJoint.angularZMotion = ConfigurableJointMotion.Free;
            animatorTransform.localPosition = Vector3.zero;
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
            animatorTransform.localPosition = _pivotDefaultLocalPos;
        }

        public void AlignPivotForItem(PhysicalItem item)
        {
            if (!item) return;
            animatorTransform.parent.localPosition = _pivotDefaultLocalPos + item.DefaultPosition;
        }
        
        private void AlignJointToPivot()
        {

            animatorTransform.localPosition = Vector3.zero;
            if(!grabJoint.connectedBody)return;
            var currentRelRot = Quaternion.Inverse(transform.rotation) * grabJoint.connectedBody.rotation;
            var desiredRelRot = Quaternion.Inverse(transform.rotation) * animatorTransform.rotation;
            grabJoint.targetRotation = Quaternion.Inverse(currentRelRot) * desiredRelRot;
        }
    }
}