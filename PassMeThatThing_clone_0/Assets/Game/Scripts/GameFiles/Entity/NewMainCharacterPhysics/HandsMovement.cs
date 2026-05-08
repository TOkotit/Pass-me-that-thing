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

        [Header("Connection interface")] 
        [SerializeField] private Transform connectionInterface;
        [SerializeField] private Joint hardConnection;
        [SerializeField] private ConfigurableJoint connectionToPivot;
        [SerializeField] Rigidbody pivot;
        

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
            Debug.Log("Grabbing Item");
            connectionInterface.position = item.transform.position;
            if (item.HandleType == HandleType.OneHanded | item.HandleType == HandleType.TwoHanded)
            {
                connectionInterface.rotation = item.transform.rotation;
            }
            hardConnection.connectedBody = item.Rigidbody;
            connectionToPivot.connectedBody = pivot;
        }

        [Server]
        public void ReleaseItem(PhysicalItem item)
        {
            connectionToPivot.connectedBody = null;
            hardConnection.connectedBody = null;
            item.Rigidbody.AddForce(throwForce * pivot.transform.forward, ForceMode.Impulse);
            throwForce = 0;
            _isThrowing = false;
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
            _isThrowing =  true;
        }
        
        private void FixedUpdate()
        {
            if (_isThrowing)
            {
                while (throwForce < maxThrowForce)
                {
                    throwForce += Time.fixedDeltaTime * 3;
                }
            }
        }
    }
}