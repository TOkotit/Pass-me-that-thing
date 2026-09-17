using System;
using System.Collections.Generic;
using ObservableCollections;


namespace Game.Scripts.GameFiles.Items
{
    public class PlayerInventoryModel
    {
        private readonly ObservableDictionary<int, ItemSlot> _inventory = new ();
        private int _activeSlotIndex;

        private int _throwCharge;

        private List<UseHint> _sceneUseHints = new();
        private List<UseHint> _itemUseHints = new();
        private List<ControlHint> _sceneControlHints = new();
        private List<ControlHint> _itemControlHints = new();

        public ObservableDictionary<int, ItemSlot> Inventory => _inventory;

        public int ActiveSlotIndex
        {
            get => _activeSlotIndex;
            set
            {
                if (value != _activeSlotIndex) 
                    OnActiveSlotChanged?.Invoke(value);
                _activeSlotIndex = value;
            }
        }

        public int ThrowCharge
        {
            get => _throwCharge;
            set
            {
                if (value != _throwCharge) 
                    OnThrowChargeChanged?.Invoke(value);
                _throwCharge = value;
            }
        }

        public List<UseHint> SceneUseHints { get => _sceneUseHints; set => _sceneUseHints = value; }
        public List<UseHint> ItemUseHints { get => _itemUseHints; set => _itemUseHints = value; }
        public List<ControlHint> SceneControlHints { get => _sceneControlHints; set => _sceneControlHints = value; }
        public List<ControlHint> ItemControlHints { get => _itemControlHints; set => _itemControlHints = value; }

        
        public event Action<int> OnActiveSlotChanged;
        
        public event Action<int> OnThrowChargeChanged;

        public event Action OnHintChanged;

        

        public void HintsChanged()
        {
            OnHintChanged?.Invoke();
        }
    }
}