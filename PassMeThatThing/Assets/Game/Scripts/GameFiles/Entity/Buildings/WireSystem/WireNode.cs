using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNode : NetworkBehaviour, Interactable
    {
        [SerializeField] private WireType wireType;

        [SerializeField] private int connLimit = 2;
        [SerializeField] private List<GameObject> portObjects;

        [SerializeField] private bool isSplitter;

        
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

        public WireType WireType => wireType;

        public int ConnLimit => connLimit;

        public List<GameObject> PortObjects => portObjects;

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
            
            _handlerModel.HighlightNode(NodeId, this);
        }

        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {
            
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}