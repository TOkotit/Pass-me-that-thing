using UnityEngine;

namespace Game.Entity.Stats
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [SerializeField] private float speed = 140f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float baseCarry = 15f;
        
        public float Speed => speed;
        public float SprintMultiplier => sprintMultiplier;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float BaseCarry => baseCarry;
    }
}