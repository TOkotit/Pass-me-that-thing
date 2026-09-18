using System.Collections;
using Assets.Game.Scripts.GameFiles.GameRoot;
using DI;
using Game.Scripts.Systems;
using Mirror;
using Systems;
using UIRoot;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using VContainer;
using VContainer.Unity;

namespace Root
{
    public class EntryPoint : IStartable
    {
        private readonly ICoroutineRunner _coroutines;
        readonly GameManager _gameManager;
        private readonly OptionsManager _optionsManager;
        
        private EntryPoint(
            ICoroutineRunner coroutines,
            GameManager gameManager,
            OptionsManager optionsManager)
        {
            _coroutines = coroutines;
            _gameManager = gameManager;
            _optionsManager = optionsManager;
        }
        
        public void Start()
        {
            _optionsManager.SetInitialSettings();
        }
        

    }
}