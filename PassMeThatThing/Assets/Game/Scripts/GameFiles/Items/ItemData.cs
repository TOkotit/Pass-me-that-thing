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

    [Header("Misc")]
    [SerializeField] private bool isStackable;
    [SerializeField] private bool canAppearInTask;

    [Header("Control hints")]
    [SerializeField] private List<ControlHint> controlHints;

    public string Id => id;
    public string ItemName => itemName;
    public GameObject WorldPrefab => worldPrefab;
    public bool IsStackable => isStackable;
    public Sprite ItemImage => itemImage;

    public List<ControlHint> ControlHints => controlHints;

    public bool CanAppearInTask => canAppearInTask;
}

[Serializable]
public class ControlHint
{
    public UseHintType useHintIcon1;
    public UseHintType useHintIcon2;
    public UseHintType useHintIcon3;
    public InputActionReference bind;
    public string name;
}

