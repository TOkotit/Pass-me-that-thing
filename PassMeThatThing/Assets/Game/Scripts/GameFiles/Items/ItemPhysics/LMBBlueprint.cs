using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Events.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBBlueprint : LMBReaction
    {
        private PhysicalItem _item;
        private BlueprintItem _blueprintItem;
        private string _buildingId;
        
        public LMBBlueprint(PhysicalItem item) : base(item)
        {
            if (item.TryGetComponent<BlueprintItem>(out _blueprintItem))
            {
                _item = item;
                _buildingId = _blueprintItem.BuildingId;
            }
        }

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBBlueprint)}");
            
            _blueprintItem.LocalBuildingHandlerModel.StartBuildPreview(_buildingId, _item.Network.instanceId);
        }

        public override void CollisionEnter(Collision other)
        {
            
        }
    }
}