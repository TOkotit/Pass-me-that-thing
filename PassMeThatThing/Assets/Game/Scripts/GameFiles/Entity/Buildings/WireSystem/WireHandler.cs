using Mirror;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireHandler : NetworkBehaviour
    {
        [Inject] private LocalWireHandlerModel _handlerModel;
        [Inject] private WireManager _buildingManager;
        
        public WireNode highlightedNode;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnWireNodeHighlighted += WireNodeHighlighted;
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnWireNodeHighlighted -= WireNodeHighlighted;
            }
        }

        public void WireNodeHighlighted(WireNode node)
        {
            
        }
        
        
    }
}