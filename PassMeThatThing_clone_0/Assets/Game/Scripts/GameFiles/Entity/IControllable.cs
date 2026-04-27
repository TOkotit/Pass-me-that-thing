
using UnityEngine;

namespace MainCharacter
{
    public interface IControllable
    {
        public void Move(Vector3 direction);
        public void Jump();
        public void Dash();
        public void Rotate(Quaternion rotation) { }
    }
}