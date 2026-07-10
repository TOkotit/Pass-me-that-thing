using System;
using System.Collections;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
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
    [SerializeField] private PhysicalItemInteractionController _itemController;
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
            character.CmdFall(2);
        }
    }
    public Vector3 GetCurrentVelocity()
    {
        var velocity = _moveDirection * (_isSprinting ? _model.Speed * _model.SprintMultiplier : _model.Speed);
        velocity.y = _velocity.y; 
        return velocity;
    }
    public void SetMovementMultiplier(float weight)
    {
        var multiplier = _model.BaseCarry / weight;
        _movementMultiplier = Mathf.Min(1f, _model.BaseCarry / weight);
    }

    public void ResetMovementMultiplier()
    {
        _movementMultiplier = 1;   
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
            Debug.LogError($"!Impact with {hit.gameObject.name}! {impactSpeed}");
            if (impactSpeed > 10f)
            {
                var stunDuration = Mathf.Min(impactSpeed / 5f, 5f);
                character.CmdFall(stunDuration);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogError($"Impact with {collision.gameObject.name}");
        var impactSpeed = 0f;
        if (collision.rigidbody)
        {
            impactSpeed = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
        }
        else
        {
            return;
        }

        Debug.LogError($"Impact speed: {impactSpeed} with {collision.gameObject.name}");

        if (impactSpeed > 10f)
        {
            var stunDuration = Mathf.Min(impactSpeed / 5f, 5f);
            character.CmdFall(stunDuration);
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
            character.CmdFall(3);
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
        Debug.Log("Jump: " + groundCheck.IsGrounded);
        if (groundCheck.IsGrounded)
        {
            _velocity.y = _model.JumpHeight;
        }
    }
    
    public void SetSprinting(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer || !characterController.enabled) return;
        var currentSpeed = _model.Speed * _movementMultiplier;
        if (_isSprinting) currentSpeed *= _model.SprintMultiplier;
        var horizontalVelocity = _moveDirection.normalized * currentSpeed;
        _lastVelocity = horizontalVelocity + Vector3.up * _velocity.y;
        
        MoveInternal();
        ApplyGravity();
        if (groundCheck.IsGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }
    
    private void MoveInternal()
    {
        if (!isCharacterCanMove || !characterController.enabled) return;

        var desiredMove = _moveDirection; 

        if (_itemController && _itemController.CurrentHeldItem)
        {
            var itemTransform = _itemController.CurrentHeldItem.transform;
            var itemPos = itemTransform.position;
            var charPos = transform.position;
            var currentDist = Vector3.Distance(charPos, itemPos);

            if (currentDist > maxHoldDistance)
            {
                var dirToItem = (itemPos - charPos).normalized;
                if (Vector3.Dot(desiredMove, dirToItem) < 0)
                {
                    var projected = Vector3.ProjectOnPlane(desiredMove, dirToItem);
                    desiredMove = projected.normalized * desiredMove.magnitude;
                }
            }
        }

        var currentSpeed = _model.Speed * _movementMultiplier;
        if (_isSprinting)
            currentSpeed *= _model.SprintMultiplier;

        characterController.Move(desiredMove * (currentSpeed * Time.fixedDeltaTime));
    }
    
    private void ApplyGravity()
    {
        if (!isCharacterCanMove || !characterController.enabled) return;
        _velocity += Vector3.down * (_model.Gravity * Time.fixedDeltaTime);
        characterController.Move(_velocity * Time.fixedDeltaTime);
    }
}
