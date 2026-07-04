using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : KSingleton<CustomerSpawner>
{
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private Vector2Int spawnCell;
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private float[] spawnWeights;   // customerPrefabs와 1:1. 비었거나 짧으면 해당 항목 가중치 1로 취급.
    [SerializeField] private Transform poolParent;

    private float timer;
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> pools = new();

    public Vector2Int SpawnCell => spawnCell;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        timer -= spawnInterval;
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
}
