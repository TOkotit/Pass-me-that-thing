using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class HandsMaskLayerController : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MainCharacterView mainCharacterView;
        [SerializeField] private List<Rigidbody> handsRBs;

        private int fullBodyLayerIndex;
        private int bodyOnlyLayerIndex;

        [SyncVar(hook = nameof(OnFullBodyActiveChanged))]
        private bool fullBodyActive;

        private void Start()
        {
            fullBodyLayerIndex = animator.GetLayerIndex("FullBody");
            bodyOnlyLayerIndex = animator.GetLayerIndex("BodyOnly");
            
            if (isServer)
                SetBodyOnlyActive();
            else
                EnableBodyOnlyAnimation(); 
        }

        public void EnableFullBodyAnimation()
        {
            if (isServer)
                SetFullBodyActive();
        }

        public void EnableBodyOnlyAnimation()
        {
            if (isServer)
                SetBodyOnlyActive();
        }

        [Server]
        private void SetFullBodyActive()
        {
            fullBodyActive = true;
        }

        [Server]
        private void SetBodyOnlyActive()
        {
            fullBodyActive = false;
        }

        private void OnFullBodyActiveChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                animator.SetLayerWeight(fullBodyLayerIndex, 1f);
                animator.SetLayerWeight(bodyOnlyLayerIndex, 0f);
                foreach (Rigidbody rb in handsRBs)
                    rb.isKinematic = true;
            }
            else
            {
                animator.SetLayerWeight(bodyOnlyLayerIndex, 1f);
                animator.SetLayerWeight(fullBodyLayerIndex, 0f);
                foreach (Rigidbody rb in handsRBs)
                    rb.isKinematic = false;
            }
        }
    }
}