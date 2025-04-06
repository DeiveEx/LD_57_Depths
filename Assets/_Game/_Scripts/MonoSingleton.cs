using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Fields

    private static T _instance;
    protected static bool _isQuitting;

    [Header("Singleton")] 
    [SerializeField] protected bool _lazyInitilize;
    [SerializeField] protected bool _dontDestroyOnLoad;


    #endregion

    public static T Instance
    {
        get
        {
            if (_isQuitting)
                return null;
            
            if (_instance == null)
                _instance = FindFirstObjectByType<T>();

            if (_instance == null)
            {
                var go = new GameObject(typeof(T).ToString());
                _instance = go.AddComponent<T>();
            }

            return _instance;
        }
        
        protected set => _instance = value;
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        if (!_lazyInitilize && _instance == null)
            _instance = FindFirstObjectByType<T>();

        if(_dontDestroyOnLoad)
            DontDestroyOnLoad(this.gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
