using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KinematicLocomotion : LocomotionModule
{
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    private Vector3 slopeHitPoint;
    private Vector3 slopeProjection;
    private float slopeAngle;

    private float _verticalVelocity;

    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        CheckDownSlope();

        if (_controller.isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += gravity * deltaTime;
        }

        if(slopeAngle > 0f && slopeAngle <= _controller.slopeLimit && _controller.isGrounded)
        {
            Velocity = slopeProjection.normalized * Velocity.magnitude;

        }
        else
        {
            Velocity.y += _verticalVelocity;
        }


        _controller.Move(Velocity * deltaTime);
    }

    private void CheckDownSlope()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red, 1f);

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

    private void SnapToSlope()
    {
        if(!_controller.isGrounded && slopeAngle > 0f && slopeAngle <= _controller.slopeLimit)
        {
            Vector3 snapDirection = Vector3.down;
            float snapDistance = groundCheckDistance;
            if(Physics.Raycast(transform.position, snapDirection, out RaycastHit hit, snapDistance, groundLayer))
            {
                _controller.Move(Vector3.down * hit.distance);
            }
        }
    }

}
