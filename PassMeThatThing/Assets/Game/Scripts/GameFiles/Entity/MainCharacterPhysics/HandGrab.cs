using System;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class HandGrab : NetworkBehaviour
    {
        [SerializeField] private Hand hand;
        [SerializeField] private PhysicalItemInteractionController interactionController;
        [SerializeField] private ConfigurableJoint joint;
        [SerializeField] private Transform handTransform;
        [SerializeField] private Collider handCollider;
        private void OnTriggerEnter(Collider other)
        {
            var item = interactionController.CurrentHeldItem;
            if (item.Collider == other)
            {
                var point = other.ClosestPoint(transform.position);
                var normal = (point - handCollider.bounds.center).normalized;
                LockMovement();
            }
        }
        private void LockMovement()
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        private void UnlockMovement()
        {
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }
    }
}