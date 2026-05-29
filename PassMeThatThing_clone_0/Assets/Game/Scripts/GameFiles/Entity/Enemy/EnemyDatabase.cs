using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Scriptable Objects/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> allEnemies;
    
    public EnemyData GetEnemy(string id)
    {
        return allEnemies.Find(enemy => enemy.Id == id);
    }
}
