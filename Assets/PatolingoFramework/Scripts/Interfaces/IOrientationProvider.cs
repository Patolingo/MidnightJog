using UnityEngine;

public interface IOrientationProvider 
{
    public Vector3 Right { get; }
    public Vector3 Forward { get; }
    public Vector3 Up { get; }
}
