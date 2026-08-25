using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class NetworkObjectSpot : MonoBehaviour
    {
        public NetworkObjectsOnLevelType NetworkObjectsOnLevelType;
        
        [SerializeField] private NetworkObjectsDatabase _database;
        [SerializeField] private Transform _spawnContainer;
        public Transform SpawnContainer => _spawnContainer != null ? _spawnContainer : transform.parent;

        private void OnDrawGizmos()
        {
            if (!_database) return;

            var prefab = _database.GetPrefab(NetworkObjectsOnLevelType);
            if (!prefab) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            var rootScaleMatrix = Matrix4x4.Scale(prefab.transform.localScale);

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            var skinnedMeshes = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (meshFilters.Length == 0 && skinnedMeshes.Length == 0)
            {
                Gizmos.matrix = transform.localToWorldMatrix * rootScaleMatrix;
                Gizmos.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }

            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                var relativeMatrix = GetMatrixRelativeToRoot(mf.transform, prefab.transform);
                Gizmos.matrix = transform.localToWorldMatrix * rootScaleMatrix * relativeMatrix;
                Gizmos.DrawWireMesh(mf.sharedMesh);
            }

            foreach (var smr in skinnedMeshes)
            {
                if (smr.sharedMesh == null) continue;
                var relativeMatrix = GetMatrixRelativeToRoot(smr.transform, prefab.transform);
                Gizmos.matrix = transform.localToWorldMatrix * rootScaleMatrix * relativeMatrix;
                Gizmos.DrawWireMesh(smr.sharedMesh);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private Matrix4x4 GetMatrixRelativeToRoot(Transform child, Transform root)
        {
            var relativeMatrix = Matrix4x4.identity;
            var current = child;

            while (current != null && current != root)
            {
                relativeMatrix = Matrix4x4.TRS(current.localPosition, current.localRotation, current.localScale) * relativeMatrix;
                current = current.parent;
            }

            return relativeMatrix;
        }

    }
}