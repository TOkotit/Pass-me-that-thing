using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class ProjectileShotReaction : ItemReaction
    {
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform firePoint;

        public override void Act()
        {
            if (!isServer) return;
            var instance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            NetworkServer.Spawn(instance);
        }
    }
}