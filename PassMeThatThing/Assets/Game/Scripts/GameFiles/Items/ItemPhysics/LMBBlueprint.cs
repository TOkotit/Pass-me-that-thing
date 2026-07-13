using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Events.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBBlueprint : LMBReaction
    {
        private BlueprintPhysItem _blueprintItem;
        public LMBBlueprint(PhysicalItem item) : base(item)
        {
            if (item is BlueprintPhysItem blueprintItem)
            {
                _blueprintItem = blueprintItem;
            }
        }

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBBlueprint)}");
            
            _blueprintItem.LocalBuildingHandlerModel.StartBuildPreview(_blueprintItem.BuildingId);
        }

        public override void CollisionEnter(Collision other)
        {
            
        }
    }
}