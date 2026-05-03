using UnityEngine;

namespace UIRoot
{
    public class UIRootView :  MonoBehaviour, IUIRootView
    {

        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private Transform _uiSceneContainer;

        private void Awake()
        {
            HideLoadingScreen();
        }

        public void ShowLoadingScreen()
        {
            _loadingScreen.SetActive(true);
        }

        public void HideLoadingScreen()
        {
            _loadingScreen.SetActive(false);
        }
        
        public void AttachSceneUI(GameObject sceneUI)
        {
            ClearSceneUI();

            sceneUI.transform.SetParent(_uiSceneContainer, false);
        }

        private void ClearSceneUI()
        {
            for (var i = _uiSceneContainer.childCount - 1; i >= 0; i--)
                Destroy(_uiSceneContainer.GetChild(i).gameObject);
        }
    }
}