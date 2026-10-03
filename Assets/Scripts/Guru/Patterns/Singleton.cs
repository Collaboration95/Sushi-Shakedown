
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T instance
    {
        get
        {
            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this as T) _instance = null;
    }

    public virtual void Awake()
    {
        RuntimeLog.Write("Singleton Awake called");

        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}