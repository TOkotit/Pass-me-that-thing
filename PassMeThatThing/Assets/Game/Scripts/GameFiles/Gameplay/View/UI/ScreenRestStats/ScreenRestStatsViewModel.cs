using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.UI;
using System;
using System.Collections.Generic;
using Systems;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Gameplay.View.UI
{
    public class ScreenRestStatsViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly GameInputManager _gameInputManager;

        private readonly EnemySpawner _enemySpawner;
        private readonly ResourceStorage _mainResourceStorage;
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GlobalStageManager _globalStageManager;

        public readonly EnemyDatabase enemyDatabase;
        public readonly ResourceDatabase resourceDatabase;
        public readonly GameEventsDatabase gameEventsDatabase;

        public override string Id => "ScreenRestStats";

        public ScreenRestStatsViewModel(GameplayUIManager uiManager,
            IObjectResolver container)
        {
            _uiManager = uiManager;

            _gameInputManager = container.Resolve<GameInputManager>();

            _enemySpawner = container.Resolve<EnemySpawner>();
            _gameRandomEventManager = container.Resolve<GameRandomEventManager>();

            enemyDatabase = container.Resolve<EnemyDatabase>();
            resourceDatabase = container.Resolve<ResourceDatabase>();
            gameEventsDatabase = container.Resolve<GameEventsDatabase>();
            _globalStageManager = container.Resolve<GlobalStageManager>();


            _gameInputManager.GameInput.Gameplay.PauseMenu.performed += PauseMenuPerformed;
        }

        public override void Dispose()
        {
            _gameInputManager.GameInput.Gameplay.PauseMenu.performed -= PauseMenuPerformed;
        }

        public void PauseMenuPerformed(InputAction.CallbackContext c)
        {
            RequestGoToScreenGameplay();
        }

        public void RequestGoToScreenGameplay()
        {
            _uiManager.OpenScreenGameplay();
        }

        public void RequestInitKilledEnemies(Action<IReadOnlyDictionary<string, int>> f)
        {
            f(_enemySpawner.EnemyKilled);
        }

        public void RequestInitRecievedRes(Action<IReadOnlyDictionary<Resource, float>> f)
        {
            f(MainResourceStorage.Instance.DiffReceivedResOnPhase);
        }

        public void RequestInitFixedEvents(Action<IReadOnlyDictionary<GameEventsType, int>> f)
        {
            f(_gameRandomEventManager.FixedEvents);
        }


        public void RequestInitGlobalState(Action<Stage> f)
        {
            f(_globalStageManager.Stage);
        }

        public void RequestSubGlobalState(Action<Stage> f)
        {
            _globalStageManager.OnStageChangedUI += f;
        }

        public void RequestUnsubGlobalState(Action<Stage> f)
        {
            _globalStageManager.OnStageChangedUI -= f;
        }

        public void RequestSubGlobalStateTimer(Action<float> f)
        {
            f(_globalStageManager.SyncRemainingTime);

            _globalStageManager.OnTimerChangedUI += f;
        }

        public void RequestUnsubGlobalStateTimer(Action<float> f)
        {
            _globalStageManager.OnTimerChangedUI -= f;
        }
    }
}
