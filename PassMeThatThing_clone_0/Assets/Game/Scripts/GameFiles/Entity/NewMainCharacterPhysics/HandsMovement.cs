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
        
        [SerializeField] private float moveForce = 30f;
        [SerializeField] private float maxVelocity = 5f;
        
        [SerializeField] private Transform headTransform; // направление взгляда

        private bool moveLeft;
        private bool moveRight;

        private void FixedUpdate()
        {
            if (moveLeft && leftHandRB)
                MoveHand(leftHandRB, headTransform.forward, leftJoint);
            if (moveRight && rightHandRB)
                MoveHand(rightHandRB, headTransform.forward, rightJoint);
        }

        private void MoveHand(Rigidbody handRB, Vector3 direction, ConfigurableJoint joint)
        {
            Vector3 targetVel = direction * maxVelocity;
            Vector3 deltaVel = targetVel - handRB.linearVelocity;
            Vector3 force = deltaVel * moveForce;
            handRB.AddForce(force, ForceMode.Acceleration);
            
            if (joint) joint.connectedBody = null;
        }

        [Server]
        public void Move(Hand hand)
        {
            if (hand == Hand.Left)
            {
                moveLeft = true;
                if (leftJoint) leftJoint.connectedBody = null;
            }
            else
            {
                moveRight = true;
                if (rightJoint) rightJoint.connectedBody = null;
            }
        }

        [Server]
        public void ResetLeftHand()
        {
            moveLeft = false;
            if (leftHandRB) leftHandRB.linearVelocity = Vector3.zero;
            if (leftJoint)
            {
                leftJoint.connectedBody = null;
                leftJoint.targetPosition = Vector3.zero;
            }
        }

        [Server]
        public void ResetRightHand()
        {
            moveRight = false;
            if (rightHandRB) rightHandRB.linearVelocity = Vector3.zero;
            if (rightJoint)
            {
                rightJoint.connectedBody = null;
                rightJoint.targetPosition = Vector3.zero;
            }
        }

        [Server]
        public void Move(Hand hand, PhysicalItem item)
        {
            // Останавливаем движение силой
            if (hand == Hand.Left)
            {
                moveLeft = false;
                if (leftHandRB) leftHandRB.linearVelocity = Vector3.zero;
                if (leftJoint)
                {
                    leftJoint.connectedBody = item.HandleType == HandleType.OneHanded ? item.UniversalPoint : item.LeftHandlPoint;
                    leftJoint.targetPosition = Vector3.zero;
                }
            }
            else
            {
                moveRight = false;
                if (rightHandRB) rightHandRB.linearVelocity = Vector3.zero;
                if (rightJoint)
                {
                    rightJoint.connectedBody = item.HandleType == HandleType.OneHanded ? item.UniversalPoint : item.RightHandPoint;
                    rightJoint.targetPosition = Vector3.zero;
                }
            }
        }
    }
}