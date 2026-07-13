using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNode : Interactable
    {
        [SerializeField] private LineRenderer lineRenderer;
        
        [SerializeField] private bool isSplitter;
        [SerializeField] private int splitterConnLimit = 4;
        
        [Inject] private WireManager _wireManager;
        
        [Inject] private LocalWireHandlerModel _handlerModel;
        
        [SyncVar]
        private int _nodeId = -1;
        [SyncVar]
        private int _netId = -1;

        private bool isHighlighted;

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

        public LineRenderer LineRenderer => lineRenderer;

        private void Start()
        {
            _handlerModel.OnWireNodeHighlighted += CheckSelf;
            if (isServer)
            {
                _wireManager.RegisterNode(this);
            }
        }

        private void OnDestroy()
        {
            _handlerModel.OnWireNodeHighlighted -= CheckSelf;
            if (isServer)
            {
                _wireManager.UnRegisterNode(NodeId);
            }
        }

        private void CheckSelf(int nodeId)
        {
            isHighlighted = nodeId == NodeId;
        }

        public override void Interact()
        {
            //Debug.Log($"[W] wirenode interact");
            
            // if (NetId != -1)
            // {
            //     _handlerModel.ClearNode(NodeId);
            // }
            // else
            {
                _handlerModel.HighlightNode(NodeId);
            }
        }

        public override void SrbToggle()
        {
            
        }
    }
}