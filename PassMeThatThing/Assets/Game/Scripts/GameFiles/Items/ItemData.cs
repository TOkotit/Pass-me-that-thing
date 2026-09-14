using Assets.Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string id; 
    [SerializeField] private string itemName;
    [SerializeField] private GameObject worldPrefab;
    [SerializeField] private Sprite itemImage;
    [SerializeField] private bool isStackable;

    [Header("Control hints")]
    [SerializeField] private List<ControlHint> controlHints;
    [SerializeField] private List<UseHint> useHints;

    public string Id => id;
    public string ItemName => itemName;
    public GameObject WorldPrefab => worldPrefab;
    public bool IsStackable => isStackable;
    public Sprite ItemImage => itemImage;

    public List<ControlHint> ControlHints => controlHints;

    public List<UseHint> UseHints => useHints;
}

[Serializable]
public class ControlHint
{
    public InputActionReference bind;
    public string name;

    public ControlHint(InputActionReference bind, string name)
    {
        this.bind = bind;
        this.name = name;
    }
}

[Serializable]
public class UseHint
{
    public UseHintType useHintType;
    public string name;
}

