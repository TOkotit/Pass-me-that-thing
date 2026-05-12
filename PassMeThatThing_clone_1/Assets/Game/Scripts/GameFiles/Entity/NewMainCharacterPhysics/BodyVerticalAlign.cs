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
    private bool _isLocked = false;
    
    private void FixedUpdate()
    {
        if (!isServer) return;
        
        foreach (var pair in joints)
        {
            if (!pair.joint || !pair.joint.gameObject.activeInHierarchy) continue;
            Rigidbody parent = pair.joint.connectedBody;
            if (!parent) continue;

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
            pair.joint.rotationDriveMode = RotationDriveMode.XYAndZ;
            
            if (globalSpringMultiplier <= 0f)
                continue;

            
            
            Transform jointTransform = pair.joint.transform;
            Vector3 projectedForward = Vector3.ProjectOnPlane(jointTransform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.ProjectOnPlane(parent.transform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.forward;

            Quaternion worldTarget = Quaternion.LookRotation(projectedForward, Vector3.up);
            worldTarget = worldTarget * tiltOffset;   
            Quaternion localTarget = Quaternion.Inverse(parent.transform.rotation) * worldTarget;

            
            
            pair.joint.targetRotation = localTarget;

        }
    }

    [Server]
    public void SetGlobalMultiplier(float multiplier, bool locked = false)
    {
        if (_isLocked && !locked) return;
        globalSpringMultiplier = Mathf.Clamp01(multiplier);
        
        _isLocked = locked;
    }

    
    [Server]
    public void SetTilt(Vector3 eulerAngles)
    {
        tiltOffset = Quaternion.Euler(eulerAngles);
    }
    
    [Server]
    public void EmergencyRelax()
    {
        foreach (var pair in joints)
        {
            if (!pair.joint) continue;
            JointDrive lowDrive = new JointDrive
            {
                positionSpring = 0, 
                positionDamper = 0,
                maximumForce = float.MaxValue
            };
            pair.joint.angularXDrive = lowDrive;
            pair.joint.angularYZDrive = lowDrive;
        }
    }
    
    [Command]
    public void CmdSetTilt(Vector3 eulerAngles)
    {
        SetTilt(eulerAngles);
    }
}