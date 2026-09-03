using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.Utils;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.GlobalStageManager
{
    public class GlobalStageManager : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnStageChanged))]
        private GlobalStagesType _currentGameStage;
        public GlobalStagesType CurrentGameStage => _currentGameStage;

        public int SyncRemainingTime => Mathf.CeilToInt(_syncRemainingTime);

        [Inject] private GameRandomEventManager _gameRandomEventManager;
        [Inject] private EnemyDatabase _enemyDatabase;
        [Inject] private EnemySpawner _enemySpawner;
        [Inject] private PlayerReadyManager _playerReadyManager;  // <-- теперь через DI

        [Header("Timers")]
        [SerializeField] private float preparationStageDuration = 200f;
        [SerializeField] private float fightStageDuration = 300f;

        private NetworkTimer _timer;
        private bool _inOvertime;
        private bool _fightEnded;

        [SyncVar(hook = nameof(OnTimeChanged))]
        private float _syncRemainingTime;

        public event Action<float> OnTimerChangedUI;
        public event Action<GlobalStagesType> OnStageChangedUI;

        public static GlobalStageManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _timer = new NetworkTimer(this, OnTimerTick);
            _timer.TimeIsOver += OnTimerFinished;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _playerReadyManager.OnAllPlayersReady += () => TrySkipPreparationStage();
            StartStage(GlobalStagesType.Preparation);
        }

        private void Update()
        {
            if (!isServer) return;

            if (_currentGameStage == GlobalStagesType.Fight && !_inOvertime && !_fightEnded)
            {
                if (_enemySpawner.EnemyCount == 0)
                    EndFight();
            }

            if (_inOvertime && !_fightEnded)
            {
                if (_enemySpawner.EnemyCount == 0)
                    EndFight();
            }
        }

        [Server]
        private void StartStage(GlobalStagesType newStage)
        {
            _timer.Stop();
            _inOvertime = false;
            _fightEnded = false;
            _currentGameStage = newStage;

            float duration = _currentGameStage switch
            {
                GlobalStagesType.Preparation => preparationStageDuration,
                GlobalStagesType.Fight => fightStageDuration,
                _ => 0f
            };

            if (_currentGameStage == GlobalStagesType.Preparation)
            {
                _playerReadyManager.ResetReady();
                //_gameRandomEventManager.TryTriggerRandomEvents();
            }
            else if (_currentGameStage == GlobalStagesType.Fight)
            {
                _gameRandomEventManager.TryTriggerRandomEvents();
                var spiderData = _enemyDatabase.GetEnemy("spider");
                var zombieData = _enemyDatabase.GetEnemy("zombie");
                _enemySpawner.SpawnWave(new List<EnemyData>()
                {
                    //spiderData,
                    zombieData,
                    //zombieData,
                    //zombieData,
                });
            }

            if (duration > 0)
                StartTimer(duration);
            else
                _syncRemainingTime = 0f;
        }

        [Server]
        public void TrySkipPreparationStage()
        {
            if (_currentGameStage != GlobalStagesType.Preparation) return;
            StartStage(GlobalStagesType.Fight);
        }

        [Command(requiresAuthority = false)]
        public void CmdSkipPreparation(NetworkIdentity playerIdentity)
        {
            _playerReadyManager.SetReady(playerIdentity);
        }

        [Server]
        public void RegisterPlayer(NetworkIdentity player)
        {
            _playerReadyManager.Register(player);
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
        }

        private void OnTimerFinished()
        {
            if (_currentGameStage == GlobalStagesType.Preparation)
            {
                StartStage(GlobalStagesType.Fight);
            }
            else if (_currentGameStage == GlobalStagesType.Fight)
            {
                if (_enemySpawner.EnemyCount > 0)
                {
                    _inOvertime = true;
                    _syncRemainingTime = 0f;
                    RpcStartOvertime();
                }
                else
                {
                    EndFight();
                }
            }
        }

        [Server]
        private void EndFight()
        {
            if (_fightEnded) return;
            _fightEnded = true;
            _inOvertime = false;
            StartStage(GlobalStagesType.Preparation);
        }

        [ClientRpc]
        private void RpcStartOvertime()
        {
            Debug.Log("Овертайм! Убейте оставшихся врагов.");
        }

        private void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.TimeIsOver -= OnTimerFinished;
                _timer.Stop();
            }
            if (Instance == this)
                Instance = null;
        }

        private void OnTimeChanged(float oldTime, float newTime)
        {
            OnTimerChangedUI?.Invoke(Mathf.CeilToInt(newTime));
        }

        private void OnStageChanged(GlobalStagesType oldStage, GlobalStagesType newStage)
        {
            OnStageChangedUI?.Invoke(_currentGameStage);
        }
    }
}