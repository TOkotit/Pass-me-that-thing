using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
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

        public event Action OnDestroyBuilding;

        public event Action<string, string> OnSeed;

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

        public void DestroyBuilding()
        {
            OnDestroyBuilding?.Invoke();
        }

        public void SetSeed(string seed, string instId)
        {
            OnSeed?.Invoke(seed, instId);
        }
    }
}