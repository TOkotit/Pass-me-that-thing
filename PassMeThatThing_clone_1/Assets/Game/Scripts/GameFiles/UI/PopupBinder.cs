using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// класс который будет переписываться и цепляться к всплывающим окнам(объектам) ui
    /// в качестве T принимает соответствующий view model вспл. окна(попапа)
    /// Дополнен специально для всплывающих окон
    /// </summary>
    public class PopupBinder<T> : WindowBinder<T>
        where T : WindowViewModel
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnCloseAlt;

        protected virtual void Start()
        {
            _btnClose?.onClick.AddListener(OnCloseButtonClick);
            _btnCloseAlt?.onClick.AddListener(OnCloseButtonClick);
        }

        protected virtual void OnDestroy()
        {
            _btnClose?.onClick.RemoveListener(OnCloseButtonClick);
            _btnCloseAlt?.onClick.RemoveListener(OnCloseButtonClick);
        }
        
        protected virtual void OnCloseButtonClick()
        {
            ViewModel.RequestClose();
        }
        
    }
}