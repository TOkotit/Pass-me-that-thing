using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    [Serializable]
    public class WireColorSpritesData
    {
        public Color color;
        public VectorImage start;
        public VectorImage end;
        public VectorImage wire;
    }

    public class ScreenMinigameBinder : WindowBinder<ScreenMinigameViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private SerializedDictionary<GameEventsType, VisualTreeAsset> minigames;

        [SerializeField] private Sprite redIndicator;
        [SerializeField] private Sprite greenIndicator;

        [Header("blackoutCutWires")]
        [SerializeField] private List<WireColorSpritesData> wireColors;

        //ui refs
        private VisualElement _root;
        private Button _closeBtn;
        private VisualElement _successImg;
        private GroupBox _minigameContentContainer;
        private TemplateContainer _currentMinigame;
        
        
        //floodPipeBreak
        private VisualElement _rotationWheel;
        private ProgressBar _wheelProgressBar;
        private bool _isHolding;

        private Vector2 _wheelCenter;
        private float _lastAngle;
        private float _currentAngle;
        private float _progressSpeed = 0.1f;
        
        //blackoutBlowFuse
        private VisualElement _p30Img;
        private VisualElement _p60Img;
        private VisualElement _p100Img;
        private List<CustomToggle> _customToggles;

        private int _turnedToggles;
        
        //blackoutCutWires
        private GroupBox _inputContainer;
        private GroupBox _outputContainer;

        private Dictionary<VisualElement, Color> _elemRegistry = new();
        private List<Color> _matchedColors = new();
        
        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _closeBtn = _root.Q<Button>("CloseBtn");
            _successImg = _root.Q<VisualElement>("SuccessImg");
            _successImg.visible = false;
            _minigameContentContainer = _root.Q<GroupBox>("MinigameContentContainer");
        }
        
        private void Start()
        {
            EnableMinigameByType(ViewModel.Parameters);
            
            _closeBtn.RegisterCallback<ClickEvent>(CloseMinigame);
        }

        private void OnDestroy()
        {
            _closeBtn.UnregisterCallback<ClickEvent>(CloseMinigame);

            DisableMinigames();
        }
        
        public void EnableMinigameByType(MinigameParameters parameters)
        {
            _currentMinigame = minigames[parameters.eventType].Instantiate();
            _minigameContentContainer.Add(_currentMinigame);

            switch (parameters.eventType)
            {
                case GameEventsType.FloodBrokenPump:
                    break;
                case GameEventsType.FloodPipeBreak:
                    EnterFloodPipeBreakMinigame(_currentMinigame);
                    break;
                case GameEventsType.BlackoutBlowFuse:
                    EnterBlackoutBlowFuseMinigame(_currentMinigame);
                    break;
                case GameEventsType.BlackoutCutWires:
                    EnterBlackoutCutWiresMinigame(_currentMinigame);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void DisableMinigames()
        {
            _minigameContentContainer.Clear();

            if (_customToggles != null)
            {
                foreach (var t in _customToggles)
                {
                    t.UnregisterValueChangedCallback(CheckToggles);
                }
            }
        }

        
        
        public void EnterBlackoutBlowFuseMinigame(TemplateContainer templateContainer)
        {
            _p30Img = templateContainer.Q<VisualElement>("P30Img");
            _p60Img = templateContainer.Q<VisualElement>("P60Img");
            _p100Img = templateContainer.Q<VisualElement>("P100Img");

            _customToggles = templateContainer.Q<GroupBox>("TogglesContainer")
                .Children()
                .Select(x => x.Children())
                .SelectMany(x => x)
                .Select(x => (CustomToggle)x).ToList();

            foreach (var t in _customToggles)
            {
                t.RegisterValueChangedCallback(CheckToggles);
            }
        }

        public void CheckToggles(ChangeEvent<bool> e)
        {
            _turnedToggles = 0;
            foreach (var t in _customToggles)
            {
                _turnedToggles += t.value ? 1:0;
            }

            UpdateIndicators();
            
            Debug.Log($"OnToggleClicked {_turnedToggles}");
            if (_turnedToggles >= _customToggles.Count)
            {
                CompleteMinigame();
            }
        }

        public void UpdateIndicators()
        {
            var percent = _turnedToggles / (float)_customToggles.Count;

            _p30Img.style.backgroundImage = new StyleBackground(redIndicator);
            _p60Img.style.backgroundImage = new StyleBackground(redIndicator);
            _p100Img.style.backgroundImage = new StyleBackground(redIndicator);
            
            if (percent >= 0.3f) 
                _p30Img.style.backgroundImage = new StyleBackground(greenIndicator);
            if (percent >= 0.6f)
                _p60Img.style.backgroundImage = new StyleBackground(greenIndicator);
            if (percent >= 1f)
                _p100Img.style.backgroundImage = new StyleBackground(greenIndicator);
        }
        
        //blackoutCutWires
        public void EnterBlackoutCutWiresMinigame(TemplateContainer templateContainer)
        {
            _inputContainer = templateContainer.Q<GroupBox>("InputContainer");
            _outputContainer = templateContainer.Q<GroupBox>("OutputContainer");

            var rand = new Random();
            wireColors = wireColors.OrderBy(_ => rand.Next()).ToList();

            foreach (var t in wireColors)
            {
                var newOutput = new VisualElement();
                _outputContainer.Add(newOutput);
                newOutput.AddToClassList("slot");

                newOutput.style.width = 180;
                newOutput.style.height = 120;
                newOutput.style.marginTop = 30f;

                newOutput.style.backgroundImage = new StyleBackground(t.end);
                newOutput.style.alignSelf = Align.FlexEnd;

                _elemRegistry[newOutput] = t.color;
            }
            
            wireColors = wireColors.OrderBy(_ => rand.Next()).ToList();
            foreach (var t in wireColors)
            {
                var inputBox = new VisualElement();
                _inputContainer.Add(inputBox);

                inputBox.style.width = 120;
                inputBox.style.height = 120;
                inputBox.style.marginTop = 30f;

                inputBox.transform.scale = new Vector2(-1, 1);
                inputBox.style.backgroundImage = new StyleBackground(t.end);


                var newWireLine = new LineElement(
                    new Vector2(0, 0),
                    new Vector2(0, 0),
                    t.wire, 120f);
                
                newWireLine.style.position = Position.Absolute;
                
                templateContainer.Add(newWireLine);
                
                var newInput = new VisualElement();
                
                
                inputBox.Add(newInput);
                
                newInput.AddToClassList("draggable");
                
                newInput.AddManipulator(new WireDragAndDropManipulator(newInput,
                    slotContainerName: "OutputContainer",
                    slotClassName: "slot",
                    onDrop: CheckWires,
                    line: newWireLine,
                    box: inputBox,
                    t: t));

                _elemRegistry[newInput] = t.color;
                newInput.style.width = 110;
                newInput.style.height = 110;
                //newInput.style.backgroundColor = Color.lightPink;
            }
        }

        private void CheckWires(VisualElement a, VisualElement b)
        {
            if (_elemRegistry[a] == _elemRegistry[b])
            {
                if (!_matchedColors.Contains(_elemRegistry[a]))
                    _matchedColors.Add(_elemRegistry[a]);
            }
            else
            {
                _matchedColors.Remove(_elemRegistry[a]);
            }
            Debug.Log($"CheckWires {_matchedColors.Count}");

            if (_matchedColors.Count >= wireColors.Count)
            {
                CompleteMinigame();
            }
        }
        
        //floodPipeBreak
        public void EnterFloodPipeBreakMinigame(TemplateContainer templateContainer)
        {
            _rotationWheel = templateContainer.Q<VisualElement>("RotationWheel");
            _wheelProgressBar = templateContainer.Q<ProgressBar>("WheelProgressBar");
            
            _rotationWheel.RegisterCallback<PointerDownEvent>(RWOnPointerDown);
            _rotationWheel.RegisterCallback<PointerUpEvent>(RWOnPointerUp);
            _rotationWheel.RegisterCallback<PointerCaptureOutEvent>(RWOnPointerCaptureOut);
            _rotationWheel.RegisterCallback<PointerMoveEvent>(RWOnPointerMove);
        }
        
        public void RWOnPointerDown(PointerDownEvent evt)
        {
            _isHolding = true;
            _rotationWheel.CapturePointer(evt.pointerId);
            
            _wheelCenter = _rotationWheel.worldBound.center;
            var mousePosition = evt.position;
            var direction = (Vector2)mousePosition - _wheelCenter;
            
            _lastAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        
        public void RWOnPointerUp(PointerUpEvent evt)
        {
            _isHolding = false;
            if (_rotationWheel.HasPointerCapture(evt.pointerId))
            {
                _rotationWheel.ReleasePointer(evt.pointerId);
            }
        }
        
        private void RWOnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _isHolding = false;
        }
        
        private void RWOnPointerMove(PointerMoveEvent evt)
        {
            if (_isHolding)
            {
                var mousePosition = evt.position;
                var direction = (Vector2)mousePosition - _wheelCenter;

                _currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                var angleDelta = Mathf.DeltaAngle(_lastAngle, _currentAngle);
                _wheelProgressBar.value += angleDelta * _progressSpeed;
                
                if (_wheelProgressBar.value >= _wheelProgressBar.highValue)
                {
                    CompleteMinigame();
                    return;
                }
                
                _lastAngle = _currentAngle;
                _rotationWheel.style.rotate = new StyleRotate(new Rotate(new Angle(_currentAngle, AngleUnit.Degree)));
            }
        }

        //general
        public void CompleteMinigame()
        {
            var anim =  DOTween.Sequence();
            
            _successImg.visible = true;
            
            anim.Append(DOTween.To(
                    () => _successImg.style.opacity.value,
                    x => _successImg.style.opacity = x,
                    1f, 0.3f).From(0f))
                .Join(_successImg.DOScale(1f, 0.3f).From(new Vector2(0f, 0f)).SetEase(Ease.OutBounce))
                .Append(_currentMinigame.DOScale(0f, 0.5f).From(new Vector2(1f, 1f)).SetEase(Ease.OutBounce))
                .OnComplete(() =>
                {
                    ViewModel.RequestCompleteMinigame();
                });
        }

        public void CloseMinigame(ClickEvent e)
        {
            var anim =  DOTween.Sequence();

            anim.Append(_currentMinigame.DOScale(0f, 0.5f).From(new Vector2(1f, 1f)).SetEase(Ease.OutBounce))
                .OnComplete(() =>
                {
                    ViewModel.RequestCloseMinigame();
                });
        }
    }
}