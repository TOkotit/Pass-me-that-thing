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

    }
}