using System;
using System.Collections;
using Ami.BroAudio;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using MainCharacter_old;
using Mirror;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainCharacterMovement : NetworkBehaviour
{
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private MainCharacter character;
    [SerializeField] private float maxHoldDistance = 2.0f;
    [SerializeField] private float holdSoftZone = 1.5f;
    [SerializeField] private PhysicalItemInteractionController _itemController;
    
    
    [SerializeField] private SoundSource footstepSound;
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [SerializeField] private float sprintFootstepInterval = 0.3f;
    private float _footstepTimer;
    
    
    [Inject] private DamagableRegistry _damagableRegistry;
    private MainCharacterModel _model => character.MainCharacterModel;
    public Vector3 CurrentVelocity => characterController ? characterController.velocity : Vector3.zero;

    
    [SyncVar]
    private bool isCharacterCanMove = true;
    private bool _isSprinting = false;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _lastWaterDrop;
    private float _movementMultiplier = 1.0f;
    private Vector3 _lastVelocity;
    private PhysicalItem _item;
    public Vector3 LastVelocity => _lastVelocity;
    public void DisableController() => characterController.enabled = false;
    public void EnableController() => characterController.enabled = true;

    private void Awake()
    {
        groundCheck.OnWaterTouched += OnWaterTouched;
        groundCheck.OnRunningOnItem += OnRunningOnItem;
    }

    private void OnRunningOnItem()
    {
        if (_isSprinting)
        {
            character.CmdFall(2, new Vector3());
        }
    }
    public Vector3 GetCurrentVelocity()
    {
        var velocity = _moveDirection * (_isSprinting ? _model.Speed * _model.SprintMultiplier : _model.Speed);
        velocity.y = _velocity.y; 
        return velocity;
    }
    public void SetMovementMultiplier(PhysicalItem item)
    {
        _movementMultiplier = Mathf.Min(1f, _model.BaseCarry / item.Rigidbody.mass);
        _item = item;
    }

    public void ResetMovementMultiplier()
    {
        _movementMultiplier = 1;   
        _item = null;
    }
    
    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }


    public Vector3 MoveDirection
    {
        get => _moveDirection;
        set => _moveDirection = value;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_damagableRegistry.TryGetDamagable(hit.gameObject, out var damagable) && damagable is MainCharacter otherCharacter)
        {
            var relativeVelocity = otherCharacter.Movement.LastVelocity - LastVelocity;
            var impactSpeed = Vector3.Dot(relativeVelocity, hit.normal);
            Debug.Log($"<color=red>[ERROR]</color>! Impact with {hit.gameObject.name}! {impactSpeed}");
            if (impactSpeed > 25f)
            {
                var stunDuration = Mathf.Min(impactSpeed / 5f, 5f);
                character.CmdFall(stunDuration, new Vector3());
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"<color=red>[ERROR]</color> Impact with {collision.gameObject.name}");
        if (_itemController?.CurrentHeldItem?.Collider == collision.collider) return;
        var impactSpeed = 0f;
        if (collision.rigidbody)
        {
            impactSpeed = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
        }
        else
        {
            return;
        }
        
        if (impactSpeed > 10f)
        {
            var stunDuration = Mathf.Min(impactSpeed / 5f, 5f);
            character.CmdFall(stunDuration,new Vector3());
        }
    }

   public override void OnStartClient()
    {
        if (!isLocalPlayer)
        {
            //characterController.enabled = false;
        }
    }

    public void Move(Vector3 direction)
    {
        if (isCharacterCanMove)
            _moveDirection = direction;
    }

    private void OnWaterTouched()
    {
        if (Time.time - _lastWaterDrop > 7f)
        {
            character.CmdFall(3, new Vector3());
            _lastWaterDrop = Time.time;
        }
    }
    public void LockUpMovement()
    {
        isCharacterCanMove = false;
        _moveDirection = Vector3.zero;
        Debug.Log("MOVE LOCKED");
    }

    public void UnlockMovement()
    {
        isCharacterCanMove = true;
        Debug.Log("MOVE UNLOCKED");
    }
    
    
    public void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    
    public void Jump()
    {
        if (groundCheck.IsGrounded)
            _velocity.y = _model.JumpHeight;
    }
    
    public void SetSprinting(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer || !characterController.enabled) return;
        var holderCount = Mathf.Max(1, _item ? _item.Holders.Count : 1);
        var currentSpeed = _model.Speed * (_movementMultiplier / holderCount);if (_isSprinting) currentSpeed *= _model.SprintMultiplier;
        var horizontalVelocity = _moveDirection.normalized * currentSpeed;
        _lastVelocity = horizontalVelocity + Vector3.up * _velocity.y;
        
        MoveInternal();
        ApplyGravity();
        HandleFootsteps();
        if (groundCheck.IsGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }
    
    private void MoveInternal()
    {
        if (!isCharacterCanMove || !characterController.enabled) return;

        var desiredMove = _moveDirection;

        if (_itemController && _itemController.CurrentHeldItem && !_itemController.CurrentHeldItem.CanBeOwned && _itemController.HandsMovement)
        {
            var grabWorldPos = _itemController.CurrentHeldItem.transform.TransformPoint(
                _itemController.HandsMovement.LocalPoint);
            var charPos = transform.position;
            var currentDist = Vector3.Distance(charPos, grabWorldPos);

            if (currentDist > holdSoftZone)
            {
                var dirAway = (charPos - grabWorldPos).normalized;
                var awayComponent = Vector3.Dot(desiredMove, dirAway);
                if (awayComponent > 0)
                {
                    var t = Mathf.InverseLerp(holdSoftZone, maxHoldDistance, currentDist);
                    var factor = 1f - t;
                    desiredMove = desiredMove - dirAway * awayComponent + dirAway * (awayComponent * factor);
                }
            }
        }
        var holderCount = Mathf.Max(1, _item ? _item.Holders.Count : 1);
        var currentSpeed = _model.Speed * (_movementMultiplier / holderCount);
        if (_isSprinting) currentSpeed *= _model.SprintMultiplier;

        characterController.Move(desiredMove * (currentSpeed * Time.fixedDeltaTime));
    }
    
    private void ApplyGravity()
    {
        if (!isCharacterCanMove || !characterController.enabled) return;
        _velocity += Vector3.down * (_model.Gravity * Time.fixedDeltaTime);
        characterController.Move(_velocity * Time.fixedDeltaTime);
    }
    
    
    private void HandleFootsteps()
    {
        var isMoving = _moveDirection.sqrMagnitude > 0.01f;
    
        if (groundCheck.IsGrounded && isMoving)
        {
            _footstepTimer -= Time.fixedDeltaTime;
            if (_footstepTimer <= 0f)
            {
                CmdPlayFootstepSound();
                _footstepTimer = _isSprinting ? sprintFootstepInterval : walkFootstepInterval;
            }
        }
        else
        {
            _footstepTimer = 0f; 
        }
    }
    
    [Command]
    private void CmdPlayFootstepSound()
    {
        RpcPlayFootstepSound();
    }

    [ClientRpc]
    private void RpcPlayFootstepSound()
    {
        if (footstepSound) footstepSound.Play();
    }
}
