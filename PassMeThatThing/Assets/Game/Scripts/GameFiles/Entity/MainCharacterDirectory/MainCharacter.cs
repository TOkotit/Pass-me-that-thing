// Game.Entity.MainCharacter

using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using DI;
using Entity;
using Game.Entity.Stats;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View;
using Game.Scripts.GameFiles.Entity.MainCharacterPhysics;
using Game.Scripts.GameFiles.Items;
using MainCharacter_old;
using MainCharacterNetwork;
using Mirror;
using Unity.VisualScripting;
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
        [SerializeField] private MainCharacterMovement movement;
        [SerializeField] private MainCharacterCamera mCamera;
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private Animator animator;
        [SerializeField] private MainCharacterView view;
        [SerializeField] private RagdollHandler ragdollHandler;
        [SerializeField] private float fallDelay = 5;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private HandsMaskLayerController  maskLayerController;
        public MainCharacterModel MainCharacterModel => _model;
        public override DamagableModel DamagableModel => _model;
        public MainCharacterMovement Movement => movement;
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
        public void Fall(float delay, Vector3 impulce = new Vector3())
        {
            movement.LockUpMovement();
            if (mCamera) mCamera.IsCameraRotating = false;
            RpcFall(impulce);
            StartCoroutine(GetUpAfterDelay(delay));
        }
        [Server]
        private IEnumerator GetUpAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if(delay > 0) StandUp();
        }
        [Command]
        public void CmdFall(float delay, Vector3 impulse)
        {
            Fall(delay, impulse);
        }

        [ClientRpc]
        private void RpcFall(Vector3 additionalImpulse)
        {
            maskLayerController.EnableFullBodyAnimation();
            playerInteraction.Drop();
            movement.LockUpMovement();
            movement.DisableController();
            if (mCamera) mCamera.IsCameraRotating = false;
            view.DisableAnimator();
            ragdollHandler.EnableRagdoll();
            ragdollHandler.Hit(movement.LastVelocity * 2 + additionalImpulse, transform.position);
        }
        
        [Server]
        public void StandUp()
        {
            if (!_isAlive) return;
            movement.UnlockMovement();               
            if (mCamera) mCamera.IsCameraRotating = true; 
            RpcStandUp();
        }
        [ClientRpc]
        private void RpcStandUp()
        {
            ragdollHandler.DisableRagdoll();
            view.PlayStandingUp(() => 
            {
                view.EnableAnimator();
                maskLayerController.EnableBodyOnlyAnimation(); 
                movement.UnlockMovement();
                movement.EnableController();
                if (mCamera) mCamera.IsCameraRotating = true;
            });
        }

        
        protected void Awake()
        {
            _toughnessModel = new ToughnessModel();
        }

        public new void Start()
        {
            base.Start();
            Initialize();

            if (isServer)
            {
                ServerSetMaxHealth(100, true); //SO
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
            throw new System.NotImplementedException();
        }

        // public override void OnStartClient()
        // {
        //     Initialize();
        // }
        // public override void OnStartServer()
        // {
        //     Initialize();
        //     _damagableRegistry.Register(this);
        // }

        public override void OnDeath()
        {
            /*
            verticalAlign.CmdSetConsciousness(0);
            verticalAlign.Consciousness = 0f;
            verticalAlign.LockConsciousness = true;
            */
            Fall(0);
            movement.LockUpMovement();
            mCamera.IsCameraRotating = false;
            
            
            if (!isLocalPlayer) return;
            
            Debug.Log("[MainCharacter] OnDeath");
            _isAlive = false;
            _localModel.IsDead = true;
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (!isLocalPlayer) return;
            
            Debug.Log($"[MainCharacter] OnHealthChanged {currentHealth}");
            
            _localModel.Health = currentHealth;
            _localModel.MaxHealth = maxHealth;
        }
    }
}