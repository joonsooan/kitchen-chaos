using System.Collections.Generic;
using UnityEngine;

// 씬의 모든 CookingStation에 구독 — 조리 시작 시 진행바 스폰.
// 스테이션은 씬 고정 배치라 Start에서 1회 수집 (노션 규칙: 인스턴스 이벤트 구독/해제 짝).
public class CookingGaugeSpawner : MonoBehaviour
{
    private const string GaugePrefabPath = "UI/World/CookingGauge";

    private CookingGaugeView gaugePrefab;
    private readonly List<CookingStation> stations = new();

    private void Start()
    {
        gaugePrefab = Resources.Load<GameObject>(GaugePrefabPath)?.GetComponent<CookingGaugeView>();
        if (gaugePrefab == null)
        {
            Debug.LogWarning($"[CookingGaugeSpawner] 프리팹 못 찾음: {GaugePrefabPath}");
            return;
        }

        foreach (var station in FindObjectsByType<CookingStation>(FindObjectsSortMode.None))
        {
            stations.Add(station);
            station.OnCookingStarted += HandleCookingStarted;
        }
    }

    private void OnDestroy()
    {
        foreach (var station in stations)
        {
            if (station != null)
                station.OnCookingStarted -= HandleCookingStarted;
        }
    }

    private void HandleCookingStarted(CookingStation station)
    {
        Instantiate(gaugePrefab).Bind(station);
    }
}
