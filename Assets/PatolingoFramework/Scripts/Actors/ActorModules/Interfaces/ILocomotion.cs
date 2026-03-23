using UnityEngine;

public interface ILocomotion
{
    public void SetMovementInput(LocomotionInput input);
    public void SetOrientation(OrientationContext orientation);
}
