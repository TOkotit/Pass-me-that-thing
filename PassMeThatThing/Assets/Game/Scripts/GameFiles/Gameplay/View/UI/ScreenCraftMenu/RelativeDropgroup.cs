using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.ScreenCraftMenu
{
    [UxmlElement("RelativeDropgroup")]
    public partial class RelativeDropgroup : VisualElement
    {
        private readonly Button _mainButton;
        private readonly Label _label;
        private readonly VisualElement _arrow;
        private readonly VisualElement _content;

        private bool _isOpen;

        public VisualElement Content => _content;

        public RelativeDropgroup()
        {
            AddToClassList("dropgroup");

            _mainButton = new Button() { name = "dropgroup-button" };
            _mainButton.RegisterCallback<ClickEvent>(ToggleDropdown);

            _mainButton.AddToClassList("dropgroup__button");
            Add(_mainButton);

            _arrow = new VisualElement { name = "dropgroup-arrow" };
            _arrow.AddToClassList("dropgroup__arrow");
            _mainButton.Add(_arrow);

            _label = new Label("...") { name = "dropgroup-label" };
            _label.AddToClassList("dropgroup__label");
            _mainButton.Add(_label);

            _content = new VisualElement { name = "dropgroup-content" };
            _content.AddToClassList("dropgroup__content");
            _content.style.display = DisplayStyle.None;
            Add(_content);

        }

        private void ToggleDropdown(ClickEvent e)
        {
            if (_isOpen) CloseDropdown();
            else OpenDropdown();
        }

        private void OpenDropdown()
        {
            //Debug.Log("[ui] OpenDropdown");
            _isOpen = true;
            _content.style.display = DisplayStyle.Flex;
            _arrow.AddToClassList("dropgroup__arrow--open");
        }

        private void CloseDropdown()
        {
            //Debug.Log("[ui] CloseDropdown");
            _isOpen = false;
            _content.style.display = DisplayStyle.None;
            _arrow.RemoveFromClassList("dropgroup__arrow--open");
        }

        public void SetLabel(string text)
        {
            _label.text = text;
        }
    }
}