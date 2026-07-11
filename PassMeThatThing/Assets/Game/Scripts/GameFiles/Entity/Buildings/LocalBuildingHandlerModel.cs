using System;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class LocalBuildingHandlerModel
    {
        
        public event Action<int> OnStartBuildPreview;
        public event Action OnCancelBuildPreview;
        public event Action OnConfirmBuildPreview;

        public void StartBuildPreview(int buildingIndex)
        {
            Debug.Log($"Starting building preview {buildingIndex}");
                OnStartBuildPreview?.Invoke(buildingIndex);
        }

        public void CancelBuildPreview()
        {
            OnCancelBuildPreview?.Invoke();
        }
        
        public void ConfirmBuildPreview()
        {
            OnConfirmBuildPreview?.Invoke();
        }
    }
}