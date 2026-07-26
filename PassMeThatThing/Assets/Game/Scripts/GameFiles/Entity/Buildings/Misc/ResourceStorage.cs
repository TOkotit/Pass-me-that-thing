using System;
using System.Collections.Generic;
using Mirror;
using Game.Scripts.Enums;
using UnityEngine;

public class ResourceStorage : NetworkBehaviour
{
    private static Dictionary<GameObject, ResourceStorage> storages = new Dictionary<GameObject, ResourceStorage>();
    private readonly SyncDictionary<Resource, float> storedResources = new SyncDictionary<Resource, float>();
    public static Dictionary<GameObject, ResourceStorage> Storages => storages;
    public IReadOnlyDictionary<Resource, float> StoredResources => storedResources;
    [Server]
    public void AddResource(Resource resource, float amount)
    {
        if (storedResources.TryGetValue(resource, out float current))
            storedResources[resource] = current + amount;
        else
            storedResources.Add(resource, amount);
        PrintResources();
    }

    [Server]
    public bool RemoveResource(Resource resource, float amount)
    {
        if (!storedResources.TryGetValue(resource, out float current)) return false;
        float newAmount = current - amount;
        if (newAmount < 0) return false;
        if (newAmount == 0) storedResources.Remove(resource);
        else storedResources[resource] = newAmount;
        PrintResources();
        return true;
    }

    public bool HasResource(Resource resource, float amount)
    {
        if (!storedResources.TryGetValue(resource, out float current)) return false;
        return current - amount >= 0;
    }

    private void PrintResources()
    {
        foreach (var pair in storedResources)
        {
            Debug.Log(pair.Key + ": " + pair.Value);
        }
    }

    private void Awake()
    {
        storages[transform.gameObject] = this;
    }
}