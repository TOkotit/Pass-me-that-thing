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
        
        private void Start()
        {
            fullBodyLayerIndex = animator.GetLayerIndex("FullBody");
            bodyOnlyLayerIndex = animator.GetLayerIndex("BodyOnly");
            EnableBodyOnlyAnimation();
        }
        
        public void EnableFullBodyAnimation()
        {
            if (!isLocalPlayer) return;
            animator.SetLayerWeight(fullBodyLayerIndex, 1f);
            animator.SetLayerWeight(bodyOnlyLayerIndex, 0f);
            foreach (Rigidbody rb in handsRBs)
            {
                rb.isKinematic = true;
            }
        }

        public void EnableBodyOnlyAnimation()
        {
            if (!isLocalPlayer) return;
            animator.SetLayerWeight(bodyOnlyLayerIndex, 1f);
            animator.SetLayerWeight(fullBodyLayerIndex, 0f);
            foreach (Rigidbody rb in handsRBs)
            {
                rb.isKinematic =  false;
            }
        }
    }
}