using System;
using System.Collections.Generic;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class PlayerRagdollHandler : RagdollHandler
    {
        [SerializeField] private GameObject ragdollPrefab;
        [SerializeField] private MainCharacter player;
        [SerializeField] private SkinnedMeshRenderer playerMeshRenderer;
        private PlayerNetworkRagdoll _ragdollInstance;
        private Dictionary<string, Rigidbody> _playerBoneDict;


        private void Start()
        {
            Setup();
        }

        public override void EnableRagdoll()
        {
            if (!_ragdollInstance)
            {
                Setup();
            }
            _ragdollInstance?.EnableRagdoll();
            DisablePlayer();
        }

        private void Setup()
        {
            var go = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            _ragdollInstance = go.GetComponent<PlayerNetworkRagdoll>();
            _playerBoneDict = new Dictionary<string, Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                if (rb && !_playerBoneDict.ContainsKey(rb.name))
                    _playerBoneDict.Add(rb.name, rb);
            }
            if (player) _ragdollInstance.Setup(player, _playerBoneDict);
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