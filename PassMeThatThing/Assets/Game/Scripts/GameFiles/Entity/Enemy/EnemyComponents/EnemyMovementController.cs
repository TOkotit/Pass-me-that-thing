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
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.position);
        }

        [Server]
        public void StopNavigating()
        {
            navMeshAgent.isStopped = true;
        }

        [Server]
        public void SetSpeed(float speed)
        {
            navMeshAgent.speed = speed;
        }

        [Server]
        public void RotateTo(Vector3 target, float rotationSpeed=1f)
        {
            var direction = (target - transform.position).normalized;
        
            direction.y = 0;
        
            var lookRotation = Quaternion.LookRotation(direction);

            while (Quaternion.Angle(lookRotation, transform.rotation) >= 5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    lookRotation, 
                    Time.fixedDeltaTime * rotationSpeed);
            }
        }
    }
}