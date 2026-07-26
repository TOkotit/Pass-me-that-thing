using Enums;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu
{
    [UxmlElement("RebindButton")]
    public partial class RebindButton : Button
    {
        public int inputActionId;
        public int compBindingId;
        public InputMapType inputMapType;

        public event Action<int, int, InputMapType> OnRebindClick;
        public RebindButton() : base()
        {
            RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnClick(ClickEvent e)
        {
            Debug.Log($"RebindButton OnClick {inputActionId} {compBindingId}");
            OnRebindClick?.Invoke(inputActionId, compBindingId, inputMapType);
        }
    }
}