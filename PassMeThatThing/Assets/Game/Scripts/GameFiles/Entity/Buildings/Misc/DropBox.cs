using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class DropBox : NetworkBehaviour
    {
        [SerializeField] private ResourceStorage resourceStorage;
        [SerializeField] private ResourceDepot depot;

        private void OnCollisionEnter(Collision other)
        {
            depot.OnTriggerEnter(other.collider);
        }
    }
}