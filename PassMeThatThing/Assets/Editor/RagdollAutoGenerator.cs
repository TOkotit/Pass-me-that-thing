// RagdollDirectGeneratorFixed.cs
// Place inside an Editor folder.
// Select a model with SkinnedMeshRenderer → Tools → Generate Direct Ragdoll (fixed).

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class RagdollDirectGeneratorFixed : EditorWindow
{
    private const float BOUNDS_SHRINK = 0.85f;
    private const string RAGDOLL_LAYER_NAME = "Ragdoll";

    [MenuItem("Tools/Generate Direct Ragdoll (fixed)")]
    private static void GenerateDirectRagdollFixed()
    {
        var selected = Selection.activeGameObject;
        if (!selected)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        var smr = selected.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!smr)
        {
            Debug.LogError("No SkinnedMeshRenderer found on selected object.");
            return;
        }

        if (!smr.sharedMesh)
        {
            Debug.LogError("SkinnedMeshRenderer has no mesh.");
            return;
        }

        EnsureRagdollLayerAndDisableSelfCollisions();

        var anim = selected.GetComponentInChildren<Animator>();
        if (anim)
        {
            Undo.RecordObject(anim, "Disable Animator");
            anim.enabled = false;
            Debug.Log("Animator disabled.");
        }

        GenerateFixedRagdoll(smr);
    }

    private static void GenerateFixedRagdoll(SkinnedMeshRenderer smr)
    {
        var mesh = smr.sharedMesh;
        var bones = smr.bones;
        if (bones == null || bones.Length == 0)
        {
            Debug.LogError("SkinnedMeshRenderer has no bones.");
            return;
        }

        var originalPoses = new Dictionary<Transform, Matrix4x4>();
        foreach (var bone in bones)
        {
            if (bone)
                originalPoses[bone] = Matrix4x4.TRS(bone.localPosition, bone.localRotation, bone.localScale);
        }

        SetBonesToBindPose(smr);

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

        var boneSet = new HashSet<Transform>(bones);
        var rootBone = bones.FirstOrDefault(b => b && (!b.parent || !boneSet.Contains(b.parent)));
        if (!rootBone) rootBone = bones[0];

        var boneToRigidbody = new Dictionary<Transform, Rigidbody>();

        foreach (var bone in bones)
        {
            if (!bone) continue;
            if (!boneVertices.TryGetValue(bone, out var vertsLocal) || vertsLocal.Count == 0) continue;

            var bounds = new Bounds(vertsLocal[0], Vector3.zero);
            foreach (var p in vertsLocal)
                bounds.Encapsulate(p);
            var shrink = bounds.size * (1f - BOUNDS_SHRINK) * 0.5f;
            bounds.Expand(-shrink.magnitude);

            bone.gameObject.layer = ragdollLayer;

            var box = Undo.AddComponent<BoxCollider>(bone.gameObject);
            box.center = bounds.center;
            box.size = bounds.size;

            var rb = Undo.AddComponent<Rigidbody>(bone.gameObject);
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;
            boneToRigidbody[bone] = rb;

            if (bone != rootBone && bone.parent && boneToRigidbody.TryGetValue(bone.parent, out var parentRb))
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

                joint.angularXDrive = new JointDrive { positionSpring = 0, positionDamper = 0, maximumForce = 0 };
                joint.angularYZDrive = new JointDrive { positionSpring = 0, positionDamper = 0, maximumForce = 0 };

                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.projectionDistance = 0.1f;
                joint.projectionAngle = 10f;
            }
        }

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

    private static void EnsureRagdollLayerAndDisableSelfCollisions()
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
        if (layerIndex < 0) return;
        Physics.IgnoreLayerCollision(layerIndex, layerIndex, true);
        Debug.Log($"Self-collisions disabled for layer '{RAGDOLL_LAYER_NAME}'.");
    }
}