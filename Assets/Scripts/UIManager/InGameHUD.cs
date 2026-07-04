using System.Collections.Generic;
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

    private readonly Dictionary<Customer, UISlot> activeSlots = new Dictionary<Customer, UISlot>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        timeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    private void OnEnable()
    {
        Customer.OnAnyCustomerSeated += HandleCustomerSeated;
        Customer.OnAnyCustomerLeft += HandleCustomerLeft;
    }

    private void OnDisable()
    {
        Customer.OnAnyCustomerSeated -= HandleCustomerSeated;
        Customer.OnAnyCustomerLeft -= HandleCustomerLeft;
    }

    // 손님이 착석해 주문을 넣는 시점 — 주문 슬롯 추가
    private void HandleCustomerSeated(Customer customer)
    {
        if (activeSlots.ContainsKey(customer)) return;
        activeSlots[customer] = AddOrder(customer.CustomerData);
    }

    // 손님이 퇴장하는 시점(성공/실패 모두) — 해당 주문 슬롯 제거
    private void HandleCustomerLeft(Customer customer)
    {
        if (!activeSlots.TryGetValue(customer, out UISlot slot)) return;

        activeSlots.Remove(customer);
        if (slot != null) Destroy(slot.gameObject);
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
}
