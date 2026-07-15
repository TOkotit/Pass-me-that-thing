using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft
{
    public class CraftHandler : NetworkBehaviour
    {
        [Inject] private LocalCraftModel _localCraftModel;
        [Inject] private CraftManager _manager;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _localCraftModel.OnCraft += Craft;
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _localCraftModel.OnCraft -= Craft;
            }
        }

        public void Craft(string recipeId, Workbench workbench)
        {
            _manager.CmdCraft(workbench, recipeId);
        }
    }
}