using System;
using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class HandsMovement : NetworkBehaviour
    {
        [Header("Hands")]
        [SerializeField] private Rigidbody leftHandRB;
        [SerializeField] private Rigidbody rightHandRB;
        [SerializeField] private ConfigurableJoint leftJoint;
        [SerializeField] private ConfigurableJoint rightJoint;

        [Header("Throwing")]
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;
        private bool _isThrowing;
        private float _chargeStartTime;
        private float _throwForce;

        [Header("Grabbing")]
        [SerializeField] private ConfigurableJoint grabJoint;   
        [SerializeField] private Rigidbody pivot;
        public Rigidbody Pivot => pivot;
        public float CurrentThrowForce => _throwForce;

        private void Awake()
        {
            if (grabJoint)
                grabJoint.gameObject.SetActive(false);
        }

        private void MoveHand(Rigidbody handRB, Vector3 direction, ConfigurableJoint joint)
        {
            // реализация позже
        }

        public void Move(Hand hand)
        {
            // реализация позже
        }

        [Server]
        public void Move(Hand hand, PhysicalItem item)
        {
            if (hand == Hand.Left)
            {
                if (leftJoint)
                {
                    leftJoint.connectedBody = item.HandleType == HandleType.OneHanded ? item.UniversalPoint : item.LeftHandlPoint;
                    leftJoint.targetPosition = Vector3.zero;
                }
            }
            else
            {
                if (rightJoint)
                {
                    rightJoint.connectedBody = item.HandleType == HandleType.OneHanded ? item.UniversalPoint : item.RightHandPoint;
                    rightJoint.targetPosition = Vector3.zero;
                }
            }
        }

        [Server]
        public void ResetLeftHand()
        {
            if (leftJoint)
                leftJoint.connectedBody = null;
        }

        [Server]
        public void ResetRightHand()
        {
            if (rightJoint)
                rightJoint.connectedBody = null;
        }

        [Server]
        public void GrabItem(PhysicalItem item)
        {
            grabJoint.gameObject.SetActive(true);
            grabJoint.connectedBody = null;
            grabJoint.connectedBody = item.Rigidbody;
            ClientGrabItem(item);
        }

        [ClientRpc]
        private void ClientGrabItem(PhysicalItem item)
        {
            grabJoint.gameObject.SetActive(true);
            grabJoint.connectedBody = null;
            grabJoint.connectedBody = item.Rigidbody;
        }

        [Server]
        public void ReleaseItem(PhysicalItem item, float throwForce, bool canThrow)
        {
            grabJoint.connectedBody = null;
            grabJoint.gameObject.SetActive(false);
    
            if (canThrow)
            {
                Vector3 force = throwForce * pivot.transform.forward;
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
                TargetApplyThrowForce(connectionToClient, item, force);
            }
    
            _throwForce = 0;
            _isThrowing = false;
    
            ClientReleaseItem();
        }

        [TargetRpc]
        private void TargetApplyThrowForce(NetworkConnection target, PhysicalItem item, Vector3 force)
        {
            if (item)
                item.Rigidbody.AddForce(force, ForceMode.Impulse);
        }
        
        [ClientRpc]
        private void ClientReleaseItem()
        {
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
                }
            }
        }
        
        public bool CanThrow => Time.time - _chargeStartTime >= minChargeTime;

        public void ResetCharge()
        {
            _isThrowing = false;
            _throwForce = 0;
        }
    }
}