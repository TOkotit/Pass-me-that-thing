
using Game.Entity;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class PhysicalItem : NetworkBehaviour
    {
        [SerializeField] private float hardness;
        public float Hardness => hardness;
        [SyncVar]
        [SerializeField] private int durability;
        public int Durability {get => durability; set => durability = value; }
        [SerializeField] private HandleType handleType;
        public HandleType HandleType => handleType;
        [SerializeField] private Rigidbody universalPoint;
        public Rigidbody UniversalPoint => universalPoint;
        [SerializeField] private Rigidbody leftHandPoint;
        public Rigidbody LeftHandlPoint => leftHandPoint;
        [SerializeField] private Rigidbody rightHandPoint;
        public Rigidbody RightHandPoint => rightHandPoint;
        [SerializeField] private Vector3 defaultPosition;
        public Vector3 DefaultPosition => defaultPosition;
        
        [SerializeField] private bool canBeOwned;
        public bool CanBeOwned => canBeOwned;
        
        private MainCharacter owner;
        public MainCharacter Owner {get => owner; set => owner = value;}
        
        
        private LMBReaction reaction;
        public LMBReaction Reaction => reaction;
        public Rigidbody[] GetHandPoints() => handleType == HandleType.OneHanded 
            ? new[] { universalPoint } 
            : new[] { leftHandPoint, rightHandPoint };
        [SerializeField] private Rigidbody rigidBody;
        public Rigidbody Rigidbody => rigidBody;
        
        private Outline _outline;
        public Outline Outline => _outline;
        
        [SerializeField] private NetworkItem _network;
        public NetworkItem Network => _network;
        public bool IsThrown { get; set; }
        
        private NetworkTransformReliable _networkTransform;
        public NetworkTransformReliable NetworkTransform => _networkTransform;
        private Collider[] _colliders;
        public Collider[] Colliders => _colliders;
        
        [Inject]
        private void Construct(PhysicalItemRegistry physicalItemRegistry)
        {
            physicalItemRegistry.Register(this);
        }

        private void Start()
        {
            /*if (Network.netIdentity.connectionToClient != null)
                Network.netIdentity.AssignClientAuthority(all);*/
            _outline = GetComponent<Outline>();
            _networkTransform = GetComponent<NetworkTransformReliable>();
            _colliders = GetComponentsInChildren<Collider>();
        }

        private void OnCollisionEnter(Collision other)
        {
            IsThrown = false;
        }
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
            if (!isServer && PhysicalItemRegistry.Instance.TryGetItem(gameObject) == null)
            {
                PhysicalItemRegistry.Instance.Register(this);
            }
        }
    }
}