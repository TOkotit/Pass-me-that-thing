using FishNet.Component.Animating;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.View
{
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] protected Animator animator;
        [SerializeField] protected NetworkAnimator netAnimator;
        [SerializeField] protected ParticleSystem particles;
        
        
        public void EnableAnimator() => animator.enabled = true;
        public void DisableAnimator() => animator.enabled = false;
        
        public void PlayParticles() => particles.Play();
    }
}