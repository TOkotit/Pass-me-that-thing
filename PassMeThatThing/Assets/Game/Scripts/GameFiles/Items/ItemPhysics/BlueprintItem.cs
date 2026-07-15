using Game.Scripts.GameFiles.Entity.Buildings;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class BlueprintItem : MonoBehaviour
    {
        [SerializeField] private string buildingId;
        
        [Inject] private LocalBuildingHandlerModel _localBuildingHandlerModel;
        
        public string BuildingId
        {
            get => buildingId;
            set => buildingId = value;
        }
        
        public LocalBuildingHandlerModel LocalBuildingHandlerModel => _localBuildingHandlerModel;
    }
}