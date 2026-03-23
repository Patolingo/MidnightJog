using UnityEditor;
using UnityEngine;

public class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance => GetInstance();

    public static bool InstanceExists => _instance != null;

    protected static bool IsApplicationQuitting { get; private set; }

    protected virtual void Awake()
    {
        _instance = this as T;

#if UNITY_EDITOR

        EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                IsApplicationQuitting = true;
            }
        };
#endif

        Application.quitting += () =>
        {
            IsApplicationQuitting = true;
        };

    }

    protected static T GetInstance()
    {
        if(IsApplicationQuitting)
        {
            Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on application quit. Returning null.");
            return null;
        }

        if (_instance != null)
            return _instance;

        _instance = FindAnyObjectByType<T>();

        if (_instance == null)
        {
            GameObject singletonObject = new GameObject(typeof(T).Name);
            _instance = singletonObject.AddComponent<T>();
        }

        return _instance;
    }
    
    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}

public class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }
}

public class PersistentSingleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
