using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.NewMainCharacterPhysics
{
    public class ServerCollider : NetworkBehaviour
    {
        [SerializeField] private Rigidbody bodyPart;
        [SerializeField] private GameObject colliderPrefab;
        [SerializeField] private MainCharacter player;

        [Inject] private DamagableRegistry _damagableRegistry;
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;

        private void Start()
        {
            var col = Instantiate(colliderPrefab, transform.position, transform.rotation);
            var fixedJoint = col.GetComponent<FixedJoint>();
            var itemCatcher = col.GetComponentInChildren<ItemCatcher>();
            var impactReceiver = col.GetComponent<DamagableImpactReceiver>();

            fixedJoint.connectedBody = bodyPart;
            itemCatcher.SetPlayerInteraction(player.MainCharacterModel.PlayerInteraction);
            itemCatcher.SetRegistry(_physicalItemRegistry);
            impactReceiver?.SetDamagable(player);

            _damagableRegistry.Register(col.gameObject, player);
        }
    }
}