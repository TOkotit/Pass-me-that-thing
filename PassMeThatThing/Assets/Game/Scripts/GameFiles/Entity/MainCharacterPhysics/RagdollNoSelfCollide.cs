using System;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    [RequireComponent(typeof(Collider))]
    public class RagdollNoSelfCollide : MonoBehaviour
    {
        [SerializeField] private List<Transform> ToDisable;

        private void Start()
        {
            var col = GetComponent<Collider>();
            foreach (Transform t in ToDisable)
            {
                var currentCol = t.GetComponent<Collider>();
                Physics.IgnoreCollision(col, currentCol, true);
                Debug.Log(t.name);
            }
        }
    }
    
}