using UnityEngine;

[CreateAssetMenu(fileName = "TurretData", menuName = "Scriptable Objects/TurretData")]
public class TurretData : ScriptableObject
{
    //id такой же, как в building data
    public string id; 
    
    [Header("turret stats")]
    public float damage;
    public float attackSpeed;

}
