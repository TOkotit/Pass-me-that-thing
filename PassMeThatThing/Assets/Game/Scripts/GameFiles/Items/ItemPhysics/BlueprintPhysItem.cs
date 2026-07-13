using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class BlueprintPhysItem : PhysicalItem
    {
        [SerializeField] private string buildingId;

        public string BuildingId
        {
            get => buildingId;
            set => buildingId = value;
        }
    }
}