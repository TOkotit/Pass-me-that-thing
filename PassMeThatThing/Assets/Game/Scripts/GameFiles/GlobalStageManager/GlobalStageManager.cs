using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.Utils;

using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Game.Scripts.GameFiles.GlobalStageManager
{
    
    public class GlobalStageManager : NetworkBehaviour
    {
        // [SyncVar(OnChange = nameof(OnStageChanged))]
        private readonly SyncVar<GlobalStagesType> _currentGameStage = new();
        public GlobalStagesType CurrentGameStage => _currentGameStage.Value;

        [Inject] private GameRandomEventManager  _gameRandomEventManager;
        [Inject] private EnemyDatabase _enemyDatabase;
        [Inject] private EnemySpawner  _enemySpawner;
        private NetworkTimer _timer;

        [SerializeField] private float preparationStageDuration = 200f;
        [SerializeField] private float fightStageDuration = 300f;
        
        // [SyncVar(OnChange = nameof(OnTimeChanged))]
        private readonly SyncVar<float> _syncRemainingTime = new();
        
        public event Action<float> OnTimerChangedUI;
        public event Action<GlobalStagesType> OnStageChangedUI;

        private void Awake()
        {
            _syncRemainingTime.OnChange += OnTimeChanged;
            _currentGameStage.OnChange += OnStageChanged;
            
            _timer = new NetworkTimer(this, OnTimerTick);
            _timer.TimeIsOver += OnTimerFinished;
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            StartStage(GlobalStagesType.Preparation);
        }
        
        [Server]
        private void StartStage(GlobalStagesType newStage)
        {
            _timer.Stop();

            _currentGameStage.Value = newStage;

            var duration = _currentGameStage.Value switch
            {
                GlobalStagesType.Preparation => preparationStageDuration,
                GlobalStagesType.Fight => fightStageDuration,
                _ => 0f
            };

            if (_currentGameStage.Value == GlobalStagesType.Fight)
            {
                _gameRandomEventManager.TryTriggerRandomEvents();
                _enemySpawner.SpawnWave(3, _enemyDatabase.GetEnemy("zombie"));
            }
            else if (_currentGameStage.Value == GlobalStagesType.Preparation)
            {
                _gameRandomEventManager.TryTriggerRandomEvents();
            }
            StartTimer(duration);
        }
        
        [Server]
        public void TrySkipPreparationStage()
        {
            if (_currentGameStage.Value != GlobalStagesType.Preparation)
            {
                Debug.LogWarning("[Server] Попытка пропустить стадию, но сейчас идет не подготовка!");
                return;
            }
            
            StartStage(GlobalStagesType.Fight);
        }
        
        [Server]
        public void StartTimer(float duration)
        {
            _timer.Set(duration);
            _timer.Start();
        }

        private void OnTimerTick(float remainingTime)
        {
            _syncRemainingTime.Value = remainingTime;
        }

        private void OnTimerFinished()
        {
            var nextStage = _currentGameStage.Value == GlobalStagesType.Preparation 
                ? GlobalStagesType.Fight 
                : GlobalStagesType.Preparation;

            StartStage(nextStage);
        }
        
        private void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.TimeIsOver -= OnTimerFinished;
                _timer.Stop();
            }
        }
        
        private void OnTimeChanged(float oldTime, float newTime, bool asServer)
        {
            var secondsVisual = Mathf.CeilToInt(newTime);
            
            // if (OnTimerChangedUI == null)
            // {
            //     Debug.LogWarning("[GlobalStageManager] OnTimerChangedUI is null");
            //     return;
            // }

            OnTimerChangedUI?.Invoke(secondsVisual);
        }

        private void OnStageChanged(GlobalStagesType oldStage, GlobalStagesType newStage, bool asServer)
        {
            OnStageChangedUI?.Invoke(_currentGameStage.Value);
        }
    }
}