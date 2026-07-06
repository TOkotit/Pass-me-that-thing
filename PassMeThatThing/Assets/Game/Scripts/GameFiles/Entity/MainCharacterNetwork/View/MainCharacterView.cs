using System;
using System.Linq;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View
{
    public class MainCharacterView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform hipsBone;
        [SerializeField] private LayerMask groundMask;
        private const string IdleClipName = "walk";
        private RigAdjusterForAnimation _rigAdjusterForReturnAnimation;

        private void Update()
        {
            Debug.LogWarning(animator);
            if (animator)
            {
                Debug.LogWarning($"Animator enabled: {animator.enabled}, state: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}, speed: {animator.GetFloat("Speed")}");
            }

            
        }

        public void Initialize()
        {
            var currentClips = animator.runtimeAnimatorController.animationClips;
            var bones = hipsBone.GetComponentsInChildren<Transform>();

            _rigAdjusterForReturnAnimation = new RigAdjusterForAnimation(
                currentClips.First(clip => clip.name == IdleClipName),
                bones,
                this);
        }

        public void PlayStandingUp(Action onAdjustEnded = null)
        {
            AdjustParentRotationToHipsBone();
            AdjustParentPositionToHipsBone();
            _rigAdjusterForReturnAnimation.Adjust(onAdjustEnded);
        }

        private void AdjustParentPositionToHipsBone()
        {
            Vector3 initHipsPos = hipsBone.position;
            parent.position = initHipsPos;
            if (Physics.Raycast(parent.position, Vector3.down, out RaycastHit hit, 5, groundMask))
                parent.position = new Vector3(parent.position.x, hit.point.y, parent.position.z)+
                                   new Vector3(0,1f,0);
            hipsBone.position = initHipsPos;
        }

        private void AdjustParentRotationToHipsBone()
        {
            Vector3 initHipsPos = hipsBone.position;
            Quaternion initHipsRot = hipsBone.rotation;

            Vector3 dir = hipsBone.up;
            if (Vector3.Dot(dir, Vector3.up) < 0) dir *= -1;
            dir.y = 0;
            Quaternion correction = Quaternion.FromToRotation(parent.forward, dir.normalized);
            parent.rotation *= correction;

            hipsBone.position = initHipsPos;
            hipsBone.rotation = initHipsRot;
        }

        public void EnableAnimator() => animator.enabled = true;
        public void DisableAnimator() => animator.enabled = false;
    }
}