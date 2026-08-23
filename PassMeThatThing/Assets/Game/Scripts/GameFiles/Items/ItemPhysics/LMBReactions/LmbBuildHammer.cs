
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbBuildHammer : ItemReaction
    {
        [Inject] private LocalBuildingHandlerModel localBuildingHandlerModel;

        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbBuildHammer)}");
            localBuildingHandlerModel.DestroyBuilding();
        }
    }
}