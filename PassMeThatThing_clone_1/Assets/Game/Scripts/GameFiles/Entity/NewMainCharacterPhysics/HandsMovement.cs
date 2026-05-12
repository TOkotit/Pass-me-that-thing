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
        [SerializeField] private Rigidbody leftHandRB;
        [SerializeField] private Rigidbody rightHandRB;
        [SerializeField] private ConfigurableJoint leftJoint;
        [SerializeField] private ConfigurableJoint rightJoint;
        
        [SerializeField] private float throwForce = 0;
        [SerializeField] private float maxThrowForce = 15f;
        private bool _isThrowing = false;
        private float _chargeStartTime;
        [SerializeField] private float minChargeTime = 0.3f;
        
        [Header("Connection interface")] 
        [SerializeField] private Rigidbody connectionInterface;
        [SerializeField] private Joint hardConnection;
        [SerializeField] private ConfigurableJoint connectionToPivot;
        [SerializeField] private Rigidbody pivot;
        public Rigidbody Pivot => pivot;

        private void MoveHand(Rigidbody handRB, Vector3 direction, ConfigurableJoint joint)
        {
            //тут часть уже на настоящей модели
        }

        public void Move(Hand hand)
        {
            //и тут
        }
        [Server]
        public void GrabItem(PhysicalItem item)
        {
            if (isServer)
            {
                connectionInterface.position = item.transform.position;

                Debug.Log(connectionInterface.position);
                if (item.HandleType == HandleType.OneHanded | item.HandleType == HandleType.TwoHanded)
                {
                    connectionInterface.rotation = item.transform.rotation;
                }

                hardConnection.connectedBody = null;
                hardConnection.connectedBody = item.Rigidbody;
                connectionToPivot.connectedBody = pivot;
            }

            ClientGrabItem(item);
        }
        [TargetRpc]
        private void ClientGrabItem(PhysicalItem item)
        {
            connectionInterface.position = item.transform.position;
            hardConnection.connectedBody = null;
            hardConnection.connectedBody = item.Rigidbody;
            connectionToPivot.connectedBody = pivot;
        }
        [Server]
        public void ReleaseItem(PhysicalItem item)
        {
            if (isServer)
            {
                connectionToPivot.connectedBody = null;
                hardConnection.connectedBody = null;
                if (Time.time - _chargeStartTime >= minChargeTime)
                {
                    item.Rigidbody.AddForce(throwForce * pivot.transform.forward, ForceMode.Impulse);
                }

                throwForce = 0;
                _isThrowing = false;
            }
            ClientReleaseItem(item);
        }
        [TargetRpc]
        private void ClientReleaseItem(PhysicalItem item)
        {
            connectionToPivot.connectedBody = null;
            hardConnection.connectedBody = null;
        }
        
        [Server]
        public void ResetLeftHand()
        {
            if (leftJoint)
            {
                leftJoint.connectedBody = null;
            }
        }

        [Server]
        public void ResetRightHand()
        {
            if (rightJoint)
            {
                rightJoint.connectedBody = null;
            }
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

        public void ChargeThrow()
        {
            _isThrowing = true;
            _chargeStartTime = Time.time;
        }

        private void FixedUpdate()
        {
            if (_isThrowing && Time.time - _chargeStartTime >= minChargeTime)
            {
                if (throwForce < maxThrowForce)
                {
                    throwForce += Time.fixedDeltaTime * 3;
                }
            }
        }
    }
}