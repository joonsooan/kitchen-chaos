using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : KSingleton<CustomerSpawner>
{
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private Vector2Int spawnCell;
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private bool randomOrder = false;
    [SerializeField] private Transform poolParent;

    private float timer;
    private int sequentialIndex;
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

        GameObject prefab = randomOrder
            ? customerPrefabs[Random.Range(0, customerPrefabs.Length)]
            : customerPrefabs[sequentialIndex % customerPrefabs.Length];

        if (!randomOrder)
            sequentialIndex = (sequentialIndex + 1) % customerPrefabs.Length;

        GetOrCreatePool(prefab).Get();
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
