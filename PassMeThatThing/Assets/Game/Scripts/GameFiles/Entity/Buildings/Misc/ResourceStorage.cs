using System;
using System.Collections.Generic;
using Mirror;
using Game.Scripts.Enums;
using UnityEngine;
using Game.Scripts.GameFiles.Items;

public class ResourceStorage : NetworkBehaviour
{
    private static Dictionary<GameObject, ResourceStorage> storages = new Dictionary<GameObject, ResourceStorage>();
    private readonly SyncDictionary<Resource, float> storedResources = new SyncDictionary<Resource, float>();
    public static Dictionary<GameObject, ResourceStorage> Storages => storages;
    public IReadOnlyDictionary<Resource, float> StoredResources => storedResources;



    private readonly Dictionary<Resource, float> _tempReceivedResOnStartDay = new ();
    public IReadOnlyDictionary<Resource, float> TempReceivedResOnPhase => _tempReceivedResOnStartDay;

    private readonly SyncDictionary<Resource, float> _diffReceivedResOnPhase = new();
    public IReadOnlyDictionary<Resource, float> DiffReceivedResOnPhase => _diffReceivedResOnPhase;


    public event Action<IReadOnlyDictionary<Resource, float>> OnSyncResourcesChanged;

    public event Action<Resource, float> OnAddedRes;

    public virtual void Awake()
    {
        storages[transform.gameObject] = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        storedResources.OnChange += OnStoredResDictChanged;
    }

    public override void OnStopClient()
    {
        storedResources.OnChange -= OnStoredResDictChanged;
    }

    private void OnStoredResDictChanged(SyncIDictionary<Resource, float>.Operation op, Resource r, float v)
    {
        OnSyncResourcesChanged?.Invoke(StoredResources);
    }

    [Server]
    public void AddResource(Resource resource, float amount)
    {
        if (storedResources.TryGetValue(resource, out float current))
            storedResources[resource] = current + amount;
        else
            storedResources.Add(resource, amount);

        UpdateDiffResource(resource);
        RpcAddedResChange(resource, amount);

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

        UpdateDiffResource(resource);
        RpcAddedResChange(resource, -amount);

        PrintResources();

        return true;
    }

    [Server]
    public void UpdateDiffResource(Resource r)
    {
        if (storedResources.ContainsKey(r))
        {
            if (_tempReceivedResOnStartDay.ContainsKey(r))
            {
                RecalcToDiff(r, storedResources[r] - _tempReceivedResOnStartDay[r]);
            }
            else
            {
                RecalcToDiff(r, storedResources[r]);
            }
        }
        else
        {
            if (_tempReceivedResOnStartDay.ContainsKey(r))
            {
                RecalcToDiff(r, 0 - _tempReceivedResOnStartDay[r]);
            }
        }
    }

    [Server]
    public void RecalcToDiff(Resource r, float newDiff)
    { 
        if (_diffReceivedResOnPhase.ContainsKey(r))
        {
            _diffReceivedResOnPhase[r] = newDiff;
            if (newDiff == 0) _diffReceivedResOnPhase.Remove(r);
        }
        else
        {
            if (newDiff != 0)
                _diffReceivedResOnPhase.Add(r, newDiff);
        }
        Debug.Log($"RecalcToDiff {r} {_diffReceivedResOnPhase[r]}");
    }

    //в начале дня после отдыха
    [Server]
    public void CopyStoredToTempPhaseRes()
    {
        _tempReceivedResOnStartDay.Clear();
        foreach (var r in storedResources)
        {
            _tempReceivedResOnStartDay.Add(r.Key, r.Value);
        }
    }

    //в начале дня после отдыха
    [Server]
    public void ClearDiff()
    {
        _diffReceivedResOnPhase.Clear();
    }

    [ClientRpc]
    public void RpcAddedResChange(Resource r, float v)
    {
        OnAddedRes?.Invoke(r, v);
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

}