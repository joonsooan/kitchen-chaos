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
    }
}
