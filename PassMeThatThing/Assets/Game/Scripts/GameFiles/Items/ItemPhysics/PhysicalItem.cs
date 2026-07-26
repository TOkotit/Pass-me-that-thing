
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity;
using Game.Scripts.GameFiles.Entity.Buildings;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class PhysicalItem : NetworkBehaviour
    {
        [SerializeField] private float hardness;
        [SyncVar]
        [SerializeField] private int durability;
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
        [SerializeField] private Collider collider;
        [SyncVar]
        [SerializeField] private bool _isThrown;
        [SerializedDictionary] public SerializedDictionary<Resource, float> Resources; 
        private LMBReaction reaction;
        private Outline _outline;
        private CollisionDamageDealer  damageDealer;
        private NetworkTransformReliable _networkTransform;
        
        [Inject] private ParticlePoolManager _particlePool;

        
        public float Hardness => hardness;
        public int Durability {get => durability; set => durability = value; }
        public HandleType HandleType => handleType;
        public Rigidbody UniversalPoint => universalPoint;
        public Rigidbody LeftHandPoint => leftHandPoint;
        public Rigidbody RightHandPoint => rightHandPoint;
        public Vector3 DefaultPosition => defaultPosition;
        public bool DoActAndSwing => doActAndSwing;
        public bool CanBeOwned => canBeOwned;
        public MainCharacter Owner { get; set; }
        public readonly SyncList<NetworkIdentity> Holders = new SyncList<NetworkIdentity>();
        public NetworkConnectionToClient ConnectionToClient { get; set; }
        public LMBReaction Reaction => reaction;
        public Rigidbody[] GetHandPoints() => handleType == HandleType.OneHanded 
            ? new[] { universalPoint } 
            : new[] { leftHandPoint, rightHandPoint };
        public Rigidbody Rigidbody => rigidBody;
        public NetworkItem Network => _network;
        public bool HasToBeAligned => hasToBeAligned;
        public Collider Collider => collider;
        public NetworkTransformReliable NetworkTransform => _networkTransform;
        public bool IsThrown { get => _isThrown; set => _isThrown = value; }

        private void Start()
        {
            _outline = GetComponent<Outline>();
            _networkTransform = GetComponent<NetworkTransformReliable>();
            reaction = LMBReactionFactory.CreateReaction(_network.itemId, this);
            
            if (TryGetComponent<CollisionDamageDealer>(out damageDealer))
                damageDealer.OnServerTakeDamage += RpcPlayParticlesOnHit;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isServer)
            {
                IsThrown = false;

                reaction?.CollisionEnter(other);
            }
        }
        
        [ClientRpc]
        private void RpcPlayParticlesOnHit()
        {
            Debug.Log("<color=yellow>PlayParticlesOnHit");
            
            _particlePool.GetAndPlayParticle(Particles.pow, transform.position);
            
        }

        // [Command(requiresAuthority = false)]
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
            if (!isServer)
            {
                PhysicalItemRegistry.Instance.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (!isServer)
            {
                PhysicalItemRegistry.Instance.Unregister(this);
            }
        }

        private void OnEnable()
        {
            if (!isServer && PhysicalItemRegistry.Instance.GetItem(gameObject) == null)
            {
                PhysicalItemRegistry.Instance.Register(this);
            }
        }
    }
}