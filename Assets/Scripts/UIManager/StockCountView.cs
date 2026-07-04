using TMPro;
using UnityEngine;

// 퇴식구(컵/접시 반납소) 위 남은 그릇 수 표시 — CurrentStock 폴링.
// OrderUIBridge가 씬의 ReturnStation마다 하나씩 생성.
public class StockCountView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, -0.75f, 0f);

    private ReturnStation target;
    private int lastShown = int.MinValue;

    public void Bind(ReturnStation station)
    {
        target = station;
        transform.position = station.transform.position + worldOffset;
    }

    private void LateUpdate()
    {
        if (target == null) { Destroy(gameObject); return; }
        if (countText == null) return;

        int stock = target.CurrentStock;
        if (stock == lastShown) return;
        lastShown = stock;

        countText.text = stock.ToString();
        countText.color = stock > 0 ? Color.white : new Color(0.95f, 0.35f, 0.25f);   // 0개면 빨강
    }
}
