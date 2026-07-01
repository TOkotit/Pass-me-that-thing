using System;
using System.Collections;
using DI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    [SerializeField] private float gravity = -9.81f;
    
    [SerializeField] private GroundStateManager groundStateManager;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private Rigidbody root;
    
    [SyncVar]
    private bool isCharacterCanMove = true;
    private bool _isSprinting = false;
    private Vector3 _moveDirection;
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

    
    public override void OnStartClient()
    {
        if (!isLocalPlayer)
        {
            root.isKinematic = true;
        }
    }

    public void Move(Vector3 direction)
    {
        if (isCharacterCanMove)
            _moveDirection = direction;
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
        /*if (!isCharacterCanMove)
            return;
        root.rotation = rotation;*/
    }
    
    public void Jump()
    {
        if (!groundStateManager.IsGrounded())
            return;
        root.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        
        Debug.Log(jumpHeight * -2f * gravity);
    }
    
    public void SetSprinting(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        
        
        MoveInternal();

        if (groundStateManager.IsGrounded() && _velocity.y < 0f)
            _velocity.y = -2f;
    }
    
    private void MoveInternal()
    {
        
        if (!isCharacterCanMove)
            return;
        
        var currentSpeed = speed;

        if (_isSprinting)
            currentSpeed *= sprintMultiplier;
        
        root.AddForce(_moveDirection * currentSpeed, ForceMode.Acceleration);
    }
}
