using Enums;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Mirror;
using UnityEngine;
using VContainer;

namespace Entity
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ToughnessDamageable : Damageable
    {
        private ToughnessModel _toughnessModel;
        [SerializeField] protected RagdollHandler ragdollHandler;
        public RagdollHandler RagdollHandler => ragdollHandler;

        public ToughnessModel ToughnessModel { get => _toughnessModel; set => _toughnessModel = value; }

        [SyncVar(hook = nameof(OnSyncedToughnessChanged))]
        private int _syncedToughness;
        
        [SyncVar(hook = nameof(OnSyncedMaxToughnessChanged))]
        private int _syncedMaxToughness;

        protected virtual void Awake()
        {
            if (_toughnessModel == null)
                _toughnessModel = new ToughnessModel();
        }

        protected override void Start()
        {
            base.Start();
            if (isServer)
            {
                _toughnessModel.OnToughnessChanged += OnToughnessChanged;
                _toughnessModel.OnToughnessBreak += OnToughnessBreak;
            }
        }
        public virtual void Hit(Vector3 force, Vector3 hitPosition){}
        protected virtual void OnDestroy()
        {
            base.OnDestroy();
            
            if (isServer)
            {
                _toughnessModel.OnToughnessChanged -= OnToughnessChanged;
                _toughnessModel.OnToughnessBreak -= OnToughnessBreak;
            }
        }
        
        
        [Server]
        public void ServerReduceToughness(int amount)
        {
            _toughnessModel.ReduceToughness(amount);
            _syncedToughness = _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerSetToughness(int newToughness)
        {
            _toughnessModel.SetToughness(newToughness);
            _syncedToughness =  _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerFullToughnessRecover()
        {
            _toughnessModel.SetToughness(_toughnessModel.MaxToughness);
            _syncedToughness =  _toughnessModel.CurrentToughness;
        }
        
        [Server]
        public void ServerSetMaxToughness(int newToughness, bool fullToughness)
        {
            _toughnessModel.SetMaxToughness(newToughness, fullToughness);
            _syncedMaxToughness = _toughnessModel.CurrentToughness;
        }


        // Хуки 
        public void OnSyncedToughnessChanged(int oldToughness, int newToughness)
        {
            if (!isServer) 
            {
                _toughnessModel.SetToughness(newToughness);
                
                OnToughnessChanged(_toughnessModel.CurrentToughness, 
                    _toughnessModel.MaxToughness);
                if (newToughness <= 0) OnToughnessBreak();
            }
        }
        
        public void OnSyncedMaxToughnessChanged(int oldMax, int newMax)
        {
            if (!isServer)
            {
                _toughnessModel.SetMaxToughness(newMax, false);
            }
        }
        
        public abstract void OnToughnessBreak();
        public abstract void OnToughnessChanged(int currentToughness, int maxToughness);
    }
}