using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.Buildings.Turrets;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string id; 
    public string buildingName;
    public GameObject worldPrefab; 
    public Sprite buildingImage;
    public GameObject previewPrefab;

    [Header("Building Stats")]
    public int maxHealth;
}
