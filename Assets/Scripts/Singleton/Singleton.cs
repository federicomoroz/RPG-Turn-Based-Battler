using UnityEngine;
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; protected set; }
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this as T;
    }
}
public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
}
