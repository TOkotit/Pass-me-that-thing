using System;
using System.Collections;
using DI;
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
    
    private MainCharacterModel _model => character.MainCharacterModel;
    
    [SyncVar]
    private bool isCharacterCanMove = true;
    private bool _isSprinting = false;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _lastWaterDrop;
    private float _movementMultiplier = 1.0f;
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
            character.Fall(2);
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

    
    public override void OnStartClient()
    {
        if (!isLocalPlayer)
        {
            characterController.enabled = false;
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
            character.Fall(3);
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
