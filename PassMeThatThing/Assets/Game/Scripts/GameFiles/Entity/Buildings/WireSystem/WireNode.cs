using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNode : Interactable
    {
        [SerializeField] private bool isSplitter;
        [SerializeField] private int splitterConnLimit = 4;
        
        [Inject] private WireManager _wireManager;
        
        private LocalWireHandlerModel _handlerModel;

        private bool isHighlighted;
        
        [SyncVar]
        private int _nodeId = -1;
        [SyncVar]
        private int _netId = -1;

        public int NodeId
        {
            get => _nodeId;
            set => _nodeId = value;
        }

        public int NetId
        {
            get => _netId;
            set => _netId = value;
        }

        public bool IsSplitter => isSplitter;
        public int SplitterConnLimit => splitterConnLimit;


        private void Start()
        {
            if (isServer)
            {
                _wireManager.RegisterNode(this);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                _wireManager.UnRegisterNode(NodeId);
            }
        }

        public override void Interact()
        {
            
        }

        public override void SrbToggle()
        {
            
        }
    }
}