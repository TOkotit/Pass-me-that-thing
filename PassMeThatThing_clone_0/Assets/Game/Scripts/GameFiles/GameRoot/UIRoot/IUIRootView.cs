using UnityEngine;

namespace UIRoot
{
    /// <summary>
    /// Общая оболчка для всего UI. Чтобы не создавать его каждый раз в сценах и чтобы делать экраны загрузки
    /// Можно скрывать и показывать весь UI
    /// </summary>
    public interface IUIRootView
    {
        void ShowLoadingScreen();
        void HideLoadingScreen();
        void AttachSceneUI(GameObject sceneUI);
    }
}
