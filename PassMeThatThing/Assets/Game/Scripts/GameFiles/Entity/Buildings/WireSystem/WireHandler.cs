using Mirror;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireHandler : NetworkBehaviour
    {
        [Inject] private LocalWireHandlerModel _handlerModel;
        [Inject] private WireManager _wireManager;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnWireNodeHighlighted += WireNodeHighlighted;
                _handlerModel.OnWireNodePairMatched += WireNodePairMatched;
                _handlerModel.OnWireNodeCleared +=  WireNodeCleared;
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnWireNodeHighlighted -= WireNodeHighlighted;
                _handlerModel.OnWireNodePairMatched -= WireNodePairMatched;
                _handlerModel.OnWireNodeCleared -=  WireNodeCleared;
            }
        }

        public void WireNodeHighlighted(int nodeId)
        {
            
        }

        public void WireNodePairMatched(int first, int second)
        {
            _wireManager.CmdMakeConnection(first, second);
        }
        
        public void WireNodeCleared(int nodeId)
        {
            _wireManager.CmdClearConnectionsOfNode(nodeId);
        }
        
        
    }
}