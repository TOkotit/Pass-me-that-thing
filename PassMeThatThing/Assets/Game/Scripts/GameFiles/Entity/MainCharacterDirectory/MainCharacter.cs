using System.Collections;
using DI;
using Entity;
using Game.Entity.Stats;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View;
using Game.Scripts.GameFiles.Entity.MainCharacterPhysics;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.Systems;
using MainCharacterNetwork;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Entity
{
    public class MainCharacter : ToughnessDamagable
    {
        [Inject] private DamagableRegistry _damagableRegistry;
        [Inject] private MainCharacterModel _model;
        [Inject] private MCLocalModel _localModel;
        [Inject] private GameoverHandler _gameoverHandler;

        [SerializeField] private MainCharacterMovement movement;
        [SerializeField] private MainCharacterCamera mCamera;
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private Animator animator;
        [SerializeField] private MainCharacterView view;
        [SerializeField] private float fallDelay = 5;
        [SerializeField] private PlayerStats stats;
        
        //[SerializeField] private PlayerAnimationStateController maskLayerStateController;

        public MainCharacterModel MainCharacterModel => _model;
        public override DamagableModel DamagableModel => _model;
        public MainCharacterMovement Movement => movement;

        [SyncVar(hook = nameof(OnIsAliveChanged))]
        private bool _isAlive = true;

        public bool IsAlive
        {
            get => _isAlive;
            set => _isAlive = value;
        }

        private void Initialize()
        {
            view.Initialize();
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
            _model.SetStats(stats);
        }

        [Server]
        private IEnumerator GetUpAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (delay > 0) StandUp();
        }

        [Command]
        public void CmdFall(float delay, Vector3 impulse)
        {
            Fall(delay, impulse);
        }

        [Server]
        public void Fall(float delay, Vector3 impulse = new Vector3())
        {
            movement.LockUpMovement();
            if (mCamera) mCamera.IsCameraRotating = false;
            //maskLayerStateController.ApplyFullBody();
            //maskLayerStateController.RpcSetFullBody();
            ragdollHandler.EnableRagdoll();
            RpcFall(impulse);
            StartCoroutine(GetUpAfterDelay(delay));
            Debug.LogWarning(10f * Vector3.forward);
        }

        [ClientRpc]
        private void RpcFall(Vector3 additionalImpulse)
        {
            playerInteraction.Drop();
            movement.LockUpMovement();
            movement.DisableController();
            if (mCamera) mCamera.IsCameraRotating = false;
            view.DisableAnimator();
            ragdollHandler.EnableRagdoll();
        }

        [Server]
        public void StandUp()
        {
            if (!_isAlive) return;
            movement.UnlockMovement();
            ragdollHandler.DisableRagdoll();
            //maskLayerStateController.RpcSetBodyOnly();
            if (mCamera) mCamera.IsCameraRotating = true;
            RpcStandUp();
        }

        [ClientRpc]
        private void RpcStandUp()
        {
            view.PlayStandingUp(() =>
            {
                ragdollHandler.DisableRagdoll();
                //maskLayerStateController.ApplyBodyOnly();
                view.EnableAnimator();
                movement.UnlockMovement();
                movement.EnableController();
                //view.DisableAnimator(); 
                if (mCamera) mCamera.IsCameraRotating = true;
            });
        }
        
        protected void Awake()
        {
            _toughnessModel = new ToughnessModel();
        }

        public new void Start()
        {
            _gameoverHandler.RegisterPlayer(this);
            base.Start();
            Initialize();
            if (isServer)
            {
                ServerSetMaxHealth(100, true);
            }
            else if (isClient)
            {
                OnHealthChanged(DamagableModel.HealthPool.CurrentHealth, DamagableModel.HealthPool.MaxHealth);
            }
        }

        public override void OnToughnessBreak()
        {
            Fall(fallDelay);
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            //throw new System.NotImplementedException();
        }

        public override void OnDeath()
        {
            if (!isServer) return;

            _isAlive = false;

            _gameoverHandler.CheckForGameOver();

            Fall(0);
            movement.LockUpMovement();
            if (mCamera) mCamera.IsCameraRotating = false;
        }

        private void OnIsAliveChanged(bool oldValue, bool newValue)
        {
            if (!newValue && isLocalPlayer) {
                _localModel.IsDead = true;
                Debug.Log("[MainCharacter] OnDeath (local)");
            }
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (!isLocalPlayer) return;
            Debug.Log($"[MainCharacter] OnHealthChanged {currentHealth}");
            _localModel.Health = currentHealth;
            _localModel.MaxHealth = maxHealth;
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            GlobalStageManager.Instance?.RegisterPlayer(netIdentity);
        }

        public override void Hit(Vector3 force, Vector3 hitPosition)
        {
            ragdollHandler.Hit(force, hitPosition);
        }
    }
}