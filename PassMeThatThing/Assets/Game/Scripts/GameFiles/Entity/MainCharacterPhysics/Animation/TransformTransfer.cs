using System;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics.Animation
{
    public class TransformTransfer : MonoBehaviour
    {
        [SerializeField] Rigidbody tranferTo;
        public Rigidbody Rigidbody => tranferTo;
        private void Update()
        {
            if (!tranferTo.isKinematic) return;
            tranferTo.transform.localPosition = transform.localPosition;
            tranferTo.transform.localRotation = transform.localRotation;
        }
    }
}