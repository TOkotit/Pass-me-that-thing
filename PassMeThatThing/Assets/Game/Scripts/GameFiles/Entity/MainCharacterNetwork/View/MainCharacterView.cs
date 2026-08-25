using System;
using System.Linq;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Mirror;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View
{
    public class MainCharacterView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NetworkAnimator networkAnimator;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform characterRig;
        [SerializeField] private Transform hips;
        [SerializeField] private LayerMask groundMask;
        private const string IdleClipName = "walk";
        private RigAdjusterForAnimation _rigAdjusterForReturnAnimation;

        private void Update()
        {
            /*
            Debug.LogWarning(animator);
            if (animator)
            {
                Debug.LogWarning($"Animator enabled: {animator.enabled}, state: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}, speed: {animator.GetFloat("Speed")}");
            }
            */
        }

        public void Initialize()
        {
            var currentClips = animator.runtimeAnimatorController.animationClips;
            var bones = characterRig.GetComponentsInChildren<Transform>();

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
            var initHipsPos = hips.position;
            parent.position = initHipsPos;
            if (Physics.Raycast(hips.position, Vector3.down, out RaycastHit hit, 5, groundMask))
                parent.position = new Vector3(parent.position.x, hit.point.y, parent.position.z);
            characterRig.position = initHipsPos;
        }

        private void AdjustParentRotationToHipsBone()
        {
            var initHipsPos = characterRig.position;
            var initHipsRot = characterRig.rotation;

            var dir = characterRig.up;
            if (Vector3.Dot(dir, Vector3.up) < 0) dir *= -1;
            dir.y = 0;
            var correction = Quaternion.FromToRotation(parent.forward, dir.normalized);
            parent.rotation *= correction;

            characterRig.position = initHipsPos;
            characterRig.rotation = initHipsRot;
        }

        public void EnableAnimator()
        {
            animator.enabled = true;
            networkAnimator.enabled = true;
        }

        public void DisableAnimator()
        {
            animator.enabled = false;
            networkAnimator.enabled = false;
        }
    }
}