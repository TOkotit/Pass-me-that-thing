using Game.Scripts.GameFiles.Entity.Buildings;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbBlueprint : LmbReaction
    {
        [SerializeField] private string _buildingId;
        [Inject] private LocalBuildingHandlerModel _localBuildingHandlerModel;
        
        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbBlueprint)}");
            _localBuildingHandlerModel.StartBuildPreview(_buildingId, _item.Network.instanceId);
        }
    }
}