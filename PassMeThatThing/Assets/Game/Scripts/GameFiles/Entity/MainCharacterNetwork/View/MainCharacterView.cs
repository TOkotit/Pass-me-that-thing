using System;
using System.Linq;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View
{
    public class MainCharacterView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _hipsBone;
        [SerializeField] private LayerMask groundMask;
        private const string IdleClipName = "Idle";
        private RigAdjusterForAnimation _rigAdjusterForReturnAnimation;

        public void Initialize()
        {
            var currentClips = _animator.runtimeAnimatorController.animationClips;
            var bones = _hipsBone.GetComponentsInChildren<Transform>();

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
            Vector3 initHipsPos = _hipsBone.position;
            _parent.position = initHipsPos;
            if (Physics.Raycast(_parent.position, Vector3.down, out RaycastHit hit, 5, groundMask))
                _parent.position = new Vector3(_parent.position.x, hit.point.y, _parent.position.z);
            _hipsBone.position = initHipsPos;
        }

        private void AdjustParentRotationToHipsBone()
        {
            Vector3 initHipsPos = _hipsBone.position;
            Quaternion initHipsRot = _hipsBone.rotation;

            Vector3 dir = _hipsBone.up;
            if (Vector3.Dot(dir, Vector3.up) < 0) dir *= -1;
            dir.y = 0;
            Quaternion correction = Quaternion.FromToRotation(_parent.forward, dir.normalized);
            _parent.rotation *= correction;

            _hipsBone.position = initHipsPos;
            _hipsBone.rotation = initHipsRot;
        }

        public void EnableAnimator() => _animator.enabled = true;
        public void DisableAnimator() => _animator.enabled = false;
    }
}