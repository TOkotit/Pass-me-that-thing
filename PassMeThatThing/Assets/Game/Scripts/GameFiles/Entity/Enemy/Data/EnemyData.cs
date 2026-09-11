using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
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

    [Header("Difficulty")]
    [SerializeField] private EnemyDifficulty enemyDifficulty;

    [Header("Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxToughness;
    [SerializeField] private int damage;
    [SerializeField] private int attackCooldown;
    [SerializeField] private Vector3 attackSphereArea;
    [SerializeField] private int chaseDistance;
    [SerializeField] private int attackDistance;
    [SerializeField] private float wanderTime;
    [SerializeField] private float waitTime;

    [Header("Speed")]
    [SerializeField] private int speed;
    [SerializeField] private float chaseSpeedMult;
    [SerializeField] private float wanderSpeedMult;

    [Header("Drops")]
    [SerializeField] private SerializedDictionary<ItemRarityData, float> drops; //item - base chance to this enemy


    public string Id => id;
    public string EnemyName => enemyName;
    public GameObject WorldPrefab => worldPrefab;
    public Sprite EnemyImage => enemyImage;

    public EnemyDifficulty EnemyDifficulty => enemyDifficulty;

    public int MaxHealth => maxHealth;
    public int MaxToughness => maxToughness;
    
    public int Damage => damage;
    public int AttackCooldown => attackCooldown;
    public Vector3 AttackSphereArea => attackSphereArea;
    public int ChaseDistance => chaseDistance;
    public int AttackDistance => attackDistance;

    public float WanderTime => wanderTime;
    public float WaitTime => waitTime;

    public int Speed => speed;
    public float ChaseSpeedMult => chaseSpeedMult;
    public float WanderSpeedMult => wanderSpeedMult;


    public SerializedDictionary<ItemRarityData, float> Drops  => drops;

}
