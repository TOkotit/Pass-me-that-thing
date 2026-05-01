using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace Game.Scripts.GameFiles.Items
{
    public class PlayerInventoryModel
    {
        private readonly ObservableDictionary<int, ItemSlot> _inventory = new ();
        private int _activeSlotIndex;

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
        
        public event Action<int> OnActiveSlotChanged;
    }
}