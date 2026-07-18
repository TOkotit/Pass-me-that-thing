using System;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class LocalBuildingHandlerModel
    {
        public string InstanceId { get; private set; }
        
        public event Action<string> OnStartBuildPreviewById;
        public event Action OnCancelBuildPreview;
        public event Action OnConfirmBuildPreview;
        
        public void StartBuildPreview(string buildingId, string instanceId=null)
        {
            Debug.Log($"Starting building preview {buildingId}");
            
            InstanceId = instanceId;
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