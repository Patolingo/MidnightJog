using UnityEngine;

public struct OrientationContext
{
    public Vector3 Forward { get; }
    public Vector3 Right { get; }
    public Vector3 Up { get; }

    public OrientationContext(Transform transform)
    {
        Forward = transform.forward;
        Right = transform.right;
        Up = transform.up;
    }
    public OrientationContext(Vector3 forward, Vector3 right, Vector3 up)
    {
        Forward = forward;
        Right = right;
        Up = up;
    }

    public static OrientationContext Identity => new OrientationContext(Vector3.forward, Vector3.right, Vector3.up);
}
