using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class InstantItemSpawn : NetworkBehaviour
    {
        [SerializeField] private ItemSpawner itemSpawner;

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!itemSpawner) return;
            itemSpawner.ServerSpawnCurrentItem();
        }
    }
}