using System;
using System.Collections;
using DI;
using MainCharacter;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[RequireComponent(typeof(CharacterController))]
public class MainCharacterMovement : NetworkBehaviour
{
    private CharacterController _controller;
    
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    private bool isCharacterCanMove = true;
    [SyncVar]
    private bool _isSprinting = false;
    
    [SyncVar]
    private Vector3 _moveDirection;
    [SyncVar]
    private Vector3 _velocity;
    

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
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    
    public override void OnStartClient()
    {
        if (!isServer)
            _controller.enabled = false;
    }

    [Server]
    public void Move(Vector3 direction)
    {
        if (isCharacterCanMove)
            _moveDirection = direction;
    }

    [Server]
    public void LockUpMovement()
    {
        isCharacterCanMove = false;
    }

    [Server]
    public void UnlockMovement()
    {
        isCharacterCanMove = true;
    }
    
    
    [Server]
    public void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
    
    [Server]
    public void Jump()
    {
        var grounded = IsGrounded();
        if (!grounded) return;
        
        _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    
    public void SetSprinting(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    
    private void FixedUpdate()
    {
        
        if (!isServer) return;
        
        
        MoveInternal();
        ApplyGravity();

        if (IsGrounded() && _velocity.y < 0f)
            _velocity.y = -2f;
    }
    
    private void MoveInternal()
    {
        var currentSpeed = speed;

        if (_isSprinting)
            currentSpeed *= sprintMultiplier;

        _controller.Move(_moveDirection * (currentSpeed * Time.fixedDeltaTime));
    }
    
    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.fixedDeltaTime;
        _controller.Move(_velocity * Time.fixedDeltaTime);
    }

    private bool IsGrounded()
    {
        return groundCheck && Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
    
}
