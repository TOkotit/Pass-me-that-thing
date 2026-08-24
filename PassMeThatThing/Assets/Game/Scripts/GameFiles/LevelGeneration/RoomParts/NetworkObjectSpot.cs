using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class NetworkObjectSpot : MonoBehaviour
    {
        public NetworkObjectsOnLevelType NetworkObjectsOnLevelType;
        
        [SerializeField] private NetworkObjectsDatabase _database;
        
        private void OnDrawGizmos()
        {
            if (_database == null) return;

            var prefab = _database.GetPrefab(NetworkObjectsOnLevelType);
            if (prefab == null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);

            // Включаем поиск по неактивным объектам (true)
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            var skinnedMeshes = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (meshFilters.Length == 0 && skinnedMeshes.Length == 0)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                return;
            }

            // Отрисовка обычных мешей
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                var localToPrefabMatrix = prefab.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;
                Gizmos.matrix = transform.localToWorldMatrix * localToPrefabMatrix;
                Gizmos.DrawWireMesh(mf.sharedMesh);
            }

            // Отрисовка Skinned мешей
            foreach (var smr in skinnedMeshes)
            {
                if (smr.sharedMesh == null) continue;
                var localToPrefabMatrix = prefab.transform.worldToLocalMatrix * smr.transform.localToWorldMatrix;
                Gizmos.matrix = transform.localToWorldMatrix * localToPrefabMatrix;
                Gizmos.DrawWireMesh(smr.sharedMesh);
            }
        }
    }
}