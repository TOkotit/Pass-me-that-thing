using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.Entity;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.UI;
using R3;
using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription
{
    public class WindowEnemyViewViewModel : WorldWindowViewModel
    {
        
        private GameplayUIManager _gameplayUIManager;
        private readonly MCLocalModel _mcLocalModel;
        private readonly EnemyZombie _enemy;

        public override string Id => "WindowEnemyView";

        public WindowEnemyViewViewModel(GameplayUIManager gameplayUIManager, 
            IObjectResolver container,
            EnemyZombie enemy)
        {
            _gameplayUIManager = gameplayUIManager;
            _mcLocalModel = container.Resolve<MCLocalModel>();
            _enemy = enemy;
            parent = enemy.transform;
        }

        public void RequestSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged += f;
        }
        public void RequestUnSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged -= f;
        }

        public void RequestSubEnemyHealth(Action<int, int> f)
        {
            f(_enemy.DamagableModel.HealthPool.CurrentHealth, _enemy.DamagableModel.HealthPool.MaxHealth);

            _enemy.OnEnemyHealthChanged += f;
        }

        public void RequestUnsubEnemyHealth(Action<int, int> f)
        {
            _enemy.OnEnemyHealthChanged -= f;
        }

        public void RequestSubEnemyToughness(Action<int, int> f)
        {
            f(_enemy.ToughnessModel.CurrentToughness, _enemy.ToughnessModel.MaxToughness);

            _enemy.OnEnemyToughnessChanged += f;
        }

        public void RequestUnsubEnemyToughness(Action<int, int> f)
        {
            _enemy.OnEnemyToughnessChanged -= f;
        }

        public void RequestSubEnemyAttack(Action<float, float> f)
        {
            f(_enemy.ElapsedAttack, _enemy.AttackCooldown);

            _enemy.OnEnemyElapsedAttackChanged += f;
        }

        public void RequestUnsubEnemyAttack(Action<float, float> f)
        {
            _enemy.OnEnemyElapsedAttackChanged -= f;
        }

    }
}