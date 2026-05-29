using System;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.Utils;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Game.Scripts.GameFiles.GlobalStageManager
{
    
    public class GlobalStageManager : NetworkBehaviour
    {
        [SyncVar]
        private GlobalStagesType _currentGameStage;
        public GlobalStagesType CurrentGameStage => _currentGameStage;

        [Inject] private GameRandomEventManager  gameRandomEventManager;
        private NetworkTimer _timer;

        [SerializeField] private float preparationStageDuration = 200f;
        [SerializeField] private float fightStageDuration = 300f;
        
        [SyncVar(hook = nameof(OnTimeChanged))]
        private float _syncRemainingTime;
        
        public event Action<float> OnTimerChangedUI;

        private void Awake()
        {
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

            _currentGameStage = newStage;

            var duration = _currentGameStage switch
            {
                GlobalStagesType.Preparation => preparationStageDuration,
                GlobalStagesType.Fight => fightStageDuration,
                _ => 0f
            };

            if (_currentGameStage == GlobalStagesType.Fight)
            {
                // логика для случайных ивентов
            }
            else if (_currentGameStage == GlobalStagesType.Preparation)
            {
                // логика для случайных ивентов
            }

            StartTimer(duration);
        }
        
        [Server]
        public void TrySkipPreparationStage()
        {
            if (_currentGameStage != GlobalStagesType.Preparation)
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
            _syncRemainingTime = remainingTime;
            Debug.Log($"[Timer] OnTimerTick received: {remainingTime}");
        }

        private void OnTimerFinished()
        {
            var nextStage = _currentGameStage == GlobalStagesType.Preparation 
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
        
        private void OnTimeChanged(float oldTime, float newTime)
        {
            var secondsVisual = Mathf.CeilToInt(newTime);
            
            if (OnTimerChangedUI == null)
            {
                Debug.LogWarning("[GlobalStageManager] OnTimerChangedUI is null");
                return;
            }

            OnTimerChangedUI.Invoke(secondsVisual);
        }
    }
}