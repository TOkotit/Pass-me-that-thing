using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Game.Scripts.Enums;
using Game.UI;
using Mirror;
using UnityEngine;
using Assets.Game.Scripts.Utils;
using UnityEngine.UIElements;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.UI;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;


namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayBinder : WindowBinder<ScreenGameplayViewModel>
    {
        private Color selectedColor = CustomColorUtils.FromHex("1C452B");
        private Color noSelectionColor = new Color(1f, 1f, 1f, 0f);
        
        private int _activeSlotIndex = -1;

        private Dictionary<int, TemplateContainer> _gameEvents = new ();
        
        private GameEventsDatabase _gameEventsDatabase;
        
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset gameEventPrefab;
        
        
        private VisualElement _root;
        private LevelGrid _levelGrid;
        private ProgressBar _health1;
        private VisualElement _healthImage;
        private VisualElement _deathImage;
        private Label _throwChargeText;
        private List<VisualElement> _itemImages;
        private GroupBox _gameEventsContainer;
        private Label _gameGlobalStateText;
        private Label _gameGlobalStateTimerText;
        private MinimapView  _miniMap;

        private VisualElement _leftPlugSocketImage;
        private VisualElement _rightPlugSocketImage;
        private VisualElement _leftPlugImage;
        private VisualElement _rightPlugImage;
        private GroupBox _wirePlacementContainer;

        private VisualElement _localPlayerAvatar;
        private List<VisualElement> _otherPlayerAvatars = new();

        private void Awake()
        {

            _root = uiDocument.rootVisualElement;

            _health1 = _root.Q<ProgressBar>("Health1");
            _healthImage = _root.Q<VisualElement>("HealthVisualImg");
            _deathImage = _root.Q<VisualElement>("DeathVisualImg");
            _throwChargeText = _root.Q<Label>("ThrowLb");
            _itemImages = _root.Q<GroupBox>("InventoryContainer").Children().ToList();
            _gameEventsContainer = _root.Q<GroupBox>("EventsContainer");
            _gameGlobalStateText = _root.Q<Label>("PhaseLb");
            _gameGlobalStateTimerText = _root.Q<Label>("RemainingTimeLb");
            _miniMap = _root.Q<MinimapView>("Minimap");

            _leftPlugSocketImage = _root.Q<VisualElement>("LeftPlugSocketImage");
            _rightPlugSocketImage = _root.Q<VisualElement>("RightPlugSocketImage");
            _leftPlugImage = _root.Q<VisualElement>("LeftPlugImage");
            _rightPlugImage = _root.Q<VisualElement>("RightPlugImage");
            _wirePlacementContainer = _root.Q<GroupBox>("WirePlacementContainer");

            _localPlayerAvatar = _root.Q<VisualElement>("Avatar1");

            for (var i = 2; i <= 4; i++)
            {
                _otherPlayerAvatars.Add(_root.Q<VisualElement>($"Avatar{i}"));
            }
        }

        private void Start()
        {
            ViewModel.RequestSubHealthUI(UpdateCurrHealthUI);
            ViewModel.RequestSubPlayersInfo(UpdatePlayerInfo);
            ViewModel.RequestSubDeathUI(UpdateDeathUI);

            ViewModel.RequestSubActiveSlot(SetActiveItemSlot);
            
            ViewModel.InitImage(SetItemImageSprite);
            
            ViewModel.RequestSubImage(SetItemImageSprite);

            ViewModel.InitGameEvent(Clear, AddGameEvent);
            ViewModel.InitGameEventToClient(SetupEventDatabase, ReceiveEvents);
            
            ViewModel.RequestLevelGrid(SetMinimapSource);
            
            ViewModel.RequestSubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            ViewModel.RequestSubCameraRotation(UpdateMiniMapRotation);
            ViewModel.RequestSubPlayerPosition(UpdateMiniMapPosition);
            ViewModel.RequestSubThrowCharge(UpdateThrowChargeText);
            ViewModel.RequestSubGlobalState(UpdateGameGlobalState);
            ViewModel.RequestSubGlobalStateTimer(UpdateGameGlobalStateTimer);

            ViewModel.RequestSubPlugImages(UpdatePlugImages);
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnsubHealthUI(UpdateCurrHealthUI);
            ViewModel.RequestUnsubPlayersInfo(UpdatePlayerInfo);
            ViewModel.RequestUnsubDeathUI(UpdateDeathUI);

            ViewModel.UnsubInitGameEventToClient(ReceiveEvents);
            
            ViewModel.RequestUnsubActiveSlot(SetActiveItemSlot);

            ViewModel.RequestUnsubThrowCharge(UpdateThrowChargeText);
            ViewModel.RequestUnsubGlobalState(UpdateGameGlobalState);
            ViewModel.RequestUnsubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            ViewModel.RequestUnsubCameraRotation(UpdateMiniMapRotation);
            ViewModel.RequestUnsubPlayerPosition(UpdateMiniMapPosition);
            ViewModel.RequestUnsubGlobalStateTimer(UpdateGameGlobalStateTimer);
            ViewModel.RequestUnsub();
        }
        
        private void UpdateCurrHealthUI(int newValue, int maxHealth)
        {
            Debug.Log($"[UI] new hp {newValue}");

            _healthImage.style.opacity = (1 - (float)newValue / maxHealth) * 0.4f;
            _health1.value = (float)newValue / maxHealth * 100;
        }

        private void UpdatePlayerInfo(PlayerViewData local, List<PlayerViewData> others)
        {
            _localPlayerAvatar.style.backgroundImage = new StyleBackground(local.avatar);

            for (var i=0; i < _otherPlayerAvatars.Count; i++)
            {
                if (i < others.Count)
                {
                    _otherPlayerAvatars[i].style.backgroundImage = new StyleBackground(others[i].avatar);
                }
                else
                {
                    _otherPlayerAvatars[i].style.backgroundImage = new StyleBackground();
                }
            }


        }
        
        private void UpdateDeathUI(bool isDead)
        {
            Debug.Log($"[UI] death {isDead}");
            if (isDead)
            {
                DOTween.To(
                    () => _deathImage.style.opacity.value,
                    x => _deathImage.style.opacity = x,
                    0.8f, 0.4f
                ).SetEase(Ease.OutQuad);
            }
            else
            {
                DOTween.To(
                    () => _deathImage.style.opacity.value,
                    x => _deathImage.style.opacity = x,
                    0f, 0.4f
                ).SetEase(Ease.OutQuad);
            }
        }

        private void UpdateThrowChargeText(int newValue)
        {
            _throwChargeText.text = newValue == 0 ? "" : $"{newValue.ToString()}%";
        }

        private void UpdateGameGlobalState(GlobalStagesType newValue)
        {
            _gameGlobalStateText.text = newValue switch
            {
                GlobalStagesType.Fight => "Фаза обороны",
                GlobalStagesType.Preparation => "Фаза подготовки",
                _ => "Неизвестная фаза"
            };
        }
        
        private void UpdateGameGlobalStateTimer(float remainingSeconds)
        {
            var minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            var seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            
            _gameGlobalStateTimerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void SetActiveItemSlot(int index)
        {
            if (_activeSlotIndex != -1)
            {
                _itemImages[_activeSlotIndex].style.backgroundColor = noSelectionColor;
                _itemImages[_activeSlotIndex].DOScale(1f, 0.3f);
            }
            
            _activeSlotIndex = index;
            
            if (_activeSlotIndex != -1)
            {
                _itemImages[_activeSlotIndex].style.backgroundColor = selectedColor;
                _itemImages[_activeSlotIndex].DOScale(1.2f, 0.3f);
            }
        }

        private void SetItemImageSprite(int index, Sprite sprite)
        {
            _itemImages[index].style.backgroundImage = new StyleBackground(sprite);
        }

        private void ReceiveEvents(SyncDictionary<int, BaseGameEvent> dict)
        {
            foreach (var i in dict)
            {
                var e = _gameEventsDatabase.GetEvent(i.Value.EventType);
                AddGameEvent(i.Value.EventId, e.EventImage, i.Value.EventId);
            }
        }

        private void SetupEventDatabase(GameEventsDatabase gameEventsDatabase)
        {
            _gameEventsDatabase = gameEventsDatabase;
        }

        private void Clear()
        {
            _gameEvents.Clear();
        }
        
        private void AddGameEvent(int eventId, Sprite icon, int roomNumber)
        {
            if (_gameEvents.ContainsKey(eventId)) return;
            
            var gameEvent = gameEventPrefab.Instantiate();
            
            _gameEventsContainer.Add(gameEvent);
            _gameEvents.Add(eventId, gameEvent);
            
            gameEvent.Q<VisualElement>("EventImg").style.backgroundImage = new StyleBackground(icon);
            gameEvent.Q<Label>("EventLb").text = $"R-{roomNumber}";

            gameEvent.DOScale(1f, 0.2f).From(new Vector2(0f,0f)).SetEase(Ease.InOutBack);
        }
        
        private void UpdateGameEvent(int eventId, Sprite icon, int roomNumber)
        {
            if (_gameEvents.TryGetValue(eventId, out var gameEvent) && gameEvent != null)
            {
                gameEvent.Q<VisualElement>("EventImg").style.backgroundImage = new StyleBackground(icon);
                gameEvent.Q<Label>("EventLb").text = $"R-{roomNumber}";
            }
        }

        private void RemoveGameEvent(int eventId)
        {
            if (_gameEvents.TryGetValue(eventId, out var gameEvent) && gameEvent != null)
            {
                gameEvent.DOScale(0f, 0.2f)
                    .From(new Vector2(1f, 1f)).SetEase(Ease.InOutBack)
                    .OnComplete(() =>
                    {
                        _gameEventsContainer.Remove(gameEvent);
                        _gameEvents.Remove(eventId);
                    });
            }
                
        }

        public void UpdatePlugImages(List<WireType> types)
        {
            switch (types.Count)
            {
                case 0:
                    {
                        _leftPlugSocketImage.DOPunch(new Vector3(0f, 10f, 0f), 0.2f).OnComplete(() =>
                        {
                            _leftPlugImage.visible = false;
                            _rightPlugImage.visible = false;

                            _leftPlugSocketImage.visible = false;
                            _rightPlugSocketImage.visible = false;

                            _wirePlacementContainer.visible = false;
                        });
                        break;
                    }
                case 1:
                    {
                        _wirePlacementContainer.visible = true;
                        _leftPlugSocketImage.visible = true;
                        _leftPlugImage.visible = true;
                        _leftPlugImage.style.backgroundImage = new StyleBackground(
                            ViewModel.BuildingsDatabase.wireTypeInfo[types[0]].wireTypeImage);

                        _leftPlugSocketImage.DOPunch(new Vector3(0f, 10f, 0f), 0.2f);
                        break;
                    }
                case 2:
                    {
                        _rightPlugSocketImage.visible = true;
                        _rightPlugImage.visible = true;
                        _rightPlugImage.style.backgroundImage = new StyleBackground(
                            ViewModel.BuildingsDatabase.wireTypeInfo[types[1]].wireTypeImage);
                        _rightPlugSocketImage.DOPunch(new Vector3(0f, 10f, 0f), 0.2f).OnComplete(() => 
                            {
                                _wirePlacementContainer.visible = false;
                            });
                        break;
                    }
            }
        }

        private void UpdateMiniMapRotation(float currentYAngle)
        {
            if (_miniMap == null) Debug.LogError("[UI] Не назначена миникарта");
            _miniMap.SetRotation(-currentYAngle);
        }
        
        private void SetMinimapSource(LevelGrid levelGrid)
        {
            _levelGrid = levelGrid;

            if (_miniMap == null)
            {
                Debug.LogWarning("[UI] В UXML не найден элемент 'Minimap' типа MinimapView.");
                return;
            }

            _miniMap.SetSource(levelGrid);
        }
        private void UpdateMiniMapPosition(Vector3 playerPosition)
        {
            if (_miniMap == null || _levelGrid == null) return;
            
            var cellPosition = _levelGrid.UnityGrid.WorldToCell(playerPosition);
            
            _miniMap.SetCenter(cellPosition);
        }
    }
}