using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Scriptable Objects/ResourceDatabase")]
public class ResourceDatabase : ScriptableObject
{
    public List<ResourceData> allResources;
    
    public ResourceData GetResource(Resource resourceType)
    {
        return allResources.Find(b => b.resourceType == resourceType);
    }
}
