using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.Items.ItemPhysics;

using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Items.ItemPhysics.LMBReactions
{
    public class LmbSeed : ItemReaction
    {
        [SerializeField] private PlantSeedData seedData;
        [Inject] private LocalBuildingHandlerModel localBuildingHandlerModel;

        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbSeed)}");
            localBuildingHandlerModel.SetSeed(seedData.id, _item.Network.instanceId);
        }
    }
}
