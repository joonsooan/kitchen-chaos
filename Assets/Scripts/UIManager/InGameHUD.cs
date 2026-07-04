using TMPro;
using UnityEngine;

public class InGameHUD : UIHUD
{
    
    enum Texts
    {
        TimeText,
    }

    enum GameObjects
    {
        OrderLayout,
    }

    private const string OrderPrefabPath = "UI/Slot/OrderSlot";

    private TextMeshProUGUI timeText;
    private Transform orderLayout;
    private float elapsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        timeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;

        AddOrder();
        AddOrder();
    }

    // 주문 1개 추가 — 게임 로직에서 호출
    public UISlot AddOrder(CustomerData customer)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), orderLayout);
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
        slot.Setup(customer);
        return slot;
    }

    // Update is called once per frame
    void Update()
    {
        //시간따라 mm:ss
        elapsed += Time.deltaTime;

        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // 풀에서 랜덤 손님 1명으로 주문 추가 (test용도)
    public void AddOrder()
    {
        var customers = DataTable.Customers;
        if (customers == null || customers.Length == 0) return;

        var customer = customers[Random.Range(0, customers.Length)];
        AddOrder(customer);
    }
}
