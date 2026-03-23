using UnityEngine;

public class CameraHandler : MonoBehaviour, ICamera
{
    [SerializeField] private float _sensitivity = 1f;

    [SerializeField] private CursorLockMode _startCursorLockMode = CursorLockMode.Locked;

    public bool bRotateParentYAxis = false;

    public Transform followTransform;
    public Vector3 viewOffset;

    private Camera m_Camera;

    private float _pitch;
    private float _yaw;

    private float _pitchLimit = 90f;

    public bool IsBoundToActor => followTransform != null;

    private void Awake()
    {
        InitializeCamera();
    }


    private void InitializeCamera()
    {
        Camera mainCamera = Camera.main;

        if (GetComponentInChildren<Camera>() != null)
        {
            m_Camera = GetComponentInChildren<Camera>();
        }
        else if (mainCamera == null || mainCamera.transform.parent != null)
        {
            GameObject cameraGameObject = new GameObject("Camera");
            m_Camera = cameraGameObject.AddComponent<Camera>();
        }
        else
        {
            m_Camera = mainCamera;
        }

        m_Camera.transform.SetParent(transform);
        m_Camera.transform.localPosition = Vector3.zero;
        Cursor.lockState = _startCursorLockMode;
    }

    public void AddPitchInput(float pitch) // Y Axis
    {
        _pitch += pitch * _sensitivity;
        _pitch = Mathf.Clamp(_pitch, -_pitchLimit, _pitchLimit);
    }
    public void AddYawInput(float yaw) // X Axis
    {
        _yaw += yaw * _sensitivity;
    }

    public void SetPitchAndYaw(Quaternion newRotation)
    {
        Vector3 eulerRotation = newRotation.eulerAngles;
        _pitch = eulerRotation.x;
        _yaw = eulerRotation.y;
    }

    private void Update()
    {
        if(followTransform != null)
        {
            transform.position = followTransform.position + viewOffset;
        }
        else
        {
            transform.position = viewOffset;
        }
    }

    private void LateUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        m_Camera.transform.rotation = targetRotation;

        if(bRotateParentYAxis && followTransform != null)
        {
            followTransform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }
    }

    public void ChangeCursorLockState(CursorLockMode newLockMode)
    {
        Cursor.lockState = newLockMode;
    }


    public static CameraHandler CreateOrFindCameraHandler()
    {
        CameraHandler existingHandler = FindAnyObjectByType<CameraHandler>();
        if (existingHandler != null && existingHandler.IsBoundToActor == false)
        {
            return existingHandler;
        }

        GameObject handlerGameObject = new GameObject("CameraHandler");
        return handlerGameObject.AddComponent<CameraHandler>();
    }
}
