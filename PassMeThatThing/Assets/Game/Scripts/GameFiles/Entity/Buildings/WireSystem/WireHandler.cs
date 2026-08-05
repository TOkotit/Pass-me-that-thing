using Assets.Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Gameplay.View.UI;
using Mirror;
using System;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    /// <summary>
    /// Висит на игроке и локально выполняет команды для связи проводов в зависимости от значений модели
    /// Игрок при выделении меняет модель
    /// </summary>
    public class WireHandler : NetworkBehaviour
    {
        [Inject] private LocalWireHandlerModel _handlerModel;
        [Inject] private WireManager _wireManager;

        //[Inject] private GameplayUIManager _gameplayUIManager;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnWireNodeHighlighted += WireNodeHighlighted;
                _handlerModel.OnWireNodePairMatched += WireNodePairMatched;
                _handlerModel.OnWireNodeCleared +=  WireNodeCleared;
            }
            else
            {
                Destroy(gameObject);
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

        public void WireNodeHighlighted(int nodeId, int entryId)
        {
            
        }

        public void WireNodePairMatched(int first, int second, int firstEntryId, int secondEntryId)
        {
            _wireManager.CmdMakeConnection(first, second, firstEntryId, secondEntryId);
        }
        
        public void WireNodeCleared(int nodeId)
        {
            _wireManager.CmdClearConnectionsOfNode(nodeId);
        }
        
        
    }
}