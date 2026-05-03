using System;
using System.Collections;
using DI;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using MainCharacter;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainCharacterMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 140f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [SerializeField] private GroundStateManager groundStateManager;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private Rigidbody root;
    
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
        // root =  GetComponent<Rigidbody>();
    }

    
    public override void OnStartClient()
    {
        if (!isServer)
        {
            root.isKinematic = true;
        }
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
        var grounded = groundStateManager.IsGrounded();
        if (!grounded) return;
        
        root.AddForce( new Vector3(0,jumpHeight, 0), ForceMode.Impulse);
        Debug.Log(jumpHeight * -2f * gravity);
    }
    
    public void SetSprinting(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    
    private void FixedUpdate()
    {
        
        if (!isServer) return;
        
        
        MoveInternal();

        if (groundStateManager.IsGrounded() && _velocity.y < 0f)
            _velocity.y = -2f;
    }
    
    private void MoveInternal()
    {
        var currentSpeed = speed;

        if (_isSprinting)
            currentSpeed *= sprintMultiplier;
        
        root.AddForce(_moveDirection * currentSpeed, ForceMode.Acceleration);
        root.AddForce(Vector3.up * gravity);
    }
}
