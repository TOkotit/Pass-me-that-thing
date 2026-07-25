using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    [UxmlElement("CustomToggle")]
    public partial class CustomToggle : VisualElement, INotifyValueChanged<bool>
    {
        private readonly VisualElement _track;
        private readonly VisualElement _knob;
        private readonly Label _label;

        private bool _value;

        public bool value
        {
            get => _value;
            set
            {
                if (_value == value) return;

                using (var evt = ChangeEvent<bool>.GetPooled(_value, value))
                {
                    evt.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(evt);
                }
            }
        }
        
        [UxmlAttribute]
        public Sprite BackgroundSprite;
        
        [UxmlAttribute]
        public Sprite KnobSprite;
        
        public CustomToggle()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.cursor = new StyleCursor(StyleKeyword.Null);
            
            _track = new VisualElement();
            _track.style.width = 150;
            _track.style.height = 150;

            _track.style.justifyContent = Justify.Center;
            _track.style.paddingLeft = 2;
            _track.style.paddingRight = 2;
            
            _knob = new VisualElement();
            _knob.style.width = 150;
            _knob.style.height = 150;
            
            _knob.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
            _knob.style.transitionDuration = new List<TimeValue> { new TimeValue(0.2f, TimeUnit.Second) };

            _track.Add(_knob);
            Add(_track);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<GeometryChangedEvent>(InitSprite);
            
            UpdateVisualState(false);
        }

        public void InitSprite(GeometryChangedEvent e)
        {
            _track.style.backgroundImage = new StyleBackground(BackgroundSprite);
            _knob.style.backgroundImage = new StyleBackground(KnobSprite);
            _knob.style.translate = new Translate(0, new Length(50f, LengthUnit.Percent));
        }

        private void OnClick(ClickEvent evt)
        {
            value = !value;
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            UpdateVisualState(true);
        }

        private void UpdateVisualState(bool animate)
        {
            if (_value)
            {
                _knob.style.translate = new Translate(0, 0);
            }
            else
            {
                _knob.style.translate = new Translate(0, new Length(50f, LengthUnit.Percent));
            }
        }
    }
}