using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Events.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBBlueprint : LMBReaction
    {
        private PhysicalItem _blueprintItem;
        private string _buildingId;
        public LMBBlueprint(PhysicalItem item, string buildingId) : base(item)
        {
            _blueprintItem = item;
            _buildingId = buildingId;
        }

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBBlueprint)}");
            
            _blueprintItem.LocalBuildingHandlerModel.StartBuildPreview(_buildingId);
        }

        public override void CollisionEnter(Collision other)
        {
            
        }
    }
}