using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterDirectory
{
    public interface IControllable
    {
        void Move(Vector3 direction);
        void Rotate(Quaternion rotation);
        void Jump();
        void SetSprinting(bool isSprinting);
        void LockUpMovement();
        void UnlockMovement();
        void DisableController();
        void EnableController();
        Vector3 GetCurrentVelocity();
        Vector3 LastVelocity { get; }
    }
}