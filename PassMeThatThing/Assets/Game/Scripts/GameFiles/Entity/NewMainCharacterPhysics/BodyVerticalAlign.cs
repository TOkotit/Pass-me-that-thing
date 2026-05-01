using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[Serializable]
public struct JointSpringPair
{
    public ConfigurableJoint joint;
    public float spring;  
    public float damper;  
}

public class BodyVerticalAlign : NetworkBehaviour
{
    [Header("Список суставов для выравнивания")]
    [SerializeField] private List<JointSpringPair> joints = new List<JointSpringPair>();

    [Header("Глобальный множитель жёсткости")]
    [Range(0f, 1f)]
    [SerializeField] private float globalSpringMultiplier = 1f;
    private Quaternion tiltOffset = Quaternion.identity;

    private void FixedUpdate()
    {
        if (!isServer) return;
        if (globalSpringMultiplier <= 0f) return;

        foreach (var pair in joints)
        {
            if (!pair.joint || !pair.joint.gameObject.activeInHierarchy) continue;
            Rigidbody parent = pair.joint.connectedBody;
            if (!parent) continue;

            Transform jointTransform = pair.joint.transform;
            Vector3 projectedForward = Vector3.ProjectOnPlane(jointTransform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.ProjectOnPlane(parent.transform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.forward;

            Quaternion worldTarget = Quaternion.LookRotation(projectedForward, Vector3.up);
            worldTarget = worldTarget * tiltOffset;   
            Quaternion localTarget = Quaternion.Inverse(parent.transform.rotation) * worldTarget;

            float effectiveSpring = pair.spring * globalSpringMultiplier;
            float effectiveDamper = pair.damper * globalSpringMultiplier;

            JointDrive drive = new JointDrive
            {
                positionSpring = effectiveSpring,
                positionDamper = effectiveDamper,
                maximumForce = float.MaxValue
            };
            pair.joint.angularXDrive = drive;
            pair.joint.angularYZDrive = drive;
            pair.joint.targetRotation = localTarget;
            pair.joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        }
    }

    [Server]
    public void SetGlobalMultiplier(float multiplier)
    {
        globalSpringMultiplier = Mathf.Clamp01(multiplier);
    }

    
    [Server]
    public void SetTilt(Vector3 eulerAngles)
    {
        tiltOffset = Quaternion.Euler(eulerAngles);
    }

    [Command]
    public void CmdSetTilt(Vector3 eulerAngles)
    {
        SetTilt(eulerAngles);
    }
}