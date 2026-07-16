using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNode : NetworkBehaviour, Interactable
    {
        [SerializeField] private WireType wireType;
        [SerializeField] private bool isSplitter;
        [SerializeField] private int splitterConnLimit = 4;
        
        [Inject] protected WireManager _wireManager;
        
        [Inject] private LocalWireHandlerModel _handlerModel;
        
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

        public WireType WireType => wireType;

        public virtual void Start()
        {
            if (isServer)
            {
                _wireManager.RegisterNode(this);
            }
        }

        public virtual void OnDestroy()
        {
            if (isServer)
            {
                _wireManager.UnRegisterNode(NodeId);
            }
        }

        public void Interact()
        {
            //Debug.Log($"[W] wirenode interact");
            
            _handlerModel.HighlightNode(NodeId);
        }

        public void SrbToggle()
        {
            
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}