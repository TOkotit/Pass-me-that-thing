using System;
using FishNet.Object.Synchronizing;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GlobalStageManager;
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
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        private readonly GlobalStageManager _globalStageManager;
        
        private readonly MCLocalModel  _mcLocalModel;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        public override string Id => "ScreenGameplay";
        
        
        private Action<int, Sprite, int> addEvent;
        private Action<int, Sprite, int> updateEvent;
        private Action<int> removeEvent;

        public ScreenGameplayViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _playerInventoryModel = container.Resolve<PlayerInventoryModel>();
            _itemDatabase =  container.Resolve<ItemDatabase>();
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            
            _gameRandomEventManager =  container.Resolve<GameRandomEventManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();
            
            _mcLocalModel = container.Resolve<MCLocalModel>();
        }

        public void InitHealthUI(Action<int, int> f)
        {
            f(_mcLocalModel.Health, _mcLocalModel.MaxHealth);
        }
        
        public void RequestSubHealthUI(Action<int, int> f)
        {
            _mcLocalModel.OnHealthChanged += f;
        }
        
        public void RequestUnsubHealthUI(Action<int, int> f)
        {
            _mcLocalModel.OnHealthChanged -= f;
        }

        public void RequestSubDeathUI(Action<bool> f)
        {
            _mcLocalModel.OnDeathChanged += f;
        }
        
        public void RequestUnsubDeathUI(Action<bool> f)
        {
            _mcLocalModel.OnDeathChanged -= f;
        }

        public void RequestSubGlobalState(Action<GlobalStagesType> f)
        {
            f(_globalStageManager.CurrentGameStage);
    
            _globalStageManager.OnStageChangedUI += f;
        }
        
        public void RequestUnsubGlobalState(Action<GlobalStagesType> f)
        {
            _globalStageManager.OnStageChangedUI -= f;
        }
        
        public void RequestSubGlobalStateTimer(Action<float> f)
        {
            _globalStageManager.OnTimerChangedUI += f;
        }
        
        public void RequestUnsubGlobalStateTimer(Action<float> f)
        {
            _globalStageManager.OnTimerChangedUI -= f;
        }

        public void InitActiveSlot(Action<int> f)
        {
            f(_playerInventoryModel.ActiveSlotIndex);
        }
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
            foreach (var p in _playerInventoryModel.Inventory)
            {
                f(p.Key, _itemDatabase
                    .GetItem(p.Value.itemId).ItemImage);
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
            _subscriptions.Clear();
        }

        public void InitGameEventToClient(Action<GameEventsDatabase> setupEventDatabase, Action<SyncDictionary<int, BaseGameEvent>> f)
        {
            setupEventDatabase(_gameEventsDatabase);
            _gameRandomEventManager.OnEventReceived += f;
        }
        
        public void UnInitGameEventToClient(Action<SyncDictionary<int, BaseGameEvent>> f)
        {
            _gameRandomEventManager.OnEventReceived -= f;
        }

        public void InitGameEvent(Action clear, Action<int, Sprite, int> add)
        {
            clear();
            foreach (var i in _gameRandomEventManager.StartedEvents)
            {
                var e = _gameEventsDatabase.GetEvent(i.Value.eventType);
                add(i.Value.EventId, e.EventImage, i.Value.EventId);
            }
        }
        
        
        private void OnStartedEventsChanged(SyncDictionary<int, BaseGameEvent>.Operation op, int key, BaseGameEvent newItem)
        {
            var e = _gameEventsDatabase.GetEvent(newItem.eventType);
            switch (op)
            {
                case SyncDictionary<int, BaseGameEvent>.Operation.OP_ADD:
                    addEvent(newItem.EventId, e.EventImage, newItem.EventId);
                    break;
                case SyncDictionary<int, BaseGameEvent>.Operation.OP_SET:
                    updateEvent(newItem.EventId, e.EventImage, newItem.EventId);
                    break;
                case SyncDictionary<int, BaseGameEvent>.Operation.OP_REMOVE:
                    removeEvent(newItem.EventId);
                    break;
            }
        }


        public void RequestSubGameEvent(Action<int, Sprite, int> add, 
            Action<int, Sprite, int> update, 
            Action<int> remove)
        {
            addEvent = add;
            updateEvent = update;
            removeEvent = remove;
            _gameRandomEventManager.StartedEvents.OnChange += OnStartedEventsChanged;
        }
        
        public void RequestUnsubGameEvent(Action<int, Sprite, int> add, 
            Action<int, Sprite, int> update, 
            Action<int> remove)
        {
            _gameRandomEventManager.StartedEvents.OnChange -= OnStartedEventsChanged;
        }
        
        
        
        // public void RequestGoToMainMenu()
        // {
        //     _coroutines.StartRoutine(_gameManager.LoadMainMenu());
        // }
    }
}