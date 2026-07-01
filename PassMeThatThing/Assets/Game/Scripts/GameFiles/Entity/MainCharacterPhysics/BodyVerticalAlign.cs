using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    [SerializeField] private List<JointSpringPair> spineAlignJoints = new List<JointSpringPair>();
    [SerializeField] private List<JointSpringPair> otherJoints = new List<JointSpringPair>();

    [Header("Глобальный множитель жёсткости (сознание)")]
    [Range(0f, 1f)]
    [SerializeField] private float currentConsciousness = 1f;
    public bool LockConsciousness {get; set;}
    public float Consciousness { get => currentConsciousness;
        set
        {
            if (!LockConsciousness)
            {
                currentConsciousness = value;
            }
        } 
    }

    [SyncVar(hook = nameof(OnTiltChanged))]
    private Quaternion _syncTiltOffset = Quaternion.identity;
    private Quaternion tiltOffset = Quaternion.identity;

    private void OnTiltChanged(Quaternion oldValue, Quaternion newValue)
    {
        tiltOffset = newValue;
    }

    private void FixedUpdate()
    {
        // Убираем проверку – обновляем суставы всегда
        foreach (var pair in otherJoints)
        {
            if (!pair.joint) continue;

            var drive = new JointDrive
            {
                positionSpring = pair.spring * Consciousness,
                positionDamper = pair.damper * Consciousness,
                maximumForce = float.MaxValue
            };
            pair.joint.angularXDrive = drive;
            pair.joint.angularYZDrive = drive;
            pair.joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        }

        foreach (var pair in spineAlignJoints)
        {
            if (!pair.joint || !pair.joint.gameObject.activeInHierarchy) continue;
            var parent = pair.joint.connectedBody;
            if (!parent) continue;

            var effectiveSpring = pair.spring * Consciousness;
            var effectiveDamper = pair.damper * Consciousness;

            var drive = new JointDrive
            {
                positionSpring = effectiveSpring,
                positionDamper = effectiveDamper,
                maximumForce = float.MaxValue
            };
            pair.joint.angularXDrive = drive;
            pair.joint.angularYZDrive = drive;
            pair.joint.rotationDriveMode = RotationDriveMode.XYAndZ;

            if (Consciousness <= 0f)
            {
                pair.joint.angularXDrive = new JointDrive { positionSpring = 0, positionDamper = 0 };
                pair.joint.angularYZDrive = new JointDrive { positionSpring = 0, positionDamper = 0 };
                continue;
            }

            var jointTransform = pair.joint.transform;
            var projectedForward = Vector3.ProjectOnPlane(jointTransform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.ProjectOnPlane(parent.transform.forward, Vector3.up);
            if (projectedForward.sqrMagnitude < 0.01f)
                projectedForward = Vector3.forward;

            var worldTarget = Quaternion.LookRotation(projectedForward, Vector3.up);
            worldTarget = worldTarget * tiltOffset;          
            var localTarget = Quaternion.Inverse(parent.transform.rotation) * worldTarget;

            pair.joint.targetRotation = localTarget;
        }
    }

    public void SetConsciousness(float multiplier)
    {
        Consciousness = Mathf.Clamp01(multiplier);
    }

    [Server]
    public void SetTilt(Vector3 eulerAngles)
    {
        _syncTiltOffset = Quaternion.Euler(eulerAngles);
    }

    [Command]
    public void CmdSetTilt(Vector3 eulerAngles)
    {
        SetTilt(eulerAngles);
    }
    [Command]
    public void CmdSetConsciousness(float value)
    {
        Consciousness = Mathf.Clamp01(value);
    }
}