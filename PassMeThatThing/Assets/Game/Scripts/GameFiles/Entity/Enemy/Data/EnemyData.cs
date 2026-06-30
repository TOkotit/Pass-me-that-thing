using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string id; 
    [SerializeField] private string enemyName;
    [SerializeField] private GameObject worldPrefab; 
    [SerializeField] private Sprite enemyImage;

    [Header("Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxToughness;
    [SerializeField] private int speed;
    [SerializeField] private int damage;
    [SerializeField] private int attackCooldown;
    [SerializeField] private int chaseDistance;
    [SerializeField] private int attackDistance;


    public string Id => id;
    public string EnemyName => enemyName;
    public GameObject WorldPrefab => worldPrefab;
    public Sprite EnemyImage => enemyImage;

    public int MaxHealth => maxHealth;
    public int MaxToughness => maxToughness;
    public int Speed => speed;
    public int Damage => damage;
    public int AttackCooldown => attackCooldown;
    public int ChaseDistance => chaseDistance;
    public int AttackDistance => attackDistance;

    
}
