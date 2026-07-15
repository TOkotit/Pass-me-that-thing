using System;
using System.Collections.Generic;
using Mirror;
using Game.Scripts.Enums;
using UnityEngine;

public class ResourceStorage : NetworkBehaviour
{
    private static Dictionary<GameObject, ResourceStorage> storages = new Dictionary<GameObject, ResourceStorage>();
    private readonly SyncDictionary<Resource, int> storedResources = new SyncDictionary<Resource, int>();
    public static Dictionary<GameObject, ResourceStorage> Storages => storages;
    public IReadOnlyDictionary<Resource, int> StoredResources => storedResources;
    [Server]
    public void AddResource(Resource resource, int amount)
    {
        if (storedResources.TryGetValue(resource, out int current))
            storedResources[resource] = current + amount;
        else
            storedResources.Add(resource, amount);
        PrintResources();
    }

    [Server]
    public bool RemoveResource(Resource resource, int amount)
    {
        if (!storedResources.TryGetValue(resource, out int current)) return false;
        int newAmount = current - amount;
        if (newAmount < 0) return false;
        if (newAmount == 0) storedResources.Remove(resource);
        else storedResources[resource] = newAmount;
        PrintResources();
        return true;
    }

    public bool HasResource(Resource resource, int amount)
    {
        if (!storedResources.TryGetValue(resource, out int current)) return false;
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