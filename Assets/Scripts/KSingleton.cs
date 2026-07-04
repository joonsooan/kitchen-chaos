using UnityEngine;

public class KSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /*
     * 코드리뷰:
     * 지연초기화 주의하기
     * 
     */
    
    private static T instance;

    // 씬 로드 중 다른 오브젝트의 OnEnable이 이 매니저의 Awake보다 먼저 실행될 수 있으므로
    // (예: Seat.OnEnable → SeatManager.Instance), 미초기화 시 씬에서 직접 찾아온다.
    public static T Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<T>();
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this as T;
        //돈디스토이는 그거 쓰는 곳에서 해주기.
        //DontDestroyOnLoad(gameObject);
    }
}
