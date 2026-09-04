using Game.Scripts.Enums;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.ItemSpawn
{
    public class NetworkRarityItemSpot : MonoBehaviour
    {
        [SerializeField] private Vector3 previewGizmosSize = new Vector3(1f,1f,1f);

        [SerializeField] private ItemRarityType constRarityType;
        [SerializeField] private int constItem;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
            Gizmos.DrawWireCube(transform.position, previewGizmosSize);
        }
    }
}