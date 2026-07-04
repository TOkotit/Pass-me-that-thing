// Game.Entity.MainCharacter

using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using DI;
using Entity;
using Game.Scripts.GameFiles.Entity.GlobalView;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork.View;
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
        [SerializeField] private Collider characterCollider;
        public MainCharacterModel MainCharacterModel => _model;

        public override DamagableModel DamagableModel => _model;

        private void Initialize()
        {
            view.Initialize(); 
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
            _damagableRegistry.Register(characterCollider.GameObject(), this);
        }
        
        [Server]
        public void Fall(float delay)
        {
            movement.LockUpMovement();
            if (mCamera) mCamera.IsCameraRotating = false;
            RpcFall();
            StartCoroutine(GetUpAfterDelay(delay));
        }
        [Server]
        private IEnumerator GetUpAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if(delay > 0) StandUp();
        }
        
        [ClientRpc]
        private void RpcFall()
        {
            movement.LockUpMovement();
            movement.DisableController();
            if (mCamera) mCamera.IsCameraRotating = false;
            view.DisableAnimator();
            ragdollHandler.EnableRagdoll();
        }
        
        [Server]
        public void StandUp()
        {
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
                ServerSetMaxHealth(100); //SO
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
            
            _localModel.IsDead = true;
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (!isLocalPlayer) return;
            
            Debug.Log($"[MainCharacter] OnHealthChanged {currentHealth}");
            
            _localModel.Health = currentHealth;
            if (DamagableModel != null && DamagableModel?.HealthPool != null)
            {
                _localModel.MaxHealth = DamagableModel.HealthPool.MaxHealth;
            }
        }
    }
}