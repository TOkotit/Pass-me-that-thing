using System.Collections.Generic;
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
using UnityEngine.InputSystem;
using Stage = Game.Scripts.GameFiles.GlobalStageManager.Stage;
using System.Collections;
using System;


namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayBinder : WindowBinder<ScreenGameplayViewModel>
    {
        private const string CircleCursorClassName = "cursor-circle";
        private const string CrossCursorClassName = "cursor-cross";

        private Color selectedColor = CustomColorUtils.FromHex("1C452B");
        private Color noSelectionColor = new Color(1f, 1f, 1f, 0f);

        private StyleRotate _styleRotate;
        private int _activeSlotIndex = -1;

        private Dictionary<int, TemplateContainer> _gameEvents = new ();
        
        private GameEventsDatabase _gameEventsDatabase;
        
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset gameEventPrefab;
        [SerializeField] private VisualTreeAsset hintPrefab;
        [SerializeField] private VisualTreeAsset addedResPrefab;

        private VisualElement _root;
        private VisualElement _cursor;
        private LevelGrid _levelGrid;
        private ProgressBar _health1;
        private VisualElement _healthImage;
        private VisualElement _deathImage;
        private Label _throwChargeText;
        private GroupBox _inventoryContainer;
        private List<VisualElement> _itemImages;
        private GroupBox _gameEventsContainer;
        private Label _gameGlobalStateText;
        private Label _gameGlobalStateTimerText;
        private MinimapView  _miniMap;
        private VisualElement _miniMapContainer;

        private VisualElement _leftPlugSocketImage;
        private VisualElement _rightPlugSocketImage;
        private VisualElement _leftPlugImage;
        private VisualElement _rightPlugImage;
        private GroupBox _wirePlacementContainer;

        private VisualElement _playerHud;
        private VisualElement _localPlayerAvatar;
        private List<VisualElement> _otherPlayerAvatars = new();

        private Label _localPlayerName;
        private List<Label> _otherPlayerNames = new();

        private GroupBox _hintsContainer;

        private VisualElement _screenMessageContainer;
        private Label _stageChangeMessage;

        private VisualElement _addedResContainer;
        //private int _maxAddedResCount = 4;
        //private int _currentAddedResCount;

        private List<VisualElement> _disabledPhaseBtns = new();

        private Label _electricityWireResText;
        private Label _waterWireResText;
        private Label _fuelWireResText;

        private void Awake()
        {

            _root = uiDocument.rootVisualElement;

            _cursor = _root.Q<VisualElement>("Cursor");
            _health1 = _root.Q<ProgressBar>("Health1");
            _healthImage = _root.Q<VisualElement>("HealthVisualImg");
            _deathImage = _root.Q<VisualElement>("DeathVisualImg");
            _throwChargeText = _root.Q<Label>("ThrowLb");
            _inventoryContainer = _root.Q<GroupBox>("InventoryContainer");
            _itemImages = _root.Q<GroupBox>("InventoryContainer").Children().ToList();
            _gameEventsContainer = _root.Q<GroupBox>("EventsContainer");
            _gameGlobalStateText = _root.Q<Label>("PhaseLb");
            _gameGlobalStateTimerText = _root.Q<Label>("RemainingTimeLb");
            _miniMap = _root.Q<MinimapView>("Minimap");
            _miniMapContainer = _root.Q<VisualElement>("MinimapContainer");

            _leftPlugSocketImage = _root.Q<VisualElement>("LeftPlugSocketImage");
            _rightPlugSocketImage = _root.Q<VisualElement>("RightPlugSocketImage");
            _leftPlugImage = _root.Q<VisualElement>("LeftPlugImage");
            _rightPlugImage = _root.Q<VisualElement>("RightPlugImage");
            _wirePlacementContainer = _root.Q<GroupBox>("WirePlacementContainer");

            _playerHud = _root.Q<VisualElement>("PlayerHud");

            _hintsContainer = _root.Q<GroupBox>("ControlsContainer");

            _screenMessageContainer = _root.Q<VisualElement>("ScreenMessageContainer");
            _stageChangeMessage = _root.Q<Label>("StageChangeMessage");

            _addedResContainer = _root.Q<VisualElement>("AddedResContainer");

            _localPlayerAvatar = _root.Q<VisualElement>("Avatar1");

            _disabledPhaseBtns = _root.Q<VisualElement>("DisabledPhaseButtonsContainer")
                .Children().ToList();

            _electricityWireResText = _root.Q<Label>("electricityResText");
            _waterWireResText = _root.Q<Label>("waterResText");
            _fuelWireResText = _root.Q<Label>("fuelResText");

            for (var i = 2; i <= 4; i++)
            {
                _otherPlayerAvatars.Add(_root.Q<VisualElement>($"Avatar{i}"));
            }

            _localPlayerName = _root.Q<Label>("name1");

            for (var i = 2; i <= 4; i++)
            {
                _otherPlayerNames.Add(_root.Q<Label>($"name{i}"));
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

            ViewModel.RequestInitGlobalState(UpdateGameGlobalState);
            ViewModel.RequestSubGlobalState(UpdateGameGlobalState);
            ViewModel.RequestSubGlobalState(ScreenMessageStage);

            ViewModel.RequestSubGlobalStateTimer(UpdateGameGlobalStateTimer);
            

            ViewModel.RequestSubPlugImages(UpdatePlugImages);

            ViewModel.RequestSubCursorChange(UpdateCursor);
            ViewModel.RequestSubElementsShake(UpdateElementsShake);

            ViewModel.RequestSubHintsChange(UpdateHints);

            ViewModel.RequestSubAddedRes(UpdateAddedRes);

            ViewModel.RequestSubWireRes(UpdateWireRes);
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

            ViewModel.RequestUnSubHintsChange(UpdateHints);
            ViewModel.RequestUnSubAddedRes(UpdateAddedRes);
            ViewModel.RequestUnSubWireRes(UpdateWireRes);
            ViewModel.RequestUnsub();
        }


        private void UpdateWireRes()
        {
            var d = ViewModel.localGeneralResourcesModel;
            _electricityWireResText.text = $"{d.GetVal(WireType.Electricity).requiredQuantity}/{d.GetVal(WireType.Electricity).availableQuantity}";
            _waterWireResText.text = $"{d.GetVal(WireType.Water).requiredQuantity}/{d.GetVal(WireType.Water).availableQuantity}";
            _fuelWireResText.text = $"0/0";
        }

        private void UpdateAddedRes(Resource res, float val)
        {
            var r = addedResPrefab.Instantiate();
            _addedResContainer.Add(r);
            var rData = ViewModel.resourceDatabase.GetResource(res);
            r.Q<Label>("Value").text = val > 0 ? "+" : "-" + val;
            r.Q<VisualElement>("Image").style.backgroundImage
                = new StyleBackground(rData.resourceImage);
            r.Q<Label>("Name").text = rData.resourceName;

            StartCoroutine(DeleteAddedRes(r));
        }

        private IEnumerator DeleteAddedRes(TemplateContainer r)
        {
            yield return new WaitForSeconds(0.5f);
            _addedResContainer.Remove(r);
        }

        private void UpdateHints()
        {
            _hintsContainer.Clear();

            foreach (var e in ViewModel.PlayerInventoryModel.SceneUseHints)
            {
                var h = hintPrefab.Instantiate();
                _hintsContainer.Add(h);

                h.Q<VisualElement>("HintIcon").style.backgroundImage 
                    = new StyleBackground( 
                        ViewModel.ScreenHintsDatabase.GetHintIcon(e.useHintType));
                h.Q<Label>("HintText").text = e.name;
                h.Q<Label>("Bind").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            foreach (var e in ViewModel.PlayerInventoryModel.ItemUseHints)
            {
                var h = hintPrefab.Instantiate();
                _hintsContainer.Add(h);

                h.Q<VisualElement>("HintIcon").style.backgroundImage
                    = new StyleBackground(
                        ViewModel.ScreenHintsDatabase.GetHintIcon(e.useHintType));
                h.Q<Label>("HintText").text = e.name;
                h.Q<Label>("Bind").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            foreach (var e in ViewModel.PlayerInventoryModel.SceneControlHints)
            {
                var h = hintPrefab.Instantiate();
                _hintsContainer.Add(h);

                var bindString = "[" + InputControlPath.ToHumanReadableString(
                        e.bind.action.bindings[0].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice) + "]";

                h.Q<VisualElement>("HintIcon").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                h.Q<Label>("HintText").text = e.name;
                h.Q<Label>("Bind").text = bindString;
            }

            foreach (var e in ViewModel.PlayerInventoryModel.ItemControlHints)
            {
                var h = hintPrefab.Instantiate();
                _hintsContainer.Add(h);

                var bindString = "[" + InputControlPath.ToHumanReadableString(
                        e.bind.action.bindings[0].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice) + "]";

                h.Q<VisualElement>("HintIcon").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                h.Q<Label>("HintText").text = e.name;
                h.Q<Label>("Bind").text = bindString;
            }
        }

        private void UpdateElementsShake(int sign)
        {
            _styleRotate = new StyleRotate(new Rotate(new Angle(2 * sign)));
            _inventoryContainer.style.rotate = _styleRotate;
            _playerHud.style.rotate = _styleRotate;
            _wirePlacementContainer.style.rotate = _styleRotate;
            _miniMapContainer.style.rotate = _styleRotate;
        }

        private void UpdateCursor(CursorViewType cursorViewType)
        {
            switch (cursorViewType)
            {
                case CursorViewType.Default:
                    _cursor.RemoveFromClassList(CircleCursorClassName);
                    _cursor.RemoveFromClassList(CrossCursorClassName);
                    break;
                case CursorViewType.Circle:
                    _cursor.AddToClassList(CircleCursorClassName);
                    _cursor.RemoveFromClassList(CrossCursorClassName);
                    break;
                case CursorViewType.Cross:
                    _cursor.AddToClassList(CrossCursorClassName);
                    _cursor.RemoveFromClassList(CircleCursorClassName);
                    break;
            }
        }

        private void UpdateCurrHealthUI(int newValue, int maxHealth)
        {
            //Debug.Log($"[UI] new hp {newValue}");

            _healthImage.style.opacity = (1 - (float)newValue / maxHealth) * 0.4f;
            _health1.value = (float)newValue / maxHealth * 100;
        }

        private void UpdatePlayerInfo(PlayerViewData local, List<PlayerViewData> others)
        {
            _localPlayerAvatar.style.backgroundImage = new StyleBackground(local.avatar);
            _localPlayerName.text = local.name;

            for (var i=0; i < _otherPlayerAvatars.Count; i++)
            {
                if (i < others.Count)
                {
                    _otherPlayerAvatars[i].style.backgroundImage = new StyleBackground(others[i].avatar);
                    _otherPlayerNames[i].text = others[i].name;
                }
                else
                {
                    _otherPlayerAvatars[i].style.backgroundImage = new StyleBackground();
                    _otherPlayerNames[i].text = "-";
                }
            }
        }
        
        private void UpdateDeathUI(bool isDead)
        {
            //Debug.Log($"[UI] death {isDead}");
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

        private void UpdateGameGlobalState(Stage newValue)
        {
            var text = $"Day {newValue.Day} {newValue.Type}";

            foreach (var disPhaseBtn in _disabledPhaseBtns)
            {
                disPhaseBtn.visible = true;
            }

            if (newValue.Type != GlobalStagesType.Rest)
            {
                var l = (newValue.Level - 1) % 3 ; //0, 1, 2
                if (0 <= l && l < _disabledPhaseBtns.Count)
                    _disabledPhaseBtns[l].visible = false;
            }
            else
            {
                if (3 < _disabledPhaseBtns.Count)
                    _disabledPhaseBtns[3].visible = false;
            }


            _gameGlobalStateText.text = text;
        }

        private void ScreenMessageStage(Stage stage)
        {
            _stageChangeMessage.text = $"{stage.Day}-{stage.Level} {stage.Type}";
            ScreenMessage(_stageChangeMessage);
        }

        private void ScreenMessage(VisualElement messageElement)
        {
            var seq = DOTween.Sequence();

            messageElement.visible = true;
            var opacity = messageElement.style.opacity;
            var rotate = messageElement.style.rotate;

            seq.Append(
                _stageChangeMessage
                .DOScale(new Vector2(1f, 1f), 0.5f).From(new Vector2(0.5f, 0.5f)))
                .SetEase(Ease.OutQuad);  
            seq.Join(
                DOTween.To(
                    () => opacity.value, 
                    x => opacity.value = x, 
                    0f, 1f));

            seq.Append(
                _stageChangeMessage
                .DOScale(new Vector2(0.5f, 0.5f), 0.3f).From(new Vector2(1f, 1f)))
                .SetEase(Ease.OutQuad);
            seq.Join(
                DOTween.To(
                    () => opacity.value,
                    x => opacity.value = x,
                    1f, 0f));

            seq.Play().OnComplete(
                () => {
                    messageElement.visible = false;
                });
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
            if (_miniMap == null || !_levelGrid) return;
            
            var localPos = _levelGrid.UnityGrid.transform.InverseTransformPoint(playerPosition);
            var cellSize = _levelGrid.UnityGrid.cellSize;
            var exactCellPos = new Vector2(localPos.x / cellSize.x, localPos.z / cellSize.z);
            
            _miniMap.SetCenter(exactCellPos);
        }
    }
}