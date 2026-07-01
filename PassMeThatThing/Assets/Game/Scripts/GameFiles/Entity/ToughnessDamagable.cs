using Enums;
using FishNet.Object;
using FishNet.Object.Synchronizing;

using UnityEngine;
using VContainer;

namespace Entity
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ToughnessDamagable : Damagable
    {
        protected ToughnessModel _toughnessModel;
        
        // [SyncVar(OnChange = nameof(OnSyncedToughnessChanged))]
        private readonly SyncVar<int> _syncedToughness = new();
        
        //[SyncVar(OnChange = nameof(OnSyncedMaxToughnessChanged))]
        private readonly SyncVar<int> _syncedMaxToughness = new();
        
        protected virtual void Start()
        {
            base.Start();

            _syncedToughness.OnChange += OnSyncedToughnessChanged;
            _syncedMaxToughness.OnChange += OnSyncedMaxToughnessChanged;
            
            if (IsServerStarted)
            {
                _toughnessModel.OnToughnessChanged += OnToughnessChanged;
                _toughnessModel.OnToughnessBreak += OnToughnessBreak;
            }
        }
        
        protected virtual void OnDestroy()
        {
            base.OnDestroy();
            
            if (IsServerStarted)
            {
                _toughnessModel.OnToughnessChanged -= OnToughnessChanged;
                _toughnessModel.OnToughnessBreak -= OnToughnessBreak;
            }
        }
        
        
        [Server]
        public void ServerReduceToughness(int amount)
        {
            _toughnessModel.ReduceToughness(amount);
            _syncedToughness.Value = _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerSetToughness(int newToughness)
        {
            _toughnessModel.SetToughness(newToughness);
            _syncedToughness.Value =  _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerFullToughnessRecover()
        {
            _toughnessModel.SetToughness(_toughnessModel.MaxToughness);
            _syncedToughness.Value =  _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerSetMaxToughness(int newToughness, bool fullToughness)
        {
            _toughnessModel.SetMaxToughness(newToughness, fullToughness);
            _syncedMaxToughness.Value = _toughnessModel.CurrentToughness;
        }


        // Хуки 
        public void OnSyncedToughnessChanged(int oldToughness, int newToughness, bool asServer)
        {
            if (!IsServerStarted) 
            {
                _toughnessModel.SetToughness(newToughness);
                
                OnToughnessChanged(_toughnessModel.CurrentToughness, 
                    _toughnessModel.MaxToughness);
                if (newToughness <= 0) OnToughnessBreak();
            }
        }
        
        public void OnSyncedMaxToughnessChanged(int oldMax, int newMax, bool asServer)
        {
            if (!IsServerStarted)
            {
                _toughnessModel.SetMaxToughness(newMax, false);
            }
        }
        
        public abstract void OnToughnessBreak();
        public abstract void OnToughnessChanged(int currentToughness, int maxToughness);
    }
}