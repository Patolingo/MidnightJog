using System.Collections.Generic;
using UnityEngine;

public abstract class LocomotionModule : BlockableMonoBehaviour, ILocomotion, ITick
{
    [Header("Settings")]
    [SerializeField] protected float movementSpeed = 5;
    [SerializeField] protected float sprintMultiplier = 1.5f;
    [SerializeField] protected float maxSpeed = 1.5f;
    [Space]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;


    private float _currentMovementSpeed;
    public float TargetSpeed => _isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;

    private bool _isSprinting;

    protected Vector3 MoveDirection;
    protected Vector3 Velocity;

    protected Vector3 lastMovementDirection;

    protected OrientationContext _orientationContext = OrientationContext.Identity;

    public virtual void SetMovementInput(LocomotionInput input)
    {
        Vector3 direction = input.MoveDirection;
        _isSprinting = input.isSprinting;

        direction.Normalize();
        if(direction != Vector3.zero)
        {
            lastMovementDirection = direction;
        }
        MoveDirection = direction;
    }
    public virtual void SetOrientation(OrientationContext orientation)
    {
        _orientationContext = orientation;
    }

    public virtual void Tick(float deltaTime)
    {
        float targetSpeed = TargetSpeed;

        if (MoveDirection.magnitude > 0.1f)
        {
            _currentMovementSpeed = Mathf.MoveTowards(_currentMovementSpeed, targetSpeed, acceleration * deltaTime);
        }
        else
        {
            _currentMovementSpeed = Mathf.MoveTowards(_currentMovementSpeed, 0, deceleration * deltaTime);
        }

        Velocity = (lastMovementDirection.x * _orientationContext.Right + lastMovementDirection.z * _orientationContext.Forward) * _currentMovementSpeed;

        if(Velocity.magnitude > maxSpeed)
        {
            Velocity = Velocity.normalized * maxSpeed;
        }
    }
}

public struct LocomotionInput
{
    public Vector3 MoveDirection;
    public bool isSprinting;

    public LocomotionInput(Vector3 moveDirection, bool isSprinting)
    {
        MoveDirection = moveDirection;
        this.isSprinting = isSprinting;
    }
}
