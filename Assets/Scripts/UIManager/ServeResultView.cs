using DG.Tweening;
using UnityEngine;

// TableServing.OnDishServed → 테이블 위 서빙 결과 플로팅 스폰.
// static 이벤트라 씬에 하나만 두면 됨 (노션 규칙). OrderUIBridge 옆에 붙여도 됨.
public class ServeResultView : MonoBehaviour
{
    private const string PopupPrefabPath = "UI/World/ServeResultPopup";

    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);
    [SerializeField] private Color successColor = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color failColor    = new Color(0.9f, 0.3f, 0.3f);

    private ServeResultPopup popupPrefab;

    private void Awake()
    {
        popupPrefab = Resources.Load<GameObject>(PopupPrefabPath)?.GetComponent<ServeResultPopup>();
        if (popupPrefab == null)
            Debug.LogWarning($"[ServeResultView] 프리팹 못 찾음: {PopupPrefabPath}");
    }

    private void OnEnable()  => TableServing.OnDishServed += HandleDishServed;
    private void OnDisable() => TableServing.OnDishServed -= HandleDishServed;

    private void HandleDishServed(Table table, Customer customer, RecipeData recipe, bool succeeded)
    {
        if (popupPrefab == null || table == null) return;

        // recipe는 실패(미완성/오배달) 시 null일 수 있음
        string text = succeeded && recipe != null
            ? $"+{recipe.moneyReward * (GameManager.Instance != null ? GameManager.Instance.RewardMultiplier : 1)}"
            : "실패...";

        var popup = Instantiate(popupPrefab);
        popup.Show(table.transform.position + worldOffset, text, succeeded ? successColor : failColor);

        if (succeeded) FlyCoins(table.transform.position);
    }

    // 테이블에서 HUD 코인 카운터로 동전 날리기
    private void FlyCoins(Vector3 worldPos)
    {
        if (UIManager.Instance == null || Camera.main == null) return;

        var hud = UIManager.Instance.ShowHUDUI<InGameHUD>();
        if (hud == null || hud.CoinAnchor == null) return;

        var coinPrefab = Resources.Load<GameObject>("UI/HUD/CoinFly");
        if (coinPrefab == null) return;

        Vector3 start  = Camera.main.WorldToScreenPoint(worldPos);
        Vector3 target = hud.CoinAnchor.position;   // Overlay 캔버스 = 스크린 좌표

        for (int i = 0; i < 3; i++)
        {
            var coin = Instantiate(coinPrefab, hud.CoinAnchor.root);
            coin.transform.position = start + (Vector3)(Random.insideUnitCircle * 40f);

            // 핑글핑글 — X스케일 플립(동전 뒤집힘) + 시계방향 회전, 나는 내내 지속
            coin.transform.DOScaleX(-1f, 0.16f)
                .SetLoops(-1, DG.Tweening.LoopType.Yoyo)
                .SetEase(DG.Tweening.Ease.InOutSine)
                .SetLink(coin);
            coin.transform.DORotate(new Vector3(0f, 0f, -360f), 0.5f, DG.Tweening.RotateMode.FastBeyond360)
                .SetLoops(-1, DG.Tweening.LoopType.Incremental)
                .SetEase(DG.Tweening.Ease.Linear)
                .SetLink(coin);

            // 잠깐 제자리에서 돌다가(0.25초) 카운터로 날아감
            coin.transform.DOMove(target, 0.55f)
                .SetDelay(0.25f + i * 0.07f)
                .SetEase(DG.Tweening.Ease.InCubic)
                .SetLink(coin)
                .OnComplete(() =>
                {
                    Destroy(coin);
                    hud.CoinAnchor.DOKill();
                    hud.CoinAnchor.localScale = Vector3.one;
                    hud.CoinAnchor.DOPunchScale(Vector3.one * 0.2f, 0.15f, 5, 0.5f)
                       .SetLink(hud.CoinAnchor.gameObject);
                });
        }
    }
}
