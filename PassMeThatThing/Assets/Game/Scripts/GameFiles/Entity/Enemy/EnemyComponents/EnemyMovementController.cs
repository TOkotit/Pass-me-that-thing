using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyMovementController : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;

        [Server]
        public void NavigateTo(Transform target)
        {
            // Debug.Log("Navigating to " + target.position);
            navMeshAgent.SetDestination(target.position);
        }

        [Server]
        public void SetSpeed(float speed)
        {
            navMeshAgent.speed = speed;
        }
    }
}