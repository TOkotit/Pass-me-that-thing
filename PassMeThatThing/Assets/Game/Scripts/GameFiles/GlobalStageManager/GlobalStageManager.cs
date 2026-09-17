using System;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.Utils;
using Mirror;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.GlobalStageManager
{
    public class GlobalStageManager : NetworkBehaviour
    {
        [SyncVar]
        private GlobalStagesType _currentGameStage;
        public GlobalStagesType CurrentGameStage => _currentGameStage;

        public int SyncRemainingTime => Mathf.CeilToInt(_syncRemainingTime);

        [Inject] private GameRandomEventManager _gameRandomEventManager;
        [Inject] private EnemyDatabase _enemyDatabase;
        [Inject] private GlobalStageDatabase _globalStageDatabase;

        [Inject] private EnemySpawner _enemySpawner;
        [Inject] private PlayerReadyManager _playerReadyManager;

        private NetworkTimer _timer;
        private bool _inOvertime;
        private bool _fightEnded;

        [SyncVar(hook = nameof(OnStageChanged))]
        private Stage _stage = new();

        [SyncVar(hook = nameof(OnTimeChanged))]
        private float _syncRemainingTime;

        public event Action<float> OnTimerChangedUI;
        public event Action<Stage> OnStageChangedUI;

        public static GlobalStageManager Instance { get; private set; }
        public Stage Stage => _stage;

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

            var newSt = new Stage(_stage.Type, _stage.Day, _stage.Level);

            newSt.Type = newStage;
            _currentGameStage = newStage;

            if (_currentGameStage == GlobalStagesType.Preparation)
            {
                _playerReadyManager.ResetReady();
                //_gameRandomEventManager.TryTriggerRandomEvents();

                newSt.Level++;
                newSt.Day = _stage.Level % _globalStageDatabase.LevelInDayAmount + 1;
            }
            else if (_currentGameStage == GlobalStagesType.Fight)
            {
                _gameRandomEventManager.TryTriggerRandomEvents();

                _enemySpawner.SpawnWave(GetEnemies());
            }
            else if (_currentGameStage == GlobalStagesType.Rest)
            {
                RunRestLogic();
            }

            _stage = newSt;

            var duration = _globalStageDatabase.GetStageDuration(_stage.Type, _stage.Level);

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
            else if (_currentGameStage == GlobalStagesType.Rest)
            {
                StartStage(GlobalStagesType.Preparation);
            }
        }

        [Server]
        private void EndFight()
        {
            if (_fightEnded) return;
            _fightEnded = true;
            _inOvertime = false;

            if (_stage.Level % _globalStageDatabase.LevelInDayAmount == 0)
            {
                StartStage(GlobalStagesType.Rest);
            }
            else
            {
                StartStage(GlobalStagesType.Preparation);
            }
        }

        [Server]
        private void RunRestLogic()
        {

        }

        [Server]
        private List<EnemyData> GetEnemies()
        {
            var result = new List<EnemyData>();
            var enemyPacks = _globalStageDatabase.GetEnemyPacksByLevel(_stage.Level);

            foreach (var ep in enemyPacks)
            {
                var enemiesDiff = _enemyDatabase.GetEnemiesByDiff(ep.enemyDiff);

                for (var i = 0; i < ep.count; i++)
                {
                    result.Add(enemiesDiff[Random.Range(0, enemiesDiff.Count)]);
                }
            }

            return result;
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

        private void OnStageChanged(Stage oldStage, Stage newStage)
        {
            OnStageChangedUI?.Invoke(newStage);
        }
    }

    [Serializable]
    public struct Stage
    {
        public GlobalStagesType Type;
        public int Day;
        public int Level;

        public Stage(GlobalStagesType newStage, int dayCount, int levelCount)
        {
            Type = newStage;
            Day = dayCount;
            Level = levelCount;
        }
    }
}