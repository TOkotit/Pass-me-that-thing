using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private EnemyData defaultEnemyData;

        public void SpawnEnemy(Vector3 pos, EnemyData enemyData=null)
        {
            if (!enemyData) enemyData = defaultEnemyData;
            var enemyInstance = Instantiate(enemyData.WorldPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
    }
}