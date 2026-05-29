using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string id; 
    [SerializeField] private string itemName;
    [SerializeField] private GameObject worldPrefab; 
    [SerializeField] private Sprite itemImage;
    [SerializeField] private bool isStackable;

    public string Id
    {
        get => id;
        set => id = value;
    }

    public string ItemName
    {
        get => itemName;
        set => itemName = value;
    }

    public GameObject WorldPrefab
    {
        get => worldPrefab;
        set => worldPrefab = value;
    }

    public bool IsStackable
    {
        get => isStackable;
        set => isStackable = value;
    }

    public Sprite ItemImage
    {
        get => itemImage;
        set => itemImage = value;
    }
}
