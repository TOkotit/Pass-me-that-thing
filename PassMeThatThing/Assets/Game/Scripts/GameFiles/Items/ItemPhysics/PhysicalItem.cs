
using System.Collections;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity;

using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class PhysicalItem : NetworkBehaviour
    {
        [SerializeField] private float hardness;
        
        
        [SerializeField] private HandleType handleType;
        [SerializeField] private Rigidbody universalPoint;
        [SerializeField] private Rigidbody leftHandPoint;
        [SerializeField] private Rigidbody rightHandPoint;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private bool canBeOwned;
        [SerializeField] private bool doActAndSwing;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private NetworkItem _network;
        [SerializeField] private bool hasToBeAligned;
        
        // [SyncVar]
        private readonly SyncVar<int> durability = new();
        // [SyncVar]
        private readonly SyncVar<bool> _isThrown = new();
        
        private LMBReaction reaction;
        private Outline _outline;
        private CollisionDamageDealer  damageDealer;
        private NetworkTransform _networkTransform;
        
        private ParticlePoolManager _particlePool;
        
        public float Hardness => hardness;
        public int Durability {get => durability.Value; set => durability.Value = value; }
        public HandleType HandleType => handleType;
        public Rigidbody UniversalPoint => universalPoint;
        public Rigidbody LeftHandPoint => leftHandPoint;
        public Rigidbody RightHandPoint => rightHandPoint;
        public Vector3 DefaultPosition => defaultPosition;
        public bool DoActAndSwing => doActAndSwing;
        public bool CanBeOwned => canBeOwned;
        public MainCharacter Owner { get; set; }
        public NetworkConnection ConnectionToClient { get; set; }
        public LMBReaction Reaction => reaction;
        public Rigidbody[] GetHandPoints() => handleType == HandleType.OneHanded 
            ? new[] { universalPoint } 
            : new[] { leftHandPoint, rightHandPoint };
        public Rigidbody Rigidbody => rigidBody;
        public NetworkItem Network => _network;
        public bool HasToBeAligned => hasToBeAligned;
        public NetworkTransform NetworkTransform => _networkTransform;
        public bool IsThrown { get => _isThrown.Value; set => _isThrown.Value = value; }
        
        
        // private Coroutine _actingCoroutine;
        // [SyncVar]
        // private bool _isActing;
        // public bool IsActing
        // {
        //     get => _isActing;
        //     set => _isActing = value;
        // }

        


        [Inject]
        private void Construct(NetworkManager networkManager)
        {
            _particlePool = networkManager.GetComponent<ParticlePoolManager>();
        }

        private void Start()
        {
            _outline = GetComponent<Outline>();
            _networkTransform = GetComponent<NetworkTransform>();
            
            reaction = LMBReactionFactory.CreateReaction(_network.itemId.Value, this);
            
            if (TryGetComponent<CollisionDamageDealer>(out damageDealer))
                damageDealer.OnServerTakeDamage += RpcPlayParticlesOnHit;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (IsServerStarted)
            {
                IsThrown = false;

                reaction?.CollisionEnter(other);
            }
        }
        
        [ObserversRpc]
        private void RpcPlayParticlesOnHit()
        {
            Debug.Log("<color=yellow>PlayParticlesOnHit");
            
            _particlePool.GetAndPlayParticle(Particles.pow, transform.position);
            
        }

        // [ServerRpc(RequireOwnership = false)]
        // public void EnableActingMode(float duration)
        // {
        //     _isActing = true;
        //     if (_actingCoroutine != null)
        //     {
        //         StopCoroutine(_actingCoroutine);
        //     }
        //     _actingCoroutine = StartCoroutine(ActingRoutine(duration));
        // }
        //
        // private IEnumerator ActingRoutine(float duration)
        // {
        //     yield return new WaitForSeconds(duration);
        //     _isActing = false;
        //     _actingCoroutine = null;
        // }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsServerStarted)
            {
                PhysicalItemRegistry.Instance.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (!IsServerStarted)
            {
                PhysicalItemRegistry.Instance.Unregister(this);
            }
        }

        private void OnEnable()
        {
            if (!IsServerStarted && PhysicalItemRegistry.Instance.GetItem(gameObject) == null)
            {
                PhysicalItemRegistry.Instance.Register(this);
            }
        }
    }
}