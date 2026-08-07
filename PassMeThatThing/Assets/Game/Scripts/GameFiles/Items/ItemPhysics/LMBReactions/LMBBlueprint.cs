using Game.Scripts.GameFiles.Entity.Buildings;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBBlueprint : LMBReaction
    {
        //[SerializeField] private BlueprintItem _blueprintItem;
        [SerializeField] private string _buildingId;
        [Inject] private LocalBuildingHandlerModel _localBuildingHandlerModel;

        

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBBlueprint)}");
            _localBuildingHandlerModel.StartBuildPreview(_buildingId, Item.Network.instanceId);
        }
    }
}