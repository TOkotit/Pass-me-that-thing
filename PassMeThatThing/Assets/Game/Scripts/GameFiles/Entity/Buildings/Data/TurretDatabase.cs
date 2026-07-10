using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretDatabase", menuName = "Scriptable Objects/TurretDatabase")]
public class TurretDatabase : ScriptableObject
{
    public List<TurretData> allTurrets;
    
    public TurretData GetTurret(string id)
    {
        return allTurrets.Find(b => b.id == id);
    }
}
