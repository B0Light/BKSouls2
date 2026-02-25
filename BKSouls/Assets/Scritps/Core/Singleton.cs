using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에서 해당 타입의 인스턴스를 찾습니다.
                _instance = FindAnyObjectByType<T>();

                // 인스턴스를 찾지 못했을 경우, 오류를 출력합니다.
                if (_instance == null)
                {
                    Debug.LogError("Singleton<" + typeof(T).Name + "> 인스턴스를 씬에서 찾을 수 없습니다.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("씬에 " + typeof(T).Name + " 인스턴스가 이미 존재합니다. 현재 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
        }
    }
}