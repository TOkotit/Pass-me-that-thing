using System;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class LocalBuildingHandlerModel
    {
        
        public event Action<int> OnStartBuildPreviewByIndex;
        public event Action<string> OnStartBuildPreviewById;
        public event Action OnCancelBuildPreview;
        public event Action OnConfirmBuildPreview;

        public void StartBuildPreview(int buildingIndex)
        {
            Debug.Log($"Starting building preview {buildingIndex}");
            OnStartBuildPreviewByIndex?.Invoke(buildingIndex);
        }
        
        public void StartBuildPreview(string buildingId)
        {
            Debug.Log($"Starting building preview {buildingId}");
            OnStartBuildPreviewById?.Invoke(buildingId);
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