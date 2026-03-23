using UnityEngine;

public class OrientationProvider : MonoBehaviour, IOrientationProvider
{
    public Transform trackedObject;

    public Vector3 Forward => GetForward();
    public Vector3 Right => GetRight();
    public Vector3 Up => GetUp();

    private Transform forwardIsolated;
    private Transform rightIsolated;
    private Transform upIsolated;

    private void Start()
    {
        forwardIsolated = new GameObject("Forward Isolated").transform;
        rightIsolated = new GameObject("Right Isolated").transform;
        upIsolated = new GameObject("Up Isolated").transform;

        forwardIsolated.SetParent(transform);
        rightIsolated.SetParent(transform);
        upIsolated.SetParent(transform);
    }

    private Vector3 GetForward()
    {
        forwardIsolated.rotation = trackedObject?.rotation ?? transform.rotation;
        forwardIsolated.localEulerAngles = new Vector3(0, forwardIsolated.localEulerAngles.y, 0);
        return forwardIsolated.forward;
    }

    private Vector3 GetRight()
    {
        rightIsolated.rotation = trackedObject?.rotation ?? transform.rotation;
        rightIsolated.localEulerAngles = new Vector3(0, rightIsolated.localEulerAngles.y, 0);
        return rightIsolated.right;
    }

    private Vector3 GetUp()
    {
        upIsolated.rotation = trackedObject?.rotation ?? transform.rotation;
        upIsolated.localEulerAngles = new Vector3(0, upIsolated.localEulerAngles.y, 0);
        return upIsolated.up;
    }
}
