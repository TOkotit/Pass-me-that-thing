using System;
using System.Collections;
using DI;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using MainCharacter_old;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainCharacterMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 140f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 9.81f;
    
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private CharacterController characterController;
    
    [SerializeField] private MainCharacter character;
    
    [SyncVar]
    private bool isCharacterCanMove = true;
    private bool _isSprinting = false;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _lastWaterDrop;
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
            _velocity.y = jumpHeight;
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
        
        var currentSpeed = speed;
        if (_isSprinting)
            currentSpeed *= sprintMultiplier;
        
        characterController.Move(_moveDirection * (currentSpeed * Time.fixedDeltaTime));
    }
    
    private void ApplyGravity()
    {
        if (!isCharacterCanMove || !characterController.enabled) return;
        _velocity += Vector3.down * (gravity * Time.fixedDeltaTime);
        characterController.Move(_velocity * Time.fixedDeltaTime);
    }
}
