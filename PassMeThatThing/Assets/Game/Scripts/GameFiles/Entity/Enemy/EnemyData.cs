using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string id; 
    [SerializeField] private string enemyName;
    [SerializeField] private GameObject worldPrefab; 
    [SerializeField] private Sprite enemyImage;

    [Header("Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int speed;
    [SerializeField] private int damage;
    [SerializeField] private int attackCooldown;
    [SerializeField] private int chaseDistance;
    [SerializeField] private int attackDistance;
    

    public string Id
    {
        get => id;
        set => id = value;
    }

    public string ItemName
    {
        get => enemyName;
        set => enemyName = value;
    }

    public GameObject WorldPrefab
    {
        get => worldPrefab;
        set => worldPrefab = value;
    }

    public Sprite ItemImage
    {
        get => enemyImage;
        set => enemyImage = value;
    }
}
