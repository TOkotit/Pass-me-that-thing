using Game.Scripts.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Scriptable Objects/ResourceData")]
public class ResourceData : ScriptableObject
{
    public Resource resourceType;
    
    public string resourceName;
    public Sprite resourceImage;
    
}
