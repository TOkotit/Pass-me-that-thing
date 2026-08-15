using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu
{
    [UxmlElement("CustomDropdown")]
    public partial class CustomDropdown : VisualElement, INotifyValueChanged<string>
    {
        private readonly Button _mainButton;
        private readonly Label _selectedLabel;
        private readonly VisualElement _arrow;
        private readonly VisualElement _popup;
        private readonly ScrollView _scrollView;

        private List<string> _choices = new List<string>() { "1111111111","222222","3","4","555555"};
        private string _value;
        private bool _isOpen;

        public string value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                var previous = _value;
                _value = value;
                _selectedLabel.text = string.IsNullOrEmpty(_value) ? "..." : _value;

                using var evt = ChangeEvent<string>.GetPooled(previous, _value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public CustomDropdown()
        {
            AddToClassList("custom-dropdown");

            _mainButton = new Button() { name = "dropdown-button" };
            _mainButton.RegisterCallback<ClickEvent>(ToggleDropdown);
            

            _mainButton.AddToClassList("custom-dropdown__button");
            Add(_mainButton);

            _selectedLabel = new Label("...") { name = "dropdown-label" };
            _selectedLabel.AddToClassList("custom-dropdown__label");
            _mainButton.Add(_selectedLabel);

            _arrow = new VisualElement { name = "dropdown-arrow" };
            _arrow.AddToClassList("custom-dropdown__arrow");
            _mainButton.Add(_arrow);

            _popup = new VisualElement { name = "dropdown-popup" };
            _popup.AddToClassList("custom-dropdown__popup");
            _popup.style.display = DisplayStyle.None;

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("custom-scroll-view");
            _popup.focusable = false;
            _popup.Add(_scrollView);

            

            _popup.focusable = true;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _popup.RegisterCallback<FocusOutEvent>(_ => CloseDropdown());
        }

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            _popup.style.top = parent.WorldToLocal(_mainButton.worldBound.position).y;
            _popup.style.left = parent.WorldToLocal(_mainButton.worldBound.position).x;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (panel != null && !panel.visualTree.Contains(_popup))
            {
                Debug.Log("OnAttachToPanel");
                parent.Add(_popup);
                
            }
        }

        public void SetChoices(List<string> choices)
        {
            _choices = choices ?? new List<string>();
            RebuildPopup();
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _value = newValue;
            _selectedLabel.text = string.IsNullOrEmpty(_value) ? "..." : _value;
        }

        private void ToggleDropdown(ClickEvent e)
        {
            if (_isOpen) CloseDropdown();
            else OpenDropdown();
        }

        private void OpenDropdown()
        {
            Debug.Log("[ui] OpenDropdown");
            _isOpen = true;
            _popup.style.display = DisplayStyle.Flex;
            _arrow.AddToClassList("custom-dropdown__arrow--open");
            _popup.Focus();
        }

        private void CloseDropdown()
        {
            Debug.Log("[ui] CloseDropdown");
            _isOpen = false;
            _popup.style.display = DisplayStyle.None;
            _arrow.RemoveFromClassList("custom-dropdown__arrow--open");
        }

        private void RebuildPopup()
        {
            _scrollView.Clear();

            foreach (var choice in _choices)
            {
                var itemButton = new Button(() =>
                {
                    value = choice;
                    CloseDropdown();
                })
                {
                    text = choice
                };
                itemButton.AddToClassList("custom-dropdown__item");
                itemButton.focusable = false;

                if (choice == _value)
                {
                    itemButton.AddToClassList("custom-dropdown__item--selected");
                }

                _scrollView.Add(itemButton);
            }
        }
    }
}