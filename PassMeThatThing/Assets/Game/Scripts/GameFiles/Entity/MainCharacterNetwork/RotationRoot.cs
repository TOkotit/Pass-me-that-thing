using System;
using Mirror;
using UnityEngine;

namespace MainCharacter_old
{
    public class RotationRoot : NetworkBehaviour
    {
        [SerializeField] Transform root;

        private void FixedUpdate()
        {
            transform.position = root.position;
        }
    }
}