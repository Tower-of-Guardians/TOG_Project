using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            // 애플리케이션이 종료 중일 때 접근하면 null 반환 (Destroy된 오브젝트에 접근 방지)
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T).Name}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            // 스레드 안전성을 위한 lock
            lock (_lock)
            {
                // 인스턴스가 아직 없으면 생성
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = $"(Singleton) {typeof(T).Name}";
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[Singleton] Destroying duplicate instance of {typeof(T).Name} on GameObject '{gameObject.name}'.");
            Destroy(gameObject);
        }
        else if (_instance == null)
        {
            _instance = this as T;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
        _instance = null;
    }

    protected virtual void OnDestroy()
    {
        if (!_applicationIsQuitting && _instance == this)
        {
            _instance = null;
        }
    }
}