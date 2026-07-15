using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    [CreateAssetMenu(fileName = "ItemRecipe", menuName = "Recipes") ]
    public class WorkbenchItemRecipe : ScriptableObject
    {
        public string recipeId;
        [SerializeField] private List<ResourcePair> resources;
        [SerializeField] private ItemData item;
        
        public List<ResourcePair> Resources => resources;
        public ItemData Item => item;
    }
    
    [Serializable]
    public class ResourcePair
    {
        public int amount;
        public Resource resource;
    }
}