using UnityEngine;
using UnityEngine.UI;

// 손님 머리 위 주문 말풍선 — 요청 메뉴 아이콘만 표시.
// 서빙 완료/실패로 떠나기 시작하면(또는 손님이 사라지면) 자동 제거.
public class OrderBubbleView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.75f, 0f);

    private Customer target;
    private Camera cam;

    public void Bind(Customer customer)
    {
        target = customer;
        cam = Camera.main;

        var recipe = customer != null && customer.CustomerData != null
            ? customer.CustomerData.requiredRecipe
            : null;
        if (icon != null && recipe != null)
            icon.sprite = recipe.recipeIcon;
    }

    private void LateUpdate()
    {
        // 손님 파괴/풀 복귀 포함 — 어떤 경로로든 사라지면 같이 제거
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.transform.position + worldOffset;

        if (cam != null)
            transform.forward = cam.transform.forward;

        // 음식 조달(성공)·실패로 떠나는 순간까지 유지
        if (target.CurrentState == CustomerState.LeavingSuccess ||
            target.CurrentState == CustomerState.LeavingFailure)
        {
            Destroy(gameObject);
        }
    }
}
