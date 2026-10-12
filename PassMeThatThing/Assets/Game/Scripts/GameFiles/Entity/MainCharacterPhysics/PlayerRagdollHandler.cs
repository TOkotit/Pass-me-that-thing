using System.Collections.Generic;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class PlayerRagdollHandler : RagdollHandler
    {
        [SerializeField] private GameObject ragdollPrefab;
        [SerializeField] private MainCharacter player;
        [SerializeField] private SkinnedMeshRenderer playerMeshRenderer;

        [SyncVar(hook = nameof(OnRagdollIdentityChanged))]
        private NetworkIdentity _ragdollIdentity;

        private PlayerNetworkRagdoll _ragdollInstance;
        private Dictionary<string, Rigidbody> _playerBoneDict;

        private void Awake()
        {
            _playerBoneDict = new Dictionary<string, Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                if (rb && !_playerBoneDict.ContainsKey(rb.name))
                    _playerBoneDict.Add(rb.name, rb);
            }
        }

        private void Start()
        {
            if (!NetworkServer.active) return;
            Setup();
        }

        private void OnRagdollIdentityChanged(NetworkIdentity oldVal, NetworkIdentity newVal)
        {
            if (!newVal)
            {
                _ragdollInstance = null;
                return;
            }

            _ragdollInstance = newVal.GetComponent<PlayerNetworkRagdoll>();

            if (_ragdollInstance && player)
                _ragdollInstance.Setup(player, _playerBoneDict);
        }

        private void Setup()
        {
            if (!NetworkServer.active) return;
            var go = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            var physItem = go.GetComponentInChildren<PhysicalItem>(true);
            if (physItem && physItem.Network && string.IsNullOrEmpty(physItem.Network.itemId))
            {
                physItem.Network.itemId = "ragdoll";
                physItem.Network.instanceId = System.Guid.NewGuid().ToString();
            }
            NetworkServer.Spawn(go);
            _ragdollInstance = go.GetComponent<PlayerNetworkRagdoll>();
            _ragdollIdentity = go.GetComponent<NetworkIdentity>();
            if (player) _ragdollInstance.Setup(player, _playerBoneDict);
        }

        public override void EnableRagdoll()
        {
            if (!_ragdollInstance)
            {
                Debug.LogWarning($"[RagdollHandler] {name}: _ragdollInstance == null " +
                                 $"(isServer={isServer})");
                return;
            }

            _ragdollInstance.EnableRagdoll();
            DisablePlayer();
        }

        public override void DisableRagdoll()
        {
            _ragdollInstance?.DisableRagdoll();
            EnablePlayer();
        }

        private void DisablePlayer()
        {
            foreach (var rb in rigidbodies)
            {
                rb.gameObject.SetActive(false);
            }
            playerMeshRenderer.enabled = false;
        }

        private void EnablePlayer()
        {
            foreach (var rb in rigidbodies)
            {
                rb.gameObject.SetActive(true);
            }
            playerMeshRenderer.enabled = true;
        }
    }
}