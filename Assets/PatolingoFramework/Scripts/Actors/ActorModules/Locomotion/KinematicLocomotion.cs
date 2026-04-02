using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KinematicLocomotion : LocomotionModule
{
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [Space]
    [SerializeField] private float stairsSpeed;
    [SerializeField] private LayerMask stairsLayer;

    private Vector3 slopeHitPoint;
    private Vector3 slopeProjection;
    private float slopeAngle;

    private float _verticalVelocity;

    private bool isOnStairs;

    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        CheckDownSlope();

        Vector3 finalVelocity = Velocity;

        float extraSnap = _isSprinting ? 0.2f : 0f;
        
        bool hasGroundBelow = Physics.Raycast(
            transform.position + Vector3.down + Vector3.up * 0.2f,
            Vector3.down,
            out RaycastHit hit,
            _controller.stepOffset + 0.3f + extraSnap,
            groundLayer
        );

        isOnStairs = Physics.Raycast(
            transform.position + Vector3.down + Vector3.up * 0.2f,
            Vector3.down,
            out RaycastHit stairsHit,
            _controller.stepOffset + 0.3f + extraSnap + 1,
            stairsLayer
        );


        bool isGroundedStable = _controller.isGrounded || hasGroundBelow;



        if (isGroundedStable)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += gravity * deltaTime;
        }

        if (slopeAngle > 0f && slopeAngle <= _controller.slopeLimit && isGroundedStable)
        {
            finalVelocity = slopeProjection.normalized * finalVelocity.magnitude;
        }

        finalVelocity.y = _verticalVelocity;

        if (hasGroundBelow && _verticalVelocity <= 0f)
        {
            float distance = hit.distance;

            if (distance > 0.05f && distance <= _controller.stepOffset + 0.2f + extraSnap)
            {
                float snapForce = distance / deltaTime;
                finalVelocity.y = -snapForce;
            }
        }


        int steps = Mathf.CeilToInt(finalVelocity.magnitude * deltaTime / 0.2f);
        steps = Mathf.Clamp(steps, 1, 5);

        Vector3 stepMove = (finalVelocity * deltaTime) / steps;

        for (int i = 0; i < steps; i++)
        {
            _controller.Move(stepMove);
        }
    }

    private void CheckDownSlope()
    {
       // Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red, 1f);

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            Vector3 orientationAndLastDir = _orientationContext.Forward * lastMovementDirection.z + _orientationContext.Right * lastMovementDirection.x;

            slopeProjection = Vector3.ProjectOnPlane(orientationAndLastDir, hit.normal);

            slopeAngle = Vector3.Angle(Vector3.up, hit.normal);

            slopeHitPoint = hit.point;
        }
        else
        {
            slopeAngle = 0f;
            slopeProjection = Vector3.zero;
        }

        
    }

    protected override float GetTargetSpeed()
    {
        float targetSpeed = base.GetTargetSpeed();

        if(isOnStairs)
        {
            targetSpeed = stairsSpeed;
        }

        Debug.Log($"Target Speed: {targetSpeed}");

        return targetSpeed;
    }
}
