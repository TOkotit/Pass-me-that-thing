using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings
{
    public class PreviewCollisionHandler : MonoBehaviour
    {
        [SerializeField] private List<Renderer> renderers;

        [SerializeField] private Vector3 boxHalfExtends;
        [SerializeField] private Transform boxCenter;

        public Vector3 BoxHalfExtends => boxHalfExtends;

        public Transform BoxCenter => boxCenter;

        public List<Renderer> Renderers => renderers;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCenter.position, boxHalfExtends * 2);
        }
    }
}