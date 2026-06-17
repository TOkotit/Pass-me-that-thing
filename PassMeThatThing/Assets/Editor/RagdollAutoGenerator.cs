// RagdollGeneratorWindow.cs
// Place inside an Editor folder.
// Menu: Tools → Generate Direct Ragdoll

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class RagdollGeneratorWindow : EditorWindow
{
    private GameObject target;
    private Transform rootBone;
    private enum ColliderType { Box, Capsule }
    private ColliderType colliderType = ColliderType.Capsule;
    private float angularSpring = 1000f;
    private float angularDamper = 100f;
    private float angularMaxForce = 0f; // 0 → unlimited (float.MaxValue), >0 → limited

    private const string RAGDOLL_LAYER_NAME = "Ragdoll";

    [MenuItem("Tools/Generate Direct Ragdoll")]
    public static void ShowWindow()
    {
        GetWindow<RagdollGeneratorWindow>("Ragdoll Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ragdoll Generator", EditorStyles.boldLabel);

        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
        rootBone = (Transform)EditorGUILayout.ObjectField("Root Bone (optional)", rootBone, typeof(Transform), true);

        GUILayout.Space(5);
        colliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType);

        GUILayout.Space(5);
        GUILayout.Label("Joint Drive Settings", EditorStyles.boldLabel);
        angularSpring = EditorGUILayout.FloatField("Spring", angularSpring);
        angularDamper = EditorGUILayout.FloatField("Damper", angularDamper);
        angularMaxForce = EditorGUILayout.FloatField("Max Force (0 = unlimited)", angularMaxForce);

        GUILayout.Space(10);
        GUI.enabled = target != null;
        if (GUILayout.Button("Generate Ragdoll"))
            Generate();
        GUI.enabled = true;
    }

    private void Generate()
    {
        if (!target)
        {
            Debug.LogError("No target GameObject assigned.");
            return;
        }

        var smr = target.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!smr)
        {
            Debug.LogError("No SkinnedMeshRenderer found on target.");
            return;
        }

        if (!smr.sharedMesh)
        {
            Debug.LogError("SkinnedMeshRenderer has no mesh.");
            return;
        }

        EnsureRagdollLayer();

        var anim = target.GetComponentInChildren<Animator>();
        if (anim)
        {
            Undo.RecordObject(anim, "Disable Animator");
            anim.enabled = false;
            Debug.Log("Animator disabled.");
        }

        BuildRagdoll(smr);
    }

    private void BuildRagdoll(SkinnedMeshRenderer smr)
    {
        var mesh = smr.sharedMesh;
        var bones = smr.bones;
        if (bones == null || bones.Length == 0)
        {
            Debug.LogError("SkinnedMeshRenderer has no bones.");
            return;
        }

        // Save original bone poses
        var originalPoses = new Dictionary<Transform, Matrix4x4>();
        foreach (var bone in bones)
            if (bone)
                originalPoses[bone] = Matrix4x4.TRS(bone.localPosition, bone.localRotation, bone.localScale);

        SetBonesToBindPose(smr);

        // Collect vertices per bone (in local space)
        var boneVertices = new Dictionary<Transform, List<Vector3>>();
        for (var i = 0; i < bones.Length; i++)
            if (bones[i])
                boneVertices[bones[i]] = new List<Vector3>();

        var bindposes = mesh.bindposes;
        var verts = mesh.vertices;
        var weights = mesh.boneWeights;

        for (var v = 0; v < verts.Length; v++)
        {
            var bw = weights[v];
            var bestIndex = 0;
            var bestWeight = bw.weight0;
            if (bw.weight1 > bestWeight) { bestIndex = 1; bestWeight = bw.weight1; }
            if (bw.weight2 > bestWeight) { bestIndex = 2; bestWeight = bw.weight2; }
            if (bw.weight3 > bestWeight) { bestIndex = 3; bestWeight = bw.weight3; }
            if (bestWeight <= 0f) continue;

            var boneIndex = bestIndex switch
            {
                0 => bw.boneIndex0,
                1 => bw.boneIndex1,
                2 => bw.boneIndex2,
                3 => bw.boneIndex3,
                _ => -1
            };
            if (boneIndex < 0 || boneIndex >= bones.Length || !bones[boneIndex]) continue;

            var local = bindposes[boneIndex].MultiplyPoint3x4(verts[v]);
            boneVertices[bones[boneIndex]].Add(local);
        }

        var ragdollLayer = LayerMask.NameToLayer(RAGDOLL_LAYER_NAME);
        if (ragdollLayer == -1)
        {
            Debug.LogError($"Layer '{RAGDOLL_LAYER_NAME}' not found.");
            return;
        }

        Undo.IncrementCurrentGroup();
        var undoGroup = Undo.GetCurrentGroup();

        // Determine root bone
        var boneSet = new HashSet<Transform>(bones);
        var actualRoot = rootBone;
        if (!actualRoot)
            actualRoot = bones.FirstOrDefault(b => b && (!b.parent || !boneSet.Contains(b.parent)));
        if (!actualRoot)
            actualRoot = bones[0];

        var boneToRigidbody = new Dictionary<Transform, Rigidbody>();

        // Build drive settings: 0 maxForce → unlimited (float.MaxValue)
        var drive = new JointDrive
        {
            positionSpring = angularSpring,
            positionDamper = angularDamper,
            maximumForce = angularMaxForce > 0 ? angularMaxForce : float.MaxValue
        };

        foreach (var bone in bones)
        {
            if (!bone) continue;
            if (!boneVertices.TryGetValue(bone, out var vertsLocal) || vertsLocal.Count == 0) continue;

            // Compute AABB in local space
            var localBounds = new Bounds(vertsLocal[0], Vector3.zero);
            foreach (var v in vertsLocal)
                localBounds.Encapsulate(v);
            // Slight shrink to avoid overlaps
            localBounds.Expand(-localBounds.size.magnitude * 0.075f);

            bone.gameObject.layer = ragdollLayer;

            if (colliderType == ColliderType.Capsule)
            {
                var capsule = Undo.AddComponent<CapsuleCollider>(bone.gameObject);

                // Find the longest axis of the AABB
                Vector3 size = localBounds.size;
                float maxSize = Mathf.Max(size.x, size.y, size.z);
                int longestAxis = 0;
                if (size.y == maxSize) longestAxis = 1;
                else if (size.z == maxSize) longestAxis = 2;

                float height = maxSize;

                float other1 = size[(longestAxis + 1) % 3];
                float other2 = size[(longestAxis + 2) % 3];
                float radius = Mathf.Max(other1, other2) * 0.5f;

                capsule.radius = radius;
                capsule.height = height;
                capsule.center = localBounds.center;
                capsule.direction = longestAxis; // 0 = X, 1 = Y, 2 = Z
            }
            else
            {
                var box = Undo.AddComponent<BoxCollider>(bone.gameObject);
                box.center = localBounds.center;
                box.size = localBounds.size;
            }

            var rb = Undo.AddComponent<Rigidbody>(bone.gameObject);
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;
            boneToRigidbody[bone] = rb;

            // ConfigurableJoint for all bones except root
            if (bone != actualRoot && bone.parent && boneToRigidbody.TryGetValue(bone.parent, out var parentRb))
            {
                var joint = Undo.AddComponent<ConfigurableJoint>(bone.gameObject);
                joint.connectedBody = parentRb;

                joint.anchor = Vector3.zero;
                joint.connectedAnchor = bone.parent.InverseTransformPoint(bone.position);

                var worldAxis = (bone.position - bone.parent.position).normalized;
                joint.axis = bone.InverseTransformDirection(worldAxis);

                var boneUp = bone.up;
                Vector3.OrthoNormalize(ref worldAxis, ref boneUp);
                joint.secondaryAxis = bone.InverseTransformDirection(boneUp);

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                joint.lowAngularXLimit = new SoftJointLimit { limit = 20f };
                joint.highAngularXLimit = new SoftJointLimit { limit = 20f };
                joint.angularYLimit = new SoftJointLimit { limit = 30f };
                joint.angularZLimit = new SoftJointLimit { limit = 30f };

                joint.angularXDrive = drive;
                joint.angularYZDrive = drive;

                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.projectionDistance = 0.1f;
                joint.projectionAngle = 10f;
            }
        }

        // Restore original bone poses
        foreach (var kv in originalPoses)
        {
            var bone = kv.Key;
            var mat = kv.Value;
            bone.localPosition = mat.GetColumn(3);
            bone.localRotation = mat.rotation;
            bone.localScale = mat.lossyScale;
        }

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log($"Ragdoll created on {boneToRigidbody.Count} bones. Layer '{RAGDOLL_LAYER_NAME}'.");
    }

    private static void SetBonesToBindPose(SkinnedMeshRenderer smr)
    {
        var mesh = smr.sharedMesh;
        var bones = smr.bones;
        var bindposes = mesh.bindposes;

        for (var i = 0; i < bones.Length; i++)
        {
            if (!bones[i]) continue;
            bones[i].localPosition = bindposes[i].inverse.GetColumn(3);
            bones[i].localRotation = bindposes[i].inverse.rotation;
            bones[i].localScale = Vector3.one;
        }
    }

    private static void EnsureRagdollLayer()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");
        var layerExists = false;

        for (var i = 8; i < layersProp.arraySize; i++)
        {
            var sp = layersProp.GetArrayElementAtIndex(i);
            if (sp.stringValue == RAGDOLL_LAYER_NAME)
            {
                layerExists = true;
                break;
            }
        }

        if (!layerExists)
        {
            for (var i = 8; i < layersProp.arraySize; i++)
            {
                var sp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(sp.stringValue))
                {
                    sp.stringValue = RAGDOLL_LAYER_NAME;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Layer '{RAGDOLL_LAYER_NAME}' created.");
                    layerExists = true;
                    break;
                }
            }

            if (!layerExists)
            {
                Debug.LogError("No free user layer (8–31) available. Free one and retry.");
                return;
            }
        }

        var layerIndex = LayerMask.NameToLayer(RAGDOLL_LAYER_NAME);
        if (layerIndex >= 0)
        {
            Physics.IgnoreLayerCollision(layerIndex, layerIndex, true);
            Debug.Log($"Self-collisions disabled for layer '{RAGDOLL_LAYER_NAME}'.");
        }
    }
}