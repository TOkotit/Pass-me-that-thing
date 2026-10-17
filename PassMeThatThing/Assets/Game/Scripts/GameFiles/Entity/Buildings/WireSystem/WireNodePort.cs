using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using R3;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNodePort : WireNode
    {
        [SerializeField] private PortType portType;
        
        [SerializeField] private float availableValue;
        [SerializeField] private float requiredValue;

        private bool _isOn = true;

        public float AvailableValue => availableValue;
        public float RequiredValue => requiredValue;
        
        public bool IsOn
        {
            get => _isOn;
            set
            {
                _isOn = value;
                RecalculateNet();
            }
        }

        public PortType PortType => portType;

        public event Action<WireType, bool> OnWireNetStateChanged;

        public Subject<(WireType, bool)> OnWireStateChanged = new();

        public void OnWireNetWorkingStateChanged(bool isNetWorking)
        {
            OnWireNetStateChanged?.Invoke(WireType, isNetWorking);
            OnWireStateChanged.OnNext((WireType, isNetWorking));
        }
        
        public void RecalculateNet()
        {
            if (isServer && NetId != -1)
                _wireManager.WireNets[NetId].Recalculate();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

    }
}