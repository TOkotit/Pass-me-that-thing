using System;
using Game.Scripts.Enums;
using Mirror;
using Systems;
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
        [SyncVar]
        [SerializeField] private bool isHeld;
        public bool IsHeld {get => isHeld; set => isHeld = value; }
        private LMBReaction reaction;
        public LMBReaction Reaction => reaction;
        public Rigidbody[] GetHandPoints() => handleType == HandleType.OneHanded 
            ? new[] { universalPoint } 
            : new[] { leftHandPoint, rightHandPoint };
        [SerializeField] private Rigidbody rigidBody;
        public Rigidbody Rigidbody => GetComponent<Rigidbody>();
        [Inject]
        private void Construct(PhysicalItemRegistry physicalItemRegistry)
        {
            physicalItemRegistry.Register(this);
        }
    }
}