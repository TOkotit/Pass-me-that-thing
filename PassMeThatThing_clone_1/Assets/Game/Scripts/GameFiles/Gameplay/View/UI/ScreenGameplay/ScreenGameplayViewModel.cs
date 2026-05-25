using System;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Items;
using Game.UI;
using MainCharacter_old;
using Microsoft.Unity.VisualStudio.Editor;
using Mirror;
using ObservableCollections;
using R3;
using Systems;
using UnityEngine;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        
        private readonly PlayerInventoryModel  _playerInventoryModel;
        private readonly ItemDatabase _itemDatabase;
        private readonly GameEventManager _gameEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        public override string Id => "ScreenGameplay";

        public ScreenGameplayViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _playerInventoryModel = container.Resolve<PlayerInventoryModel>();
            _itemDatabase =  container.Resolve<ItemDatabase>();
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            
            _gameEventManager =  container.Resolve<GameEventManager>();
        }

        // public void InitHealthText(Action<int> f)
        // {
        //     Debug.Log("inithealthText");
        //     f(_mainCharacter.Health.CurrentHealth);
        // }
        //
        // public void RequestSubHealthText(Action<int> f)
        // {
        //     Debug.Log($"RequestSubText {_mainCharacter.Health == null}");
        //     _mainCharacter.Health.OnHealthChanged += f;
        // }
        //
        // public void RequestUnsubHealthText(Action<int> f)
        // {
        //     Debug.Log($"RequestUnsubText {_mainCharacter.Health == null}");
        //     _mainCharacter.Health.OnHealthChanged -= f;
        // }
        //
        // public void InitStaminaText(Action<float> f)
        // {
        //     f(_mainCharacter.Stamina_old.CurrentStamina);
        // }
        //
        // public void RequestSubStaminaText(Action<float> f)
        // {
        //     _mainCharacter.Stamina_old.OnStaminaChanged += f;
        // }
        //
        // public void RequestUnsubStaminaText(Action<float> f)
        // {
        //     _mainCharacter.Stamina_old.OnStaminaChanged -= f;
        // }

        public void RequestSubActiveSlot(Action<int> f)
        {
            _playerInventoryModel.OnActiveSlotChanged += f;
        }
        
        public void RequestUnsubActiveSlot(Action<int> f)
        {
            _playerInventoryModel.OnActiveSlotChanged -= f;
        }
        
        public void RequestSubThrowCharge(Action<int> f)
        {
            _playerInventoryModel.OnThrowChargeChanged += f;
        }
        
        public void RequestUnsubThrowCharge(Action<int> f)
        {
            _playerInventoryModel.OnThrowChargeChanged -= f;
        }
        
        public void RequestSubInteractionText(Action<bool> f)
        {
            _playerInventoryModel.OnAbleInteract += f;
        }
        
        public void RequestUnsubInteractionText(Action<bool> f)
        {
            _playerInventoryModel.OnAbleInteract -= f;
        }

        public void InitImage(Action<int, Sprite> f)
        {
            for (var i=0; i < _playerInventoryModel.Inventory.Count; i++)
            {
                f(i, _itemDatabase
                    .GetItem(_playerInventoryModel.Inventory[i].itemId).ItemImage);
            }
        }
        
        public void RequestSubImage(Action<int, Sprite> f)
        {
            _subscriptions.Add(_playerInventoryModel.Inventory.ObserveAdd()
                .Subscribe(e
                    => f(e.Value.Key, _itemDatabase.GetItem(e.Value.Value.itemId).ItemImage)));
            
            _subscriptions.Add(_playerInventoryModel.Inventory.ObserveReplace()
                .Subscribe(e
                    => f(e.NewValue.Key, _itemDatabase.GetItem(e.NewValue.Value.itemId).ItemImage)));
            
            _subscriptions.Add(_playerInventoryModel.Inventory.ObserveRemove()
                .Subscribe(e
                    => f(e.Value.Key, null)));
        }

        public void RequestUnsub()
        {
            _subscriptions.Dispose();
        }

        public void RequestSubGameEvent(Action<int, Sprite, int> add, 
            Action<int, Sprite, int> update, 
            Action<int> remove)
        {
            _gameEventManager.StartedEvents.OnChange += (SyncDictionary<int, BaseGameEvent>.Operation op, 
                int index, BaseGameEvent newItem) =>
            {
                var e = _gameEventsDatabase.GetEvent(newItem.eventType);
                switch (op)
                {
                    
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_ADD:
                        add(index, e.EventImage, newItem.RoomNumber);
                        break;
                    
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_SET:
                        update(index, e.EventImage, newItem.RoomNumber);
                        break;
        
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_REMOVE:
                        remove(index);
                        break;
                }
            };
        }
        
        public void RequestUnsubGameEvent(Action<int, Sprite, int> add, 
            Action<int, Sprite, int> update, 
            Action<int> remove)
        {
            _gameEventManager.StartedEvents.OnChange -= (SyncDictionary<int, BaseGameEvent>.Operation op, 
                int index, BaseGameEvent newItem) =>
            {
                var e = _gameEventsDatabase.GetEvent(newItem.eventType);
                switch (op)
                {
                    
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_ADD:
                        add(index, e.EventImage, newItem.RoomNumber);
                        break;
                    
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_SET:
                        update(index, e.EventImage, newItem.RoomNumber);
                        break;
        
                    case SyncDictionary<int, BaseGameEvent>.Operation.OP_REMOVE:
                        remove(index);
                        break;
                }
            };
        }
        
        
        
        // public void RequestGoToMainMenu()
        // {
        //     _coroutines.StartRoutine(_gameManager.LoadMainMenu());
        // }
    }
}