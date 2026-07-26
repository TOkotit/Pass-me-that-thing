using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View;
using Game.Scripts.GameFiles.Entity.MainCharacterPhysics.Animation;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class PlayerAnimationStateController : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MainCharacterView mainCharacterView;
        [SerializeField] private List<TransformTransfer> defaultTransfers;

        private int fullBodyLayerIndex;
        private int bodyOnlyLayerIndex;

        private void Start()
        { 
            ApplyBodyOnly();
        }

        public void ApplyBodyOnly()
        {
            foreach (TransformTransfer tt in defaultTransfers)
                tt.Rigidbody.isKinematic = false;
        }

        public void ApplyFullBody()
        {
            foreach (TransformTransfer tt in defaultTransfers)
                tt.Rigidbody.isKinematic = true;
        }

        [ClientRpc]
        public void RpcSetBodyOnly()
        {
            ApplyBodyOnly();
        }

        [ClientRpc]
        public void RpcSetFullBody()
        {
            ApplyFullBody();
        }
    }
}