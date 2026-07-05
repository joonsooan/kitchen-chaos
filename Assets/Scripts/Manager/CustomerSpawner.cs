using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : KSingleton<CustomerSpawner>
{
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private Vector2Int spawnCell;
    // 입장 경로 막힘 안내 셀 — 코드 상수로 고정(직렬화 안 되므로 씬/인스펙터 값에 절대 안 밀림).
    private static readonly Vector2Int SeatBlockedNoticeCell = new Vector2Int(15, 3);
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private float[] spawnWeights;   // customerPrefabs와 1:1. 비었거나 짧으면 해당 항목 가중치 1로 취급.
    [SerializeField] private Transform poolParent;

    private float timer;
    private bool hasSpawnedFirst;
    private int waitingCount;   // 좌석이 없어 입장을 미룬 손님 수 — 좌석이 나면 즉시 입장.
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> pools = new();

    public Vector2Int SpawnCell => spawnCell;

    private void OnEnable()
    {
        Seat.OnSeatReleased += HandleSeatReleased;
    }

    private void OnDisable()
    {
        Seat.OnSeatReleased -= HandleSeatReleased;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float threshold = hasSpawnedFirst ? spawnInterval : initialDelay;
        if (timer < threshold) return;

        timer -= threshold;
        hasSpawnedFirst = true;
        TryAdmitCustomer();
    }

    // 좌석이 있으면 바로 스폰, 없으면 스폰하지 않고 대기 인원만 늘린다.
    private void TryAdmitCustomer()
    {
        if (SeatManager.Instance != null && SeatManager.Instance.HasFreeSeat())
        {
            SpawnCustomer();
        }
        else
        {
            waitingCount++;
        }
    }

    // 좌석이 하나 비면, 대기 중인 손님이 있을 때만 그 자리로 한 명 입장시킨다.
    private void HandleSeatReleased()
    {
        if (waitingCount <= 0) return;
        if (SeatManager.Instance == null || !SeatManager.Instance.HasFreeSeat()) return;

        waitingCount--;
        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0) return;

        GameObject prefab = customerPrefabs[PickWeightedIndex()];
        GetOrCreatePool(prefab).Get();
    }

    // 가중치는 매 스폰마다 읽는다 → 나중에 시간 등에 따라 가중치를 바꾸는 로직이 생겨도
    // 이 메서드만 고치면 선택 로직은 그대로 동작한다(지금은 시간가변 로직 없음).
    private float GetWeight(int index)
    {
        if (spawnWeights != null && index < spawnWeights.Length) return Mathf.Max(0f, spawnWeights[index]);
        return 1f;
    }

    private int PickWeightedIndex()
    {
        float total = 0f;
        for (int i = 0; i < customerPrefabs.Length; i++) total += GetWeight(i);
        if (total <= 0f) return Random.Range(0, customerPrefabs.Length);   // 전부 0이면 균등

        float r = Random.value * total;                                    // [0, total) — 합이 얼마든 자동 정규화
        for (int i = 0; i < customerPrefabs.Length; i++)
        {
            r -= GetWeight(i);
            if (r < 0f) return i;
        }
        return customerPrefabs.Length - 1;                                 // 부동소수 안전망
    }

    private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out IObjectPool<GameObject> existing)) return existing;

        IObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject instance = Instantiate(prefab, poolParent);
                instance.GetComponent<Customer>().SetPool(pool);
                return instance;
            },
            actionOnGet: OnCustomerSpawned,
            actionOnRelease: instance => instance.SetActive(false),
            actionOnDestroy: instance => DestroyImmediate(instance));

        pools[prefab] = pool;
        return pool;
    }

    private void OnCustomerSpawned(GameObject instance)
    {
        instance.transform.SetPositionAndRotation(GridSystem.Instance.CellToWorld(spawnCell), Quaternion.identity);
        instance.SetActive(true);
        instance.GetComponent<CustomerMovement>().BeginSeating();
    }

    // 입장 경로가 막혀(잡초 등) 손님이 못 들어올 때, 화면 안 지정 셀에 검은 글씨로 안내.
    public void ShowSeatBlockedNotice()
    {
        var prefab = Resources.Load<GameObject>("UI/World/ServeResultPopup");
        if (prefab == null) return;

        string message = UnityEngine.Random.value < 0.5f ? "들어갈래용.." : "배고파용..";
        Instantiate(prefab).GetComponent<ServeResultPopup>()
            .Show(GridSystem.Instance.CellToWorld(SeatBlockedNoticeCell), message, Color.black);
    }
}
