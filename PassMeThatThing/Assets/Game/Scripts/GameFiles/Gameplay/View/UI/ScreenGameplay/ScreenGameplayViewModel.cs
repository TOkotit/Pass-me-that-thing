using System;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.UI;
using Mirror;
using ObservableCollections;
using R3;
using Root;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI
{
    public class PlayerViewData
    {
        public string name;
        public Texture2D avatar;

        public PlayerViewData(string name, Texture2D avatar)
        {
            this.name = name;
            this.avatar = avatar;
        }
    }

    public class ScreenGameplayViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly CompositeDisposable _subscriptions = new();
        public override string Id => "ScreenGameplay";


        private readonly PlayerInventoryModel  _playerInventoryModel;
        private readonly ItemDatabase _itemDatabase;
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        private readonly BuildingsDatabase _buildingsDatabase;
        private readonly GlobalStageManager _globalStageManager;
        private readonly GameInputManager _gameInputManager;

        
        private readonly MCLocalModel  _mcLocalModel;
        private readonly LocalWireHandlerModel _localWireHandlerModel;
        private readonly LevelOrchestrator _levelOrchestrator;
        private readonly ConnectedPlayers _connectedPlayers;

        private Action<int, Sprite, int> addEvent;
        private Action<int, Sprite, int> updateEvent;
        private Action<int> removeEvent;

        public BuildingsDatabase BuildingsDatabase => _buildingsDatabase;

        public event Action<PlayerViewData, List<PlayerViewData>> OnPlayerDataChanged;

        
        public ScreenGameplayViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _playerInventoryModel = container.Resolve<PlayerInventoryModel>();
            _itemDatabase =  container.Resolve<ItemDatabase>();
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            _buildingsDatabase = container.Resolve<BuildingsDatabase>();

            _gameRandomEventManager =  container.Resolve<GameRandomEventManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();
            
            _mcLocalModel = container.Resolve<MCLocalModel>();
            _gameInputManager = container.Resolve<GameInputManager>();

            _localWireHandlerModel = container.Resolve<LocalWireHandlerModel>();
            _levelOrchestrator = container.Resolve<LevelOrchestrator>();
            _connectedPlayers = container.Resolve<ConnectedPlayers>();
            _connectedPlayers.OnPlayersViewDataChanged += PlayersChanged;

            _gameInputManager.GameInput.Gameplay.PauseMenu.performed += RequestOpenPause;
            _gameInputManager.GameInput.Gameplay.WireMenu.performed += RequestOpenWireMenu;

            _gameInputManager.GameInput.Gameplay.CancelBuilding.performed += CancelWirePlacement;
        }
        
        public override void Dispose()
        {
            // Debug.Log("Disposing ScreenGameplayViewModel");
            _gameInputManager.GameInput.Gameplay.PauseMenu.performed -= RequestOpenPause;
            _gameInputManager.GameInput.Gameplay.WireMenu.performed -= RequestOpenWireMenu;

            _gameInputManager.GameInput.Gameplay.CancelBuilding.performed -= CancelWirePlacement;
            _connectedPlayers.OnPlayersViewDataChanged -= PlayersChanged;
        }

        public void RequestOpenPause(InputAction.CallbackContext c)
        {
            _uiManager.OpenScreenPauseMenu();
        }
        
        public void RequestOpenWireMenu(InputAction.CallbackContext c)
        {
            _uiManager.OpenScreenWireMenu();
        }

        public void CancelWirePlacement(InputAction.CallbackContext c)
        {
            _localWireHandlerModel.CancelHighlight();
        }

        public void RequestSubPlayersInfo(Action<PlayerViewData, List<PlayerViewData>> f)
        {
            OnPlayerDataChanged += f;

            PreparePlayerInfo();
        }

        public void RequestUnsubPlayersInfo(Action<PlayerViewData, List<PlayerViewData>> f)
        {
            OnPlayerDataChanged -= f;
        }

        public void PlayersChanged(List<CustomRoomPlayer> l) => PreparePlayerInfo();

        public void PreparePlayerInfo()
        {
            var local = new PlayerViewData(_connectedPlayers.localPlayer.nameText,
                _connectedPlayers.localPlayer.avatarImage);

            var others = new List<PlayerViewData>();
            foreach (var p in _connectedPlayers.players)
            {
                //TODO переделать на нормальное
                if (p.nameText != _connectedPlayers.localPlayer.nameText)
                {
                    others.Add(new PlayerViewData(p.nameText, p.avatarImage));
                }
            }

            OnPlayerDataChanged?.Invoke(local, others);
        }

        public void RequestSubHealthUI(Action<int, int> f)
        {
            f(_mcLocalModel.Health, _mcLocalModel.MaxHealth);

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
            f(_globalStageManager.SyncRemainingTime);
            
            _globalStageManager.OnTimerChangedUI += f;
        }
        
        public void RequestUnsubGlobalStateTimer(Action<float> f)
        {
            _globalStageManager.OnTimerChangedUI -= f;
        }

        public void RequestSubActiveSlot(Action<int> f)
        {
            f(_playerInventoryModel.ActiveSlotIndex);

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

        public void RequestSubCameraRotation(Action<float> f)
        {
            _mcLocalModel.OnCameraYRotationChanged += f;
        }
        
        public void RequestUnsubCameraRotation(Action<float> f)
        {
            _mcLocalModel.OnCameraYRotationChanged -= f;
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
        
        public void UnsubInitGameEventToClient(Action<SyncDictionary<int, BaseGameEvent>> f)
        {
            _gameRandomEventManager.OnEventReceived -= f;
        }

        public void InitGameEvent(Action clear, Action<int, Sprite, int> add)
        {
            clear();
            foreach (var i in _gameRandomEventManager.StartedEvents)
            {
                var e = _gameEventsDatabase.GetEvent(i.Value.EventType);
                add(i.Value.EventId, e.EventImage, i.Value.EventId);
            }
        }
        
        
        private void OnStartedEventsChanged(SyncDictionary<int, BaseGameEvent>.Operation op, int key, BaseGameEvent newItem)
        {
            var e = _gameEventsDatabase.GetEvent(newItem.EventType);
            Debug.Log($"[EVENT GVM] {newItem.EventType} {e.GameEventType}");
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


        public void RequestSubPlugImages(Action<List<WireType>> f)
        {
            f(_localWireHandlerModel.HighlightedNodesTypes);

            _localWireHandlerModel.OnTypesChanged += f;
        }

        public void RequestUnsubPlugImages(Action<List<WireType>> f)
        {
            _localWireHandlerModel.OnTypesChanged -= f;
        }
        
        public void RequestLevelGrid(Action<LevelGrid> f)
        {
            if (_levelOrchestrator != null && _levelOrchestrator.levelGrid != null)
            {
                f?.Invoke(_levelOrchestrator.levelGrid);
            }
        }
        
        public void RequestSubPlayerPosition(Action<Vector3> f)
        {
            _mcLocalModel.OnPlayerPositionChanged += f;
        }
        
        public void RequestUnsubPlayerPosition(Action<Vector3> f)
        {
            _mcLocalModel.OnPlayerPositionChanged -= f;
        }

    }
}