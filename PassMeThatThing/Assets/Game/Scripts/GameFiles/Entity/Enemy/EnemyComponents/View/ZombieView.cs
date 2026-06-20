using System;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class ZombieView : EnemyView
    {
        [SerializeField] protected LayerMask groundMask;
        
        private const string WalkKey = "Walk";
        private const string Attack1Key = "Attack1";
        private const string Attack2Key = "Attack2";
        private const string DeathKey = "Death";

        public void Walk() => base.animator.SetTrigger(WalkKey);
        public void Attack1() => base.animator.SetTrigger(Attack1Key);
        public void Attack2() => base.animator.SetTrigger(Attack2Key);
        public void Death() => base.animator.SetTrigger(DeathKey);
        
        
        //
        
        // private const string BackStandUpClipName = "BackStandUp";
        // private const string FrontStandClipName = "FrontStandUp";

        private const int DefaultLayer = -1;

        private RigAdjusterForAnimation _rigAdjusterForBackStandingUpAnimation;
        private RigAdjusterForAnimation _rigAdjusterForFrontStandingUpAnimation;

        [SerializeField] Transform _parent;
        [SerializeField] private Transform _hipsBone;

        private Action _standingUpCallback;

        private bool IsFrontUp => Vector3.Dot(_hipsBone.up, Vector3.up) > 0;

        public void Initialize(Transform parent)
        {
            // _parent = parent;
            // _hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);

            // var currentClips = animator.runtimeAnimatorController.animationClips;
            // var bones = _hipsBone.GetComponentsInChildren<Transform>();

            // _rigAdjusterForBackStandingUpAnimation = new RigAdjusterForAnimation(currentClips.First(clip => clip.name == BackStandUpClipName), bones, this);
            // _rigAdjusterForFrontStandingUpAnimation = new RigAdjusterForAnimation(currentClips.First(clip => clip.name == FrontStandClipName), bones, this);
        }

        public void PlayStandingUp(Action adjustAnimationEndedCallback = null, Action animationEndedCallback = null)
        {
            AdjustParentRotationToHipsBone();
            AdjustParentPositionToHipsBone();

            // _standingUpCallback = animationEndedCallback;
            //
            // if (IsFrontUp)
            //     _rigAdjusterForFrontStandingUpAnimation.Adjust(() => CallbackForAdjustStandingUpAnimation(FrontStandClipName, adjustAnimationEndedCallback));
            // else
            //     _rigAdjusterForBackStandingUpAnimation.Adjust(() => CallbackForAdjustStandingUpAnimation(BackStandUpClipName, adjustAnimationEndedCallback));
        }

        public void OnStandingUpAnimationEnded() => _standingUpCallback?.Invoke();

        private void CallbackForAdjustStandingUpAnimation(string clipName, Action additionalCallback)
        {
            additionalCallback?.Invoke();
            animator.Play(clipName, DefaultLayer, 0f);
        }

        private void AdjustParentPositionToHipsBone()
        {
            var initHipsPosition = _hipsBone.position;
            _parent.position = initHipsPosition;

            AdjustParentPositionRelativeGround();

            _hipsBone.position = initHipsPosition;
        }

        private void AdjustParentPositionRelativeGround()
        {
            if (Physics.Raycast(_parent.position, Vector3.down, out RaycastHit hit, 5, groundMask))
                _parent.position = new Vector3(_parent.position.x, hit.point.y, _parent.position.z);
        }

        private void AdjustParentRotationToHipsBone()
        {
            var initHipsPosition = _hipsBone.position;
            var initHipsRotation = _hipsBone.rotation;

            var directionForRotate = _hipsBone.up;

            if (IsFrontUp == false)
                directionForRotate *= -1;

            directionForRotate.y = 0;

            var correctionRotation = Quaternion.FromToRotation(_parent.forward, directionForRotate.normalized);
            _parent.rotation *= correctionRotation;

            _hipsBone.position = initHipsPosition;
            _hipsBone.rotation = initHipsRotation;
        }
    }
}